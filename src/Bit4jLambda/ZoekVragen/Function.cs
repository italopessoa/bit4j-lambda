using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleSystemsManagement;
using Amazon.SQS;
using Amazon.SQS.Model;
using Bit4j.Lambda.Core.Extensions;
using Bit4j.Lambda.Core.Factory;
using Bit4j.Lambda.Core.Model;
using Bit4j.Lambda.Core.Model.Nodes;
using Bit4j.Lambda.Core.Model.OpenTDB;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translation.V2;
using Neo4j.Driver.V1;
using Neo4j.Map.Extension.Map;
using Neo4j.Map.Extension.Model;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace ZoekVragen
{
    public class Function
    {
        private readonly bool _enableTranslation = false;
        TranslationClient _translationClient;
        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            if (Environment.GetEnvironmentVariable("ENABLE_TRANSLATION") != null)
                _enableTranslation = Environment.GetEnvironmentVariable("ENABLE_TRANSLATION").ToLowerInvariant().Equals("y");
        }


        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context);
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            string neo4jUser = "";
            string neo4jPassword = "";
            string neo4jServerIp = "";
#if DEBUG
            neo4jUser = "dev";
            neo4jPassword = "neo4j";
            neo4jServerIp = "bolt://127.0.0.1:7687";
#else
            AmazonSimpleSystemsManagementClient ssmCLient = AWSClientFactory.GetAmazonSimpleSystemsManagementClient();
            context.Logger.LogLine("LOADING CREDENTIALS");
            neo4jUser = await ssmCLient.GetParameterValueAsync("neo4j_user".ConvertToParameterRequest());
            context.Logger.LogLine("GET neo4jUser");
            neo4jPassword = await ssmCLient.GetParameterValueAsync("neo4j_password".ConvertToParameterRequest());
            context.Logger.LogLine("GET neo4jPassword");
            neo4jServerIp = await ssmCLient.GetParameterValueAsync("neo4j_server_ip".ConvertToParameterRequest());
            context.Logger.LogLine("GET neo4jServerIp");
            string gcCredentialsJson = await ssmCLient.GetParameterValueAsync("gc_translate_ns".ConvertToParameterRequest());
            GoogleCredential credential = GoogleCredential.FromJson(gcCredentialsJson);
            _translationClient = TranslationClient.Create(credential);
            context.Logger.LogLine("GET gc_translate");
            context.Logger.LogLine("LOADING CREDENTIALS DONE");
#endif

            context.Logger.LogLine($"DESERIALIZING MESSAGE {message.Body}");
            CategoryCatalog categoryCatalog = JsonConvert.DeserializeObject<CategoryCatalog>(message.Body);
            CategoryNode categoryNode = new CategoryNode
            {
                UUID = categoryCatalog.UUID
            };
            context.Logger.LogLine($"MESSAGE DESERIALIZED");

            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync($"https://opentdb.com/api_token.php?command=request");
            TokenRequestResult tokenRequestResult = JsonConvert.DeserializeObject<TokenRequestResult>(await response.Content.ReadAsStringAsync());

            IDriver driver = GraphDatabase.Driver(neo4jServerIp, AuthTokens.Basic(neo4jUser, neo4jPassword));

            //TODO: count by relation
            int currentTotalQuestions = 1000;
            int amount = 50;

            #region GET QUESTIONS

            List<QuestionNode> questions = new List<QuestionNode>();
            if (categoryCatalog.CategoryCount.Total < currentTotalQuestions)
            {
                while (amount > 0)
                {
                    string url = $"https://opentdb.com/api.php?amount={amount}&category={categoryCatalog.Id}&token={tokenRequestResult.Token}&encode=url3986";
                    response = await httpClient.GetAsync(url);

                    QuestionRequestResult result = JsonConvert.DeserializeObject<QuestionRequestResult>(await response.Content.ReadAsStringAsync());
                    questions.AddRange(result.Questions);

                    if (result.ResponseCode == ResponseCodeEnum.TokenEmpty)
                        amount -= (amount > 1 ? amount / 2 : 1);
                }
            }

            #endregion GET QUESTIONS

            context.Logger.LogLine($"OPEN NEO4J CONNECTION");
            context.Logger.LogLine($"{questions.Count} QUESTIONS FOUND ON CATEGORY {categoryCatalog.Id}");

            using (ISession session = driver.Session(AccessMode.Write))
            {
                List<QuestionNode> questionsToCreate = new List<QuestionNode>();

                #region CHECKING EXISTING QUESTIONS

                for (int i = 0; i < questions.Count; i++)
                {
                    QuestionNode question = questions[i];
                    string matchQuery = question.MapToCypher(CypherQueryType.MatchProperties, new string[] { "Title", "Category" });

                    try
                    {
                        IStatementResultCursor result = await session.RunAsync(matchQuery);
                        bool newQuestion = true;
                        await result.ForEachAsync(r =>
                        {
                            newQuestion = r.Keys.Count == 0;
                        });

                        if (newQuestion)
                            questionsToCreate.Add(question);
                    }
                    catch (Exception ex)
                    {
                        context.Logger.LogLine($"=================== ERROR WHILE CHECKING EXISTING QUESTIONS =================== ");
                        context.Logger.LogLine(matchQuery);
                        context.Logger.LogLine(ex.Message);
                        context.Logger.LogLine(ex.StackTrace);
                        if (ex.InnerException != null)
                        {
                            context.Logger.LogLine(ex.InnerException.Message);
                            context.Logger.LogLine(ex.InnerException.StackTrace);
                        }

                        context.Logger.LogLine($"=================== END ERROR WHILE CHECKING EXISTING QUESTIONS =================== ");
                    }
                }

                #endregion CHECKING EXISTING QUESTIONS

                #region CREATE QUESTIONS

                context.Logger.LogLine($"START CREATING/TRANSLATING QUESTIONS");

                foreach (QuestionNode question in questionsToCreate)
                {
                    question.TitlePT = TranslateString(question.Title);

                    if (question.CorrectAnswer.GetType() == typeof(string))
                        question.CorrectAnswerPT = TranslateString(question.CorrectAnswer.ToString());

                    string createQuery = string.Empty;
                    try
                    {

                        TranslateQuestion(question);
                        createQuery = question.MapToCypher(CypherQueryType.Create);
                    }
                    catch (Exception ex)
                    {
                        context.Logger.LogLine($"=================== ERROR TO MAP CREATE QUERY =================== ");
                        context.Logger.LogLine(JsonConvert.SerializeObject(question));
                        context.Logger.LogLine(ex.Message);
                        context.Logger.LogLine(ex.StackTrace);
                        if (ex.InnerException != null)
                        {
                            context.Logger.LogLine(ex.InnerException.Message);
                            context.Logger.LogLine(ex.InnerException.StackTrace);
                        }

                        context.Logger.LogLine($"=================== ERROR TO MAP CREATE QUERY =================== ");
                    }

                    IStatementResultCursor result = await session.RunAsync(createQuery);

                    QuestionNode createdQuestion = null;
                    await result.ForEachAsync(r =>
                       {
                           createdQuestion = r[r.Keys[0]].Map<QuestionNode>();
                       });

                    if (createdQuestion == null)
                    {
                        context.Logger.LogLine("QUESTION NOT CREATED");
                        context.Logger.LogLine(createQuery);
                    }

                    try
                    {
                        QuestionCategoryRelationNode relationNode = new QuestionCategoryRelationNode(createdQuestion, categoryNode);
                        string createRelationQuery = relationNode.CreateRelationQuery();
                        result = await session.RunAsync(createRelationQuery);
                    }
                    catch (Exception ex)
                    {
                        context.Logger.LogLine($"=================== ERROR TO CREATE QUESTION =================== ");
                        context.Logger.LogLine(createQuery);
                        context.Logger.LogLine(ex.Message);
                        context.Logger.LogLine(ex.StackTrace);
                        if (ex.InnerException != null)
                        {
                            context.Logger.LogLine(ex.InnerException.Message);
                            context.Logger.LogLine(ex.InnerException.StackTrace);
                        }

                        context.Logger.LogLine($"=================== ERROR TO CREATE QUESTION =================== ");
                    }
                }
                context.Logger.LogLine($"FINISHED QUESTIONS CREATION");

                #endregion CREATE QUESTIONS
            }


            await Task.CompletedTask;
        }

        private void TranslateQuestion(QuestionNode question)
        {
            if (question.Type.Equals("boolean"))
            {
                question.CorrectAnswerPT = bool.TrueString.Equals(question.CorrectAnswer) ? "Verdadeiro" : "Falso";
                question.IncorrectAnswersPT = bool.TrueString.Equals(question.IncorrectAnswers[0]) ? new string[] { "Verdadeiro" } : new string[] { "Falso" };
            }

            question.TitlePT = TranslateString(question.Title);
        }

        private string TranslateString(string message)
        {
            if (_enableTranslation)
                return _translationClient.TranslateText(message, "pt", "en").TranslatedText;

            return message;
        }
    }

    public class QuestionCategoryRelationNode : RelationNode<QuestionNode, CategoryNode>
    {
        public QuestionCategoryRelationNode(QuestionNode origin, CategoryNode destiny)
            : base(origin, destiny)
        {
            RelationType = "HAS_CATEGORY";
        }
    }
}
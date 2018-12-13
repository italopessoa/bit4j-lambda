using System.Collections.Generic;
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
        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {

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
            string questionsQueueURL = "";
            string neo4jUser = "";
            string neo4jPassword = "";
            string neo4jServerIp = "";
#if DEBUG
            questionsQueueURL = "";
            neo4jUser = "neo4j";
            neo4jPassword = "bitcoinshow";
            neo4jServerIp = "bolt://127.0.0.1:7687";
#else
            AmazonSimpleSystemsManagementClient ssmCLient = AWSClientFactory.GetAmazonSimpleSystemsManagementClient();
            questionsQueueURL = await ssmCLient.GetParameterValueAsync("vragen_queue_url".ConvertToParameterRequest());
            neo4jUser = await ssmCLient.GetParameterValueAsync("neo4j_user".ConvertToParameterRequest());
            neo4jPassword = await ssmCLient.GetParameterValueAsync("neo4j_password".ConvertToParameterRequest());
            neo4jServerIp = await ssmCLient.GetParameterValueAsync("neo4j_server_ip".ConvertToParameterRequest());
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

            context.Logger.LogLine($"OPEN NEO4J CONNECTION");
            List<QuestionNode> questionsToTranslate = new List<QuestionNode>();
            using (ISession session = driver.Session(AccessMode.Write))
            {
                context.Logger.LogLine($"{questions.Count} QUESTIONS FOUND ON CATEGORY {categoryCatalog.Id}");
                List<QuestionNode> questionsToCreate = new List<QuestionNode>();

                for (int i = 0; i < questions.Count; i++)
                {
                    QuestionNode question = questions[i];
                    string matchQuery = question.MapToCypher(CypherQueryType.Match);

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
                    catch (System.Exception ex)
                    {
                        context.Logger.LogLine($"=================== ERROR WHILE CHECKING EXISTING QUESTIONS =================== ");
                        context.Logger.LogLine(matchQuery);
                        context.Logger.LogLine(ex.Message);
                        context.Logger.LogLine(ex.StackTrace);
                        if(ex.InnerException != null)
                        {
                            context.Logger.LogLine(ex.InnerException.Message);
                            context.Logger.LogLine(ex.InnerException.StackTrace);
                        }

                        context.Logger.LogLine($"=================== END ERROR WHILE CHECKING EXISTING QUESTIONS =================== ");
                    }
                }

                context.Logger.LogLine($"START CREATION");

                foreach (QuestionNode question in questionsToCreate)
                {
                    string createQuery = question.MapToCypher(CypherQueryType.Create);
                    string matchQuery = question.MapToCypher(CypherQueryType.Match);

                    IStatementResultCursor result = await session.RunAsync(createQuery);

                    result = await session.RunAsync(matchQuery);
                    QuestionNode createdQuestion = null;
                    await result.ForEachAsync(r =>
                       {
                           createdQuestion = r[r.Keys[0]].Map<QuestionNode>();
                           questionsToTranslate.Add(createdQuestion);
                       });

                    try
                    {
                        QuestionCategoryRelationNode relationNode = new QuestionCategoryRelationNode(categoryNode, createdQuestion);
                        string createRelationQuery = relationNode.CreateRelationQuery();
                        await session.RunAsync(createRelationQuery);
                    }
                    catch (System.Exception ex)
                    {
                        //throw;
                        context.Logger.LogLine("ERROR");
                        context.Logger.LogLine(ex.StackTrace);
                    }
                }
                context.Logger.LogLine($"QUESTIONS CFREATE DONE");
            }

            //AmazonSQSClient _sqsClient = AWSClientFactory.GetAmazonSQSClient();
            //SendMessageResponse sendMessageResponse = null;
            //SendMessageRequest sendMessageRequest =
            //    new SendMessageRequest
            //    {
            //        QueueUrl = questionsQueueURL
            //    };
            //foreach (QuestionNode question in questionsToTranslate)
            //{
            //    var serializer = new JsonSerializer();
            //    var stringWriter = new StringWriter();
            //    using (var writer = new JsonTextWriter(stringWriter))
            //    {
            //        writer.QuoteName = false;
            //        serializer.NullValueHandling = NullValueHandling.Ignore;
            //        serializer.Serialize(writer, question);
            //    }

            //    var json = stringWriter.ToString();
            //    sendMessageRequest.MessageBody = json;
            //    sendMessageResponse = await _sqsClient.SendMessageAsync(sendMessageRequest);
            //}

            await Task.CompletedTask;
        }
    }

    public class QuestionCategoryRelationNode : RelationNode<CategoryNode, QuestionNode>
    {
        public QuestionCategoryRelationNode(CategoryNode origin, QuestionNode destiny)
            : base(origin, destiny)
        {
            RelationType = "CATEGORY";
        }
    }
}


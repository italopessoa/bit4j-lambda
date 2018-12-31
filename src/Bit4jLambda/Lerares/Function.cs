using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SimpleSystemsManagement;
using Bit4j.Lambda.Core.Extensions;
using Bit4j.Lambda.Core.Factory;
using Bit4j.Lambda.Core.Model;
using Bit4j.Lambda.Core.Model.Nodes;

using Lerares.Model;

using Neo4j.Driver.V1;
using Neo4j.Map.Extension.Attributes;
using Neo4j.Map.Extension.Map;
using Neo4j.Map.Extension.Model;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Lerares
{
    public static class APIGatewayProxyExtensionsc
    {
        public static APIGatewayProxyResponse BadRequest(this Function function, string message)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = JsonConvert.SerializeObject(new { message }),
                Headers = new Dictionary<string, string> { { "Content-Type", "text/json" } }
            };
        }
    }

    public class Function
    {
        private const string USER_KEY = "user";
        private const string LANGUAGE_KEY = "lang";

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            LeraresResponse responseBody = new LeraresResponse();

            bool isPortuguese = false;
            string userUUID = string.Empty;
            APIGatewayProxyResponse response = new APIGatewayProxyResponse
            {
                Headers = new Dictionary<string, string> { { "Content-Type", "text/json" } }
            };

            UserAnswer userAnswer = null;

            #region validate request. toooo much I think :D

            if (request.QueryStringParameters == null || !request.QueryStringParameters.ContainsKey(USER_KEY) || string.IsNullOrEmpty(request.QueryStringParameters[USER_KEY]))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                responseBody.Error = $"Query string parameter \"{USER_KEY}\" cannot be empty";
                response.Body = JsonConvert.SerializeObject(responseBody);
                LogResponse(context, response);
                return response;
            }

            if (string.IsNullOrEmpty(request.Body))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                responseBody.Error = $"Body cannot be empty";
                response.Body = JsonConvert.SerializeObject(responseBody);
                LogResponse(context, response);
                return response;
            }

            userUUID = request.QueryStringParameters[USER_KEY];

            if (request.QueryStringParameters.ContainsKey(LANGUAGE_KEY))
                isPortuguese = request.QueryStringParameters[LANGUAGE_KEY].Equals("pt", StringComparison.InvariantCultureIgnoreCase);

            string body = request.IsBase64Encoded ? request.Body.Base64Decode() : request.Body;
            context.Logger.LogLine($"REQUEST'S BODY = {body}");

            context.Logger.LogLine("TRYING TO DESERIALIZE REQUEST BODY");
            userAnswer = JsonConvert.DeserializeObject<UserAnswer>(body);
            context.Logger.LogLine("REQUEST BODY DESERIALIZED");

            if (string.IsNullOrEmpty(userAnswer.Answer))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                responseBody.Error = $"Body property \"{nameof(userAnswer.Answer)}\" cannot be empty";
                response.Body = JsonConvert.SerializeObject(responseBody);
                LogResponse(context, response);
                return response;
            }

            if (string.IsNullOrEmpty(userAnswer.QuestionUUID))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                responseBody.Error = $"Body property \"{nameof(userAnswer.QuestionUUID)}\" cannot be empty";
                response.Body = JsonConvert.SerializeObject(responseBody);
                LogResponse(context, response);
                return response;
            }

            if (string.IsNullOrEmpty(userAnswer.UserUUID))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                responseBody.Error = $"Body property \"{nameof(userAnswer.UserUUID)}\" cannot be empty";
                response.Body = JsonConvert.SerializeObject(responseBody);
                LogResponse(context, response);
                return response;
            }

            if (string.IsNullOrEmpty(userAnswer.SelectedOption))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                responseBody.Error = $"Body property \"{nameof(userAnswer.SelectedOption)}\" cannot be empty";
                response.Body = JsonConvert.SerializeObject(responseBody);
                LogResponse(context, response);
                return response;
            }

            string[] availableOptions = new string[] { "a", "b", "c", "d" };
            if (!availableOptions.Contains(userAnswer.SelectedOption))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                responseBody.Error = $"{nameof(userAnswer.SelectedOption)} must be in {JsonConvert.SerializeObject(availableOptions)}";
                response.Body = JsonConvert.SerializeObject(responseBody);
                LogResponse(context, response);
                return response;
            }

            if (!userAnswer.UserUUID.Equals(userUUID))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                responseBody.Error = $"User UUID don't match. Check your body request";
                response.Body = JsonConvert.SerializeObject(responseBody);
                LogResponse(context, response);
                return response;
            }

            if (userAnswer == null)
            {
                context.Logger.LogLine($"ERROR DESERIALIZING MESSAGE {body}");
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                responseBody.Error = $"Internal server error. Contact the support.";
                response.Body = JsonConvert.SerializeObject(responseBody);
                LogResponse(context, response);
                return response;
            }

            #endregion validate request

            string neo4jUser = "";
            string neo4jPassword = "";
            string neo4jServerIp = "";
#if DEBUG
            neo4jUser = "neo4j";
            neo4jPassword = "quixada";
            neo4jServerIp = "bolt://localhost:7687";
#else
            context.Logger.LogLine("LOADING CREDENTIALS");
            AmazonSimpleSystemsManagementClient ssmCLient = AWSClientFactory.GetAmazonSimpleSystemsManagementClient();
            context.Logger.LogLine("LOADING neo4jUser");
            neo4jPassword = await ssmCLient.GetParameterValueAsync("neo4j_password".ConvertToParameterRequest());
            context.Logger.LogLine("LOADING neo4jPassword ");
            neo4jServerIp = await ssmCLient.GetParameterValueAsync("neo4j_server_ip".ConvertToParameterRequest());
            context.Logger.LogLine("LOADING neo4jServerIp ");
#endif
            IDriver driver = GraphDatabase.Driver(neo4jServerIp, AuthTokens.Basic(neo4jUser, neo4jPassword));

            QuestionNode question = new QuestionNode
            {
                UUID = userAnswer.QuestionUUID
            };

            UserNode user = new UserNode
            {
                UUID = userUUID
            };

            QuestionNode currentQuestion = null;
            UserNode currentUser = null;

            using (ISession session = driver.Session(AccessMode.Read))
            {
                string matchQuestionQuery = question.MapToCypher(CypherQueryType.Match);
                IStatementResultCursor result = await session.RunAsync(matchQuestionQuery);

                await result.ForEachAsync(r =>
                {
                    currentQuestion = r[r.Keys[0]].Map<QuestionNode>();
                });

                if (currentQuestion == null)
                {
                    context.Logger.LogLine($"QUESTION WITH UUID {userAnswer.QuestionUUID} NOT FOUND");
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    responseBody.Error = $"Question not found!";
                    response.Body = JsonConvert.SerializeObject(responseBody);
                    LogResponse(context, response);
                    return response;
                }

                string matchUserQuery = user.MapToCypher(CypherQueryType.Match);
                result = await session.RunAsync(matchUserQuery);

                await result.ForEachAsync(r =>
                {
                    currentUser = r[r.Keys[0]].Map<UserNode>();
                });

                if (currentUser == null)
                {
                    context.Logger.LogLine($"USER WITH UUID {userUUID} NOT FOUND");
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    responseBody.Error = $"User not found!";
                    response.Body = JsonConvert.SerializeObject(responseBody);
                    LogResponse(context, response);
                    return response;
                }

                string correctAnswer = isPortuguese ? currentQuestion.CorrectAnswerPT.ToString() : currentQuestion.CorrectAnswer.ToString();
                UserQuestionAnswerRelationNode relation = null;
                if (correctAnswer.Equals(userAnswer.Answer, StringComparison.InvariantCultureIgnoreCase))
                    relation = new CorrectAnswerRelation(currentUser, currentQuestion);
                else
                    relation = new IncorrectAnswerRelation(user, question);

                IStatementResultCursor relationResult = await session.RunAsync(relation.CreateMatchRelationQuery());
                UserQuestionAnswerRelationNode existingRelation = null;
                await relationResult.ForEachAsync(r =>
                {
                    existingRelation = r.Values.MapRelation<UserQuestionAnswerRelationNode, UserNode, QuestionNode>();
                });

                if (existingRelation != null)
                    relation = existingRelation;

                switch (userAnswer.SelectedOption)
                {
                    case "a":
                        relation.A++;
                        break;
                    case "b":
                        relation.B++;
                        break;
                    case "c":
                        relation.C++;
                        break;
                    case "d":
                        relation.D++;
                        break;
                }

                string createRelationQuery = existingRelation == null ? relation.CreateRelationQuery() : relation.CreateUpdateRelationQuery();
                context.Logger.LogLine($"EXECUTING CYPHER QUERY {createRelationQuery}");

                relationResult = await session.RunAsync(createRelationQuery);
                await relationResult.ForEachAsync(r =>
                {
                    relation = r.Values.MapRelation<UserQuestionAnswerRelationNode, UserNode, QuestionNode>();
                });

                response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = JsonConvert.SerializeObject(responseBody),
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/json" } }
                };
            }

            LogResponse(context, response);
            return response;
        }

        private void LogResponse(ILambdaContext context, APIGatewayProxyResponse response) => context.Logger.LogLine($"RESPONSE: {JsonConvert.SerializeObject(response)}");
    }

    public class UserQuestionAnswerRelationNode : RelationNode<UserNode, QuestionNode>
    {
        [Neo4jRelationProperty(Name = "a")]
        public long A { get; set; }

        [Neo4jRelationProperty(Name = "b")]
        public long B { get; set; }

        [Neo4jRelationProperty(Name = "c")]
        public long C { get; set; }

        [Neo4jRelationProperty(Name = "d")]
        public long D { get; set; }

        public UserQuestionAnswerRelationNode(UserNode origin, QuestionNode destiny)
            : base(origin, destiny)
        {
        }

        public UserQuestionAnswerRelationNode()
            : base()
        {
        }
    }

    public class CorrectAnswerRelation : UserQuestionAnswerRelationNode
    {
        public CorrectAnswerRelation(UserNode user, QuestionNode question)
            : base(user, question)
        {
            RelationType = "ANSWER_IS_INCORRECT";
        }
    }

    public class IncorrectAnswerRelation : UserQuestionAnswerRelationNode
    {
        public IncorrectAnswerRelation(UserNode user, QuestionNode question)
            : base(user, question)
        {
            RelationType = "ANSWER_IS_CORRECT";
        }
    }
}

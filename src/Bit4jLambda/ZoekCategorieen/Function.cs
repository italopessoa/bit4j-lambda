using System.Net.Http;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.SimpleSystemsManagement;
using Amazon.SQS;
using Amazon.SQS.Model;
using Bit4j.Lambda.Core.Extensions;
using Bit4j.Lambda.Core.Factory;
using Bit4j.Lambda.Core.Model;
using Bit4j.Lambda.Core.Model.Nodes;
using Neo4j.Driver.V1;
using Neo4j.Map.Extension.Map;
using Neo4j.Map.Extension.Model;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace ZoekCategorieen
{
    public class Function
    {
        private AmazonSQSClient _sqsClient;

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string> FunctionHandler(ILambdaContext context)
        {
            string categoryQueueURL = "";
            string neo4jUser = "";
            string neo4jPassword = "";
            string neo4jServerIp = "";
#if DEBUG
            categoryQueueURL = "";
            neo4jUser = "neo4j";
            neo4jPassword = "bitcoinshow";
            neo4jServerIp = "bolt://127.0.0.1:7687";
            context.Logger.LogLine("LOADING CREDENTIALS");
#else
            AmazonSimpleSystemsManagementClient ssmCLient = AWSClientFactory.GetAmazonSimpleSystemsManagementClient();
            categoryQueueURL = await ssmCLient.GetParameterValueAsync("categorieen_queue_url".ConvertToParameterRequest());
            context.Logger.LogLine("LOADING categoryQueueURL ");
            neo4jUser = await ssmCLient.GetParameterValueAsync("neo4j_user".ConvertToParameterRequest(true));
            context.Logger.LogLine("LOADING neo4jUser  ");
            neo4jPassword = await ssmCLient.GetParameterValueAsync("neo4j_password".ConvertToParameterRequest());
            context.Logger.LogLine("LOADING neo4jPassword ");
            neo4jServerIp = await ssmCLient.GetParameterValueAsync("neo4j_server_ip".ConvertToParameterRequest());
            context.Logger.LogLine("LOADING neo4jServerIp ");
#endif

            IDriver driver = GraphDatabase.Driver(neo4jServerIp, AuthTokens.Basic(neo4jUser, neo4jPassword));

            HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://opentdb.com/api_count_global.php");

            Catalog catalog = JsonConvert.DeserializeObject<Catalog>(await response.Content.ReadAsStringAsync());

            int neo4jRegisteredQuestions = 0;
            if (catalog.Overall.VerifiedQuestions > neo4jRegisteredQuestions)
            {
                _sqsClient = AWSClientFactory.GetAmazonSQSClient();

                response = await httpClient.GetAsync("https://opentdb.com/api_category.php");
                CategoryList categories = JsonConvert.DeserializeObject<CategoryList>(await response.Content.ReadAsStringAsync());
                SendMessageRequest sendMessageRequest =
                new SendMessageRequest
                {
                    QueueUrl = categoryQueueURL
                };

                SendMessageResponse sendMessageResponse = null;

                using (ISession session = driver.Session(AccessMode.Write))
                {
                    for (int i = 0; i < categories.Categories.Count; i++)
                    {
                        CategoryNode categoryNode = new CategoryNode(categories.Categories[i]);
                        CategoryNode actualCategoryNode = null;
                        string matchQuery = categoryNode.MapToCypher(CypherQueryType.Match);
                        IStatementResultCursor result = await session.RunAsync(matchQuery);

                        await result.ForEachAsync(r =>
                        {
                            actualCategoryNode = r[r.Keys[0]].Map<CategoryNode>();
                        });

                        if (actualCategoryNode == null)
                        {
                            string createQuery = categoryNode.MapToCypher(CypherQueryType.Create);
                            await session.RunAsync(createQuery);
                            result = await session.RunAsync(matchQuery);

                            context.Logger.LogLine($"CATEGORY \"{categoryNode.Name}\" CREATED");
                            await result.ForEachAsync(r =>
                            {
                                actualCategoryNode = r[r.Keys[0]].Map<CategoryNode>();
                            });
                        }

                        response = await httpClient.GetAsync($"https://opentdb.com/api_count.php?category={categoryNode.CategoryId}");
                        CategoryCatalog categoryCatalog = JsonConvert.DeserializeObject<CategoryCatalog>(await response.Content.ReadAsStringAsync());
                        categoryCatalog.UUID = actualCategoryNode.UUID;

                        sendMessageRequest.MessageBody = JsonConvert.SerializeObject(categoryCatalog);
                        sendMessageResponse = await _sqsClient.SendMessageAsync(sendMessageRequest);
                    }
                }
            }
            context.Logger.LogLine("CATEGORIES ENQUEUED");
            return "CATEGORIES ENQUEUED";
        }
    }
}

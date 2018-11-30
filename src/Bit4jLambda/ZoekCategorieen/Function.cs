using System;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;
using Bit4j.Lambda.Core.Factory;
using Bit4j.Lambda.Core.Model;
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
        public async Task<string> FunctionHandler(string input, ILambdaContext context)
        {
            string categoryQueueURL = Environment.GetEnvironmentVariable("CATEGORY_QUEUE_NAME");
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
                    QueueUrl = "https://sqs.us-east-1.amazonaws.com/134621539640/Categorieen_Q"
                };

                SendMessageResponse sendMessageResponse = null;

                bool categoryExists = true;
                for (int i = 0; i < categories.Categories.Count; i++)
                {
                    Category category = categories.Categories[i];
                    //TODO: check if category exists
                    if (categoryExists)
                    {
                        //MATCH to get category uuid
                    }
                    else
                    {
                        //CREATE get category uuid
                    }

                    response = await httpClient.GetAsync($"https://opentdb.com/api_count.php?category={category.CategoryId}");
                    CategoryCatalog categoryCatalog = JsonConvert.DeserializeObject<CategoryCatalog>(await response.Content.ReadAsStringAsync());

                    sendMessageRequest.MessageBody = JsonConvert.SerializeObject(category);
                    sendMessageResponse = await _sqsClient.SendMessageAsync(sendMessageRequest);
                }
            }
            return input?.ToUpper();
        }
    }
}

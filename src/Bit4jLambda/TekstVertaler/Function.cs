using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleSystemsManagement;
using Bit4j.Lambda.Core.Extensions;
using Bit4j.Lambda.Core.Factory;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translation.V2;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace TekstVertaler
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
            foreach(var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context);
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            context.Logger.LogLine($"Processed message {message.Body}");

            string vertaaldeVragenQueueURL = "";
            string neo4jUser = "";
            string neo4jPassword = "";
            string neo4jServerIp = "";
            string gcCredentialsJson = "";
#if DEBUG
            vertaaldeVragenQueueURL = "";
            neo4jUser = "neo4j";
            neo4jPassword = "bitcoinshow";
            neo4jServerIp = "bolt://127.0.0.1:7687";
            gcCredentialsJson = "";
#else
            using (AmazonSimpleSystemsManagementClient ssmCLient = AWSClientFactory.GetAmazonSimpleSystemsManagementClient())
            {
                vertaaldeVragenQueueURL = await ssmCLient.GetParameterValueAsync("vertaalde_vragen_queue_url".ConvertToParameterRequest());
                neo4jUser = await ssmCLient.GetParameterValueAsync("neo4j_user".ConvertToParameterRequest(true));
                neo4jPassword = await ssmCLient.GetParameterValueAsync("neo4j_password".ConvertToParameterRequest(true));
                neo4jServerIp = await ssmCLient.GetParameterValueAsync("neo4j_server_ip".ConvertToParameterRequest(true));
                gcCredentialsJson = await ssmCLient.GetParameterValueAsync("gc_translate".ConvertToParameterRequest(true));
            }
#endif
            GoogleCredential credential = GoogleCredential.FromJson(gcCredentialsJson);
            TranslationClient translationClient = TranslationClient.Create(credential);
            TranslationResult translationResult = translationClient.TranslateText("Quake Engine", "pt", "en");
            // TODO: Do interesting work based on the new message
            await Task.CompletedTask;
        }
    }
}

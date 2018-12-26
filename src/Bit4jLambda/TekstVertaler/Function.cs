using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleSystemsManagement;
using Bit4j.Lambda.Core.Extensions;
using Bit4j.Lambda.Core.Factory;
using Bit4j.Lambda.Core.Model.Nodes;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translation.V2;
using Neo4j.Driver.V1;
using Newtonsoft.Json;


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
        public async Task<bool> FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            try
            {
                foreach (var message in evnt.Records)
                {
                    await ProcessMessageAsync(message, context);
                }
            }
            catch (System.Exception ex)
            {
                context.Logger.LogLine("************ ERROR ***************");
                context.Logger.LogLine(ex.Message);
                context.Logger.LogLine(ex.StackTrace);
                if(ex.InnerException != null)
                {
                    context.Logger.LogLine(ex.InnerException.Message);
                }
                context.Logger.LogLine("************ END-ERROR ***********");
                return false;
            }

            return true;
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            context.Logger.LogLine($"Processed message {message.Body}");
            
            string neo4jUser = "";
            string neo4jPassword = "";
            string neo4jServerIp = "";
            string gcCredentialsJson = "";
#if DEBUG
            neo4jUser = "neo4j";
            neo4jPassword = "bitcoinshow";
            neo4jServerIp = "bolt://127.0.0.1:7687";
            gcCredentialsJson = "";
#else
            AmazonSimpleSystemsManagementClient ssmCLient = AWSClientFactory.GetAmazonSimpleSystemsManagementClient();
            neo4jUser = await ssmCLient.GetParameterValueAsync("neo4j_user".ConvertToParameterRequest());
            context.Logger.LogLine("LOADING neo4jUser  ");
            neo4jPassword = await ssmCLient.GetParameterValueAsync("neo4j_password".ConvertToParameterRequest());
            context.Logger.LogLine("LOADING neo4jPassword ");
            neo4jServerIp = await ssmCLient.GetParameterValueAsync("neo4j_server_ip".ConvertToParameterRequest());
            context.Logger.LogLine("LOADING neo4jServerIp ");
            gcCredentialsJson = await ssmCLient.GetParameterValueAsync("gc_translate_np".ConvertToParameterRequest());
            context.Logger.LogLine("LOADING gc_translate ");
#endif
            QuestionNode questionToTranslate = JsonConvert.DeserializeObject<QuestionNode>(message.Body);
            GoogleCredential credential = GoogleCredential.FromJson(gcCredentialsJson);
            TranslationClient translationClient = TranslationClient.Create(credential);
            Dictionary<string, object> properties = new Dictionary<string, object>();

            string trTitle = translationClient.TranslateText(questionToTranslate.Title, "pt", "en").TranslatedText;
            properties.Add("title_pt", trTitle);

            if (questionToTranslate.CorrectAnswer.GetType() == typeof(string))
            {
                string trCorrectAnswer = translationClient.TranslateText(questionToTranslate.CorrectAnswer.ToString(), "pt", "en").TranslatedText;
                properties.Add("correct_answer_pt", trCorrectAnswer);
            }

            IDriver driver = GraphDatabase.Driver(neo4jServerIp, AuthTokens.Basic(neo4jUser, neo4jPassword));
            using (ISession session = driver.Session(AccessMode.Write))
            {
                StringBuilder cypher = new StringBuilder();
                cypher.Append($"MATCH (q:Question {{uuid:'{questionToTranslate.UUID}'}}) SET ");

                List<string> setList = new List<string>();
                foreach (KeyValuePair<string, object> keyValue in properties)
                {
                    if (keyValue.Value.GetType() == typeof(string))
                        setList.Add($"q.{keyValue.Key}='{keyValue.Value}'");
                    else
                        setList.Add($"q.{keyValue.Key}={keyValue.Value}");
                }

                cypher.Append(string.Join(", ", setList));
                cypher.Append(" RETURN q;");

                await session.RunAsync(cypher.ToString());
            }

            // TODO: Do interesting work based on the new message
            await Task.CompletedTask;
        }
    }
}

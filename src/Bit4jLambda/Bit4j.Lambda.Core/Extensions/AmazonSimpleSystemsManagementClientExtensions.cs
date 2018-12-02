using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System.Net;
using System.Threading.Tasks;

namespace Bit4j.Lambda.Core.Extensions
{
    public static class AmazonSimpleSystemsManagementClientExtensions
    {
        public static async Task<string> GetParameterValueAsync(this AmazonSimpleSystemsManagementClient ssmClient, GetParameterRequest getParameterRequest)
        {
            string parameterValue = string.Empty;
            GetParameterResponse getParameterResponse = await ssmClient.GetParameterAsync(getParameterRequest);
            if (getParameterResponse.HttpStatusCode == HttpStatusCode.OK)
                parameterValue = getParameterResponse.Parameter.Value;

            return parameterValue;
        }

        public static GetParameterRequest ConvertToParameterRequest(this string parameterName, bool withDecryption = false)
        {
            return new GetParameterRequest
            {
                Name = parameterName,
                WithDecryption = withDecryption
            };
        }
    }
}

using Amazon;
using Amazon.SimpleSystemsManagement;
using Amazon.SQS;

namespace Bit4j.Lambda.Core.Factory
{
    public static class AWSClientFactory
    {
        static AmazonSQSClient _sqsClient;
        static AmazonSimpleSystemsManagementClient _ssmClient;

        public static AmazonSQSClient GetAmazonSQSClient()
        {
            if (_sqsClient == null)
            {
#if DEBUG
                _sqsClient = new AmazonSQSClient("", "", RegionEndpoint.USEast1);
#else
                _sqsClient = new AmazonSQSClient(RegionEndpoint.USEast1);
#endif
            }
            return _sqsClient;
        }

        public static AmazonSimpleSystemsManagementClient GetAmazonSimpleSystemsManagementClient()
        {
            if (_ssmClient == null)
            {
#if DEBUG
                _ssmClient = new AmazonSimpleSystemsManagementClient("", "", RegionEndpoint.USEast1);
#else
                _ssmClient = new AmazonSimpleSystemsManagementClient(RegionEndpoint.USEast1);
#endif
            }
            return _ssmClient;
        }
    }
}

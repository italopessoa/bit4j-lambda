using Amazon;
using Amazon.SimpleSystemsManagement;
using Amazon.SQS;

namespace Bit4j.Lambda.Core.Factory
{
    public static class AWSClientFactory
    {
        public static AmazonSQSClient GetAmazonSQSClient()
        {
#if DEBUG
            return new AmazonSQSClient("", "", RegionEndpoint.USEast1);
#else
            return new AmazonSQSClient(RegionEndpoint.USEast1);
#endif
        }

        public static AmazonSimpleSystemsManagementClient GetAmazonSimpleSystemsManagementClient()
        {
#if DEBUG
            return new AmazonSimpleSystemsManagementClient("", "", RegionEndpoint.USEast1);
#else
            return new AmazonSimpleSystemsManagementClient(RegionEndpoint.USEast1);
#endif
        }
    }
}

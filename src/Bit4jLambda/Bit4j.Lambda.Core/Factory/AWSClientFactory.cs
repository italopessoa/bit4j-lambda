using Amazon;
using Amazon.SQS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bit4j.Lambda.Core.Factory
{
    public static class AWSClientFactory
    {
        static AmazonSQSClient _sqsClient;

        public static AmazonSQSClient GetAmazonSQSClient()
        {
            if (_sqsClient == null)
            {
                _sqsClient = new AmazonSQSClient("", "", RegionEndpoint.USEast1);
            }
            return _sqsClient;
        }
    }
}

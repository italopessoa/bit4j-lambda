using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.SQSEvents;

using TekstVertaler;

namespace TekstVertaler.Tests
{
    public class FunctionTest
    {
        [Fact]
        public async Task TestTranslationLambdaFunction()
        {
            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = "{\"category\":\"Entertainment: Video Games\",\"type\":\"multiple\",\"difficulty\":\"easy\",\"question\":\"Half - Life by Valve uses the GoldSrc game engine, which is a highly modified version of what engine ?\",\"correct_answer\":\"Quake Engine\",\"Id\":1485,\"UUID\":\"3c9a9ad0-fb0a-11e8-ace1-b05216d697df\"}"
                    }
                }
            };

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var function = new Function();
            await function.FunctionHandler(sqsEvent, context);

            Assert.Contains("Processed message foobar", logger.Buffer.ToString());
        }
    }
}

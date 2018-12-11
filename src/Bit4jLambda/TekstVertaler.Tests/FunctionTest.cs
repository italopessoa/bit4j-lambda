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
                        Body = "{category:\"Art\",type: \"multiple\",difficulty: \"easy\",question: \"Who painted \\\"Swans Reflecting Elephants\\\", \\\"Sleep\\\", and \\\"The Persistence of Memory\\\"?\",correct_answer: \"Salvador Dali\",Id: 877,UUID: \"5559a510-fc7d-11e8-9c65-b05216d697df\"}"
                    }
                }
            };

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var function = new Function();
            bool questionTranslated = await function.FunctionHandler(sqsEvent, context);

            Assert.True(questionTranslated);
        }
    }
}

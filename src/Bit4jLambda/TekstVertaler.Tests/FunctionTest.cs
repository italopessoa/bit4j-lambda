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
                        //Body = "{category:\"Art\",type: \"multiple\",difficulty: \"easy\",question: \"Who painted \\\"Swans Reflecting Elephants\\\", \\\"Sleep\\\", and \\\"The Persistence of Memory\\\"?\",correct_answer: \"Salvador Dali\",Id: 877,UUID: \"5559a510-fc7d-11e8-9c65-b05216d697df\"}"
                        //Body = "{category:\"Entertainment: Books\",type: \"multiple\",difficulty: \"medium\",question: \"Who wrote the \\\"A Song of Ice And Fire\\\" fantasy novel series?\",correct_answer: \"George R. R. Martin\", Id: 17945,UUID: \"c54fc780-fe32-11e8-9fd1-12e9ac86eb58\"}"
                        Body = "{category:\"Entertainment: Film\",type: \"multiple\",difficulty: \"medium\",question: \"In what year was the movie \\\"Police Academy\\\" released?\",correct_answer: 1984,Id: 3020,UUID: \"e3823440-fe50-11e8-9fd1-12e9ac86eb58\"}"
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

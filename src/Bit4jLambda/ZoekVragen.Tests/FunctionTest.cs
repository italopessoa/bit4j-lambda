using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.SQSEvents;

using ZoekVragen;

namespace ZoekVragen.Tests
{
    public class FunctionTest
    {
        [Fact]
        public async Task TestCreateQuestionstLambdaFunction()
        {
            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        //Body = "{\"category_id\":15,\"uuid\":\"64490320-f577-11e8-82f2-b05216d697df\",\"category_question_count\":{ \"total_question_count\":799}}"
                        //Body = "{\"category_id\":22,\"uuid\":\"64490320-f577-11e8-82f2-b05216d697df\",\"category_question_count\":{ \"total_question_count\":22}}"
                        //Body = "{\"category_id\":12,\"name\":\"Art\",\"uuid\":\"4027b7d0-fd3c-11e8-bbc2-12e9ac86eb58\",\"category_question_count\": {\"total_question_count\": 22 }}"
                        //Body = "{\"category_id\":13,\"name\":\"Art\",\"uuid\":\"cac08180-fe07-11e8-9507-12e9ac86eb58\",\"category_question_count\": {\"total_question_count\": 22 }}"
                        Body = "{\"category_id\":17,\"name\":\"Art\",\"uuid\":\"c25cc640-fe32-11e8-9fd1-12e9ac86eb58\",\"category_question_count\": {\"total_question_count\": 174 }}"
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

            //Assert.Contains("Processed message foobar", logger.Buffer.ToString());
            //MATCH(n: Question { category: 'Entertainment: Music', type: 'multiple', difficulty: 'medium', title: 'Which of the following bands is Tom DeLonge not a part of?', correct_answer: +44}) RETURN n
        }
    }
}

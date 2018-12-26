using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

namespace ZoekVragen.Tests
{
    public class TestLambdaLogger : ILambdaLogger
    {
        readonly ITestOutputHelper output;

        public TestLambdaLogger(ITestOutputHelper output)
        {
            this.output = output;
        }

        public void Log(string message)
        {
            Debug.Write(message);
            output.WriteLine(message);
        }

        public void LogLine(string message)
        {
            Debug.WriteLine(message);
            output.WriteLine(message);
        }
    }
    public class FunctionTest
    {
        readonly ITestOutputHelper output;

        public FunctionTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task TestCreateQuestionstLambdaFunction()
        {
            List<string> categories = new List<string>()
            {
                "{\"category_id\":9,\"name\":\"General Knowledge\",\"uuid\":\"e820f080053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                //"{\"category_id\":10,\"name\":\"Entertainment: Books\",\"uuid\":\"e8587b40053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                //"{\"category_id\":11,\"name\":\"Entertainment: Film\",\"uuid\":\"e86f85b0053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                //"{\"category_id\":12,\"name\":\"Entertainment: Music\",\"uuid\":\"e8864200053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                //"{\"category_id\":13,\"name\":\"Entertainment: Musicals & Theatres\",\"uuid\":\"e8929e10053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                //"{\"category_id\":14,\"name\":\"Entertainment: Television\",\"uuid\":\"e89f4840053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                //"{\"category_id\":15,\"name\":\"Entertainment: Video Games\",\"uuid\":\"e8aae100053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                //"{\"category_id\":16,\"name\":\"Entertainment: Board Games\",\"uuid\":\"e8b51a30053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                //"{\"category_id\":17,\"name\":\"Science & Nature\",\"uuid\":\"e8c17640053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                //"{\"category_id\":18,\"name\":\"Science: Computers\",\"uuid\":\"e8cb1330053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                //"{\"category_id\":19,\"name\":\"Science: Mathematics\",\"uuid\":\"e8d54c60053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                //"{\"category_id\":20,\"name\":\"Mythology\",\"uuid\":\"e8e244b0053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                //"{\"category_id\":21,\"name\":\"Sports\",\"uuid\":\"e8ecf310053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                //"{\"category_id\":22,\"name\":\"Geography\",\"uuid\":\"e8fcaa80053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                //"{\"category_id\":23,\"name\":\"History\",\"uuid\":\"e90cb010053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                //"{\"category_id\":24,\"name\":\"Politics\",\"uuid\":\"e917fab0053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                //"{\"category_id\":25,\"name\":\"Art\",\"uuid\":\"e922d020053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                //"{\"category_id\":26,\"name\":\"Celebrities\",\"uuid\":\"e95ddd50053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                //"{\"category_id\":27,\"name\":\"Animals\",\"uuid\":\"e96ad5a0053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                //"{\"category_id\":28,\"name\":\"Vehicles\",\"uuid\":\"e9769570053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                //"{\"category_id\":29,\"name\":\"Entertainment: Comics\",\"uuid\":\"e9869b00053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                "{\"category_id\":30,\"name\":\"Science: Gadgets\",\"uuid\":\"e990fb40053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                "{\"category_id\":31,\"name\":\"Entertainment: Japanese Anime & Manga\",\"uuid\":\"e99b5b80053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}",
                "{\"category_id\":32,\"name\":\"Entertainment: Cartoon & Animations\",\"uuid\":\"e9a74260053111e9a8c1b05216d697df\" ,\"category_question_count\" : {\"total_question_count\": 799}}"
            };


            foreach (var category in categories)
            {
                var sqsEvent = new SQSEvent
                {
                    Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = category
                    }
                }
                };

                var logger = new TestLambdaLogger(output);
                var context = new TestLambdaContext
                {
                    Logger = logger
                };

                var function = new Function();
                await function.FunctionHandler(sqsEvent, context);
            }
        }
    }
}

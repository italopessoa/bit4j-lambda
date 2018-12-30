using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Bit4j.Lambda.Core.Model;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;

using Xunit;
using Xunit.Abstractions;

namespace Lerares.Tests
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
        private readonly ITestOutputHelper output;

        public FunctionTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async void TestLeraresQueryParameterEmptyError()
        {
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;
            request = new APIGatewayProxyRequest
            {
                IsBase64Encoded = true
            };

            Function function = new Function();
            var logger = new TestLambdaLogger(output);
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            response = await function.FunctionHandler(request, context);

            Assert.NotNull(response);
            Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Query string parameter \"user\" cannot be empty", response.Body);
        }

        [Fact]
        public async void TestLeraresBodyIsEmptyError()
        {
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;
            request = new APIGatewayProxyRequest
            {
                IsBase64Encoded = true,
                QueryStringParameters = new Dictionary<string, string>
                {
                    {"user", "50da8b7009f811e9aac1b05216d697df_error" },
                    {"lang", "pt" }
                }
            };

            Function function = new Function();
            var logger = new TestLambdaLogger(output);
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            response = await function.FunctionHandler(request, context);

            Assert.NotNull(response);
            Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Body cannot be empty", response.Body);
        }

        [Fact]
        public async void TestLeraresBodyUserAnswerIsEmptyError()
        {
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;
            request = new APIGatewayProxyRequest
            {
                IsBase64Encoded = true,
                QueryStringParameters = new Dictionary<string, string>
                {
                    {"user", "50da8b7009f811e9aac1b05216d697df" },
                    {"lang", "pt" }
                }
            };

            Function function = new Function();
            var logger = new TestLambdaLogger(output);
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var answer = new UserAnswer
            {
                UserUUID = "50da8b7009f811e9aac1b05216d697df",
                QuestionUUID = "12313123123123",
                SelectedOption = "a"
            };

            var plainTextBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(answer));
            request.Body = Convert.ToBase64String(plainTextBytes);
            response = await function.FunctionHandler(request, context);

            Assert.NotNull(response);
            Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Body property \"Answer\" cannot be empty", response.Body);
        }

        [Fact]
        public async void TestLeraresBodyUserQuestionUUIDIsEmptyError()
        {
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;
            request = new APIGatewayProxyRequest
            {
                IsBase64Encoded = true,
                QueryStringParameters = new Dictionary<string, string>
                {
                    {"user", "50da8b7009f811e9aac1b05216d697df" },
                    {"lang", "pt" }
                }
            };

            Function function = new Function();
            var logger = new TestLambdaLogger(output);
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var answer = new UserAnswer
            {
                UserUUID = "50da8b7009f811e9aac1b05216d697df",
                Answer = "50da8b7009f811e9aac1b05216d69uj2",
                SelectedOption = "a"
            };

            var plainTextBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(answer));
            request.Body = Convert.ToBase64String(plainTextBytes);
            response = await function.FunctionHandler(request, context);

            Assert.NotNull(response);
            Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Body property \"QuestionUUID\" cannot be empty", response.Body);
        }

        [Fact]
        public async void TestLeraresBodyUserSelectedOptionIsEmptyError()
        {
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;
            request = new APIGatewayProxyRequest
            {
                IsBase64Encoded = true,
                QueryStringParameters = new Dictionary<string, string>
                {
                    {"user", "50da8b7009f811e9aac1b05216d697df" },
                    {"lang", "pt" }
                }
            };

            Function function = new Function();
            var logger = new TestLambdaLogger(output);
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var answer = new UserAnswer
            {
                UserUUID = "50da8b7009f811e9aac1b05216d697df",
                Answer = "50da8b7009f811e9aac1b05216d69uj2",
                QuestionUUID = "12313123123123",
            };

            var plainTextBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(answer));
            request.Body = Convert.ToBase64String(plainTextBytes);
            response = await function.FunctionHandler(request, context);

            Assert.NotNull(response);
            Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Body property \"SelectedOption\" cannot be empty", response.Body);
        }

        [Fact]
        public async void TestLeraresBodyUserSelectedUserUUIDIsEmptyError()
        {
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;
            request = new APIGatewayProxyRequest
            {
                IsBase64Encoded = true,
                QueryStringParameters = new Dictionary<string, string>
                {
                    {"user", "50da8b7009f811e9aac1b05216d697df" },
                    {"lang", "pt" }
                }
            };

            Function function = new Function();
            var logger = new TestLambdaLogger(output);
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var answer = new UserAnswer
            {
                Answer = "50da8b7009f811e9aac1b05216d69uj2",
                QuestionUUID = "12313123123123",
                SelectedOption = "a"
            };

            var plainTextBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(answer));
            request.Body = Convert.ToBase64String(plainTextBytes);
            response = await function.FunctionHandler(request, context);

            Assert.NotNull(response);
            Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Body property \"UserUUID\" cannot be empty", response.Body);
        }

        [Fact]
        public async void TestLeraresUserNotFound()
        {
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;
            request = new APIGatewayProxyRequest
            {
                IsBase64Encoded = true,
                QueryStringParameters = new Dictionary<string, string>
                {
                    {"user", "50da8b7009f811e9aac1b05216d697df_error" },
                    {"lang", "pt" }
                }
            };

            Function function = new Function();
            var logger = new TestLambdaLogger(output);
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var answer = new UserAnswer
            {
                UserUUID = "50da8b7009f811e9aac1b05216d697df_error",
                QuestionUUID = "f1644c80091b11e9b2b6b05216d697df",
                SelectedOption = "a",
                Answer = "test",
            };

            var plainTextBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(answer));
            request.Body = Convert.ToBase64String(plainTextBytes);
            response = await function.FunctionHandler(request, context);

            Assert.NotNull(response);
            Assert.Equal((int)HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("User not found!", response.Body);
        }

        [Fact]
        public async void TestLeraresUserParamDontMatch()
        {
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;
            request = new APIGatewayProxyRequest
            {
                IsBase64Encoded = true,
                QueryStringParameters = new Dictionary<string, string>
                {
                    {"user", "50da8b7009f811e9aac1b05216d697df" },
                    {"lang", "pt" }
                }
            };

            Function function = new Function();
            var logger = new TestLambdaLogger(output);
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var answer = new UserAnswer
            {
                UserUUID = "50da8b7009f811e9aac1b05216d697df_error",
                QuestionUUID = "f1644c80091b11e9b2b6b05216d697df_error",
                SelectedOption = "a",
                Answer = "test",
            };

            var plainTextBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(answer));
            request.Body = Convert.ToBase64String(plainTextBytes);
            response = await function.FunctionHandler(request, context);

            Assert.NotNull(response);
            Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("User UUID don't match. Check your body request", response.Body);
        }

        [Fact]
        public async void TestLeraresQuestionNotFound()
        {
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;
            request = new APIGatewayProxyRequest
            {
                IsBase64Encoded = true,
                QueryStringParameters = new Dictionary<string, string>
                {
                    {"user", "50da8b7009f811e9aac1b05216d697df" },
                    {"lang", "pt" }
                }
            };

            Function function = new Function();
            var logger = new TestLambdaLogger(output);
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var answer = new UserAnswer
            {
                UserUUID = "50da8b7009f811e9aac1b05216d697df",
                QuestionUUID = "f1644c80091b11e9b2b6b05216d697df_error",
                SelectedOption = "a",
                Answer = "test",
            };

            string a = JsonConvert.SerializeObject(answer);
            var plainTextBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(answer));
            request.Body = Convert.ToBase64String(plainTextBytes);
            response = await function.FunctionHandler(request, context);

            Assert.NotNull(response);
            Assert.Equal((int)HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("Question not found!", response.Body);
        }

        [Fact]
        public async void TestLeraresInvalidSelectedOption()
        {
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;
            request = new APIGatewayProxyRequest
            {
                IsBase64Encoded = true,
                QueryStringParameters = new Dictionary<string, string>
                {
                    {"user", "50da8b7009f811e9aac1b05216d697df_error" },
                    {"lang", "pt" }
                }
            };

            Function function = new Function();
            var logger = new TestLambdaLogger(output);
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var answer = new UserAnswer
            {
                UserUUID = "50da8b7009f811e9aac1b05216d697df_error",
                QuestionUUID = "f1644c80091b11e9b2b6b05216d697df_error",
                SelectedOption = "x",
                Answer = "test",
            };

            var plainTextBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(answer));
            request.Body = Convert.ToBase64String(plainTextBytes);
            response = await function.FunctionHandler(request, context);

            Assert.NotNull(response);
            Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("SelectedOption must be in [\"a\",\"b\",\"c\",\"d\"]", response.Body);
        }

        [Fact]
        public async void TestLeraresIncorrectAnswer()
        {
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;
            request = new APIGatewayProxyRequest
            {
                IsBase64Encoded = true,
                QueryStringParameters = new Dictionary<string, string>
                {
                    {"user", "50da8b7009f811e9aac1b05216d697df" },
                    {"lang", "pt" }
                }
            };

            Function function = new Function();
            var logger = new TestLambdaLogger(output);
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var answer = new UserAnswer
            {
                UserUUID = "50da8b7009f811e9aac1b05216d697df",
                QuestionUUID = "f1644c80091b11e9b2b6b05216d697df",
                SelectedOption = "a",
                Answer = "test",
            };

            var plainTextBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(answer));
            request.Body = Convert.ToBase64String(plainTextBytes);
            response = await function.FunctionHandler(request, context);

            Assert.NotNull(response);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async void TestLeraresCorrectAnswer()
        {
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;
            request = new APIGatewayProxyRequest
            {
                IsBase64Encoded = true,
                QueryStringParameters = new Dictionary<string, string>
                {
                    {"user", "50da8b7009f811e9aac1b05216d697df" },
                    {"lang", "pt" }
                }
            };

            Function function = new Function();
            var logger = new TestLambdaLogger(output);
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var answer = new UserAnswer
            {
                UserUUID = "50da8b7009f811e9aac1b05216d697df",
                QuestionUUID = "f1644c80091b11e9b2b6b05216d697df",
                SelectedOption = "b",
                Answer = "HTC",
            };

            var plainTextBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(answer));
            request.Body = Convert.ToBase64String(plainTextBytes);
            response = await function.FunctionHandler(request, context);

            Assert.NotNull(response);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        }
    }
}

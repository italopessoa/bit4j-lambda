using Xunit;
using Amazon.Lambda.TestUtilities;

namespace ZoekCategorieen.Tests
{
    public class FunctionTest
    {
        [Fact]
        public async void GetCategoriesFunctionTest()
        {

            // Invoke the lambda function and confirm the string was upper cased.
            var function = new Function();
            var context = new TestLambdaContext();
            var upperCase = await function.FunctionHandler(context);

            Assert.Equal("CATEGORIES ENQUEUED", upperCase);
        }
    }
}

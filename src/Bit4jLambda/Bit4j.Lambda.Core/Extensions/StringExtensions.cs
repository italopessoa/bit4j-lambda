using System;
using System.Text;

namespace Bit4j.Lambda.Core.Extensions
{
    public static class StringExtensions
    {
        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}

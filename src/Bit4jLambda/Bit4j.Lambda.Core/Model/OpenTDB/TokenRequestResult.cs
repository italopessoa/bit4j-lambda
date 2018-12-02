using Newtonsoft.Json;

namespace Bit4j.Lambda.Core.Model.OpenTDB
{
    public class TokenRequestResult : RequestResult
    {
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}

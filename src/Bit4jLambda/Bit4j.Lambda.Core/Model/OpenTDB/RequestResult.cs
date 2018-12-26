using Newtonsoft.Json;

namespace Bit4j.Lambda.Core.Model.OpenTDB
{
    public abstract class RequestResult
    {
        [JsonProperty("response_code")]
        public ResponseCodeEnum ResponseCode { get; set; }
    }
}

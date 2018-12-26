using Newtonsoft.Json;

namespace Bit4j.Lambda.Core.Model
{
    public class TotalQuestionCount
    {
        [JsonProperty("total_question_count")]
        public int Total { get; set; }
    }
}

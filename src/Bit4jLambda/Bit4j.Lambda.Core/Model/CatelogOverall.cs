using Newtonsoft.Json;

namespace Bit4j.Lambda.Core.Model
{
    public class CatelogOverall
    {
        [JsonProperty("total_num_of_verified_questions")]
        public int VerifiedQuestions { get; set; }
    }
}

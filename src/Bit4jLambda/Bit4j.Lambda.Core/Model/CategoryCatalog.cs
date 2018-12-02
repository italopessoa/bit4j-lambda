using Newtonsoft.Json;

namespace Bit4j.Lambda.Core.Model
{
    public class CategoryCatalog
    {
        [JsonProperty("category_id")]
        public int Id { get; set; }

        [JsonProperty("uuid")]
        public string UUID { get; set; }

        [JsonProperty("category_question_count")]
        public TotalQuestionCount CategoryCount { get; set; }
    }
}

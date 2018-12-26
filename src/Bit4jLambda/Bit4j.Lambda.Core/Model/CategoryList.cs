using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bit4j.Lambda.Core.Model
{
    public class CategoryList
    {
        [JsonProperty("trivia_categories")]
        public List<Category> Categories { get; set; }
    }
}

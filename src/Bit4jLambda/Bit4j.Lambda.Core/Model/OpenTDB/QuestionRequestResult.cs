using Bit4j.Lambda.Core.Model.Nodes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bit4j.Lambda.Core.Model.OpenTDB
{
    public class QuestionRequestResult : RequestResult
    {
        [JsonProperty("results")]
        public List<QuestionNode> Questions { get; set; }
    }
}

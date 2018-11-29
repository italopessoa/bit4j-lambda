using Newtonsoft.Json;

namespace Bit4j.Lambda.Core.Model
{
    public class Category
    {
        [JsonProperty("id")]
        public int CategoryId { get; set; }

        [JsonProperty("uuid")]
        public string UUID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{CategoryId} - {Name}";
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash *= 23 + CategoryId.GetHashCode();
                hash *= 23 + Name.GetHashCode();
                return hash;
            }
        }
    }
}

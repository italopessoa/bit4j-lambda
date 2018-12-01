using Neo4j.Map.Extension.Attributes;
using Neo4j.Map.Extension.Model;

namespace Bit4j.Lambda.Core.Model.Nodes
{
    [Neo4jLabel("Category")]
    public class CategoryNode : Neo4jNode
    {
        public CategoryNode()
        {
        }

        public CategoryNode(Category category)
        {
            Name = category.Name;
            CategoryId = category.CategoryId;
        }

        [Neo4jProperty(Name = "name")]
        public string Name { get; set; }

        [Neo4jProperty(Name = "category_id")]
        public long CategoryId { get; set; }

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

        public override string ToString()
        {
            return $"Person {{UUID: '{UUID}', Name: '{Name}', CategoryId: {CategoryId} }}";
        }
    }
}

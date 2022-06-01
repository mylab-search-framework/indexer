using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Models
{
    public class IndexingEntity
    {
        public string Id { get; set; }

        public JObject Entity { get; set; }
    }
}
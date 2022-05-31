using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Models
{
    public class IndexingRequestEntity
    {
        public string Id { get; set; }

        public JObject Entity { get; set; }
    }
}
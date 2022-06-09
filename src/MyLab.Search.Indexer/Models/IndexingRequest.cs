using MyLab.Search.EsAdapter.Indexing;
using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Models
{
    public class IndexingRequest
    {
        public string IndexId { get; set; }
        public IndexingEntity[] PostList { get; set; }
        public IndexingEntity[] PutList { get; set; }
        public IndexingEntity[] PatchList { get; set; }
        public string[] DeleteList { get; set; }

        public IndexingRequest Clone()
        {
            return new IndexingRequest
            {
                PostList = PostList,
                PutList = PutList,
                DeleteList = DeleteList,
                PatchList = PatchList,
                IndexId = IndexId
            };
        }

        public EsBulkIndexingRequest<JObject> ToEsBulkRequest()
        {
            var res = new EsBulkIndexingRequest<JObject>();
        }
    }
}
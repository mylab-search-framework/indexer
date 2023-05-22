using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Models
{
    public class IndexingRequest
    {
        public string IndexId { get; set; }
        public JObject[] PutList { get; set; }
        public JObject[] PatchList { get; set; }
        public string[] DeleteList { get; set; }

        public IndexingRequest Clone()
        {
            return new IndexingRequest
            {
                PutList = PutList,
                DeleteList = DeleteList,
                PatchList = PatchList,
                IndexId = IndexId
            };
        }

        public bool IsEmpty()
        {
            return PutList is not { Length: > 0 } &&
                   PatchList is not { Length: > 0 } && 
                   DeleteList is not { Length: > 0 };
        }
    }
}
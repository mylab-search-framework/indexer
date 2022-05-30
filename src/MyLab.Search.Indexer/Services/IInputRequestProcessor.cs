using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Services
{
    public interface IInputRequestProcessor
    {
        Task IndexAsync(IndexingRequest indexingRequest);
    }

    public class IndexingRequest
    {
        public string IndexId { get; set; }
        public IndexingRequestEntity[] PostList { get; set; }
        public IndexingRequestEntity[] PutList { get; set; }
        public IndexingRequestEntity[] PatchList { get; set; }
        public string[] DeleteList { get; set; }
    }

    public class IndexingRequestEntity
    {
        public string Id { get; set; }

        public JObject Entity { get; set; }
    }
}

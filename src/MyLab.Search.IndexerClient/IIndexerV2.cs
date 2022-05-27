using System.Threading.Tasks;
using MyLab.ApiClient;

namespace MyLab.Search.IndexerClient
{
    /// <summary>
    /// MyLab.Search.Indexer contract
    /// </summary>
    [Api("v2",Key = "indexer")]
    public interface IIndexerV2
    {
        /// <summary>
        /// Index entities
        /// </summary>
        [Post]
        Task IndexAsync([JsonContent] IndexingRequest request);
    }
}

using System.Threading.Tasks;
using MyLab.ApiClient;

namespace MyLab.Search.IndexerClient
{
    /// <summary>
    /// MyLab.Search.Indexer contract
    /// </summary>
    public interface IIndexerV2
    {
        /// <summary>
        /// Index entities
        /// </summary>
        Task IndexAsync([JsonContent] IndexingRequest request);
    }
}

using System.Threading.Tasks;
using MyLab.ApiClient;

namespace MyLab.Search.IndexerClient
{
    /// <summary>
    /// MyLab.Search.Indexer sync task contract
    /// </summary>
    [Api("processing", Key = "indexer")]
    public interface IIndexerSyncTaskApi
    {
        /// <summary>
        /// Starts synchronization process
        /// </summary>
        [Post]
        Task StartSynchronizationAsync();
    }
}
using System.Threading.Tasks;
using MyLab.ApiClient;
using Newtonsoft.Json.Linq;

namespace MyLab.Search.IndexerClient
{
    /// <summary>
    /// MyLab.Search.Indexer contract
    /// </summary>
    [Api("v2/indexes",Key = "indexer")]
    public interface IIndexerV2
    {
        /// <summary>
        /// Index new doc
        /// </summary>
        [Post("{indexId}")]
        Task PostAsync([Path] string indexId, [JsonContent] JObject doc);

        /// <summary>
        /// Index new doc or reindex if already indexed
        /// </summary>
        [Put("{indexId}")]
        Task PutAsync([Path] string indexId, [JsonContent] JObject doc);

        /// <summary>
        /// Merge specified partial data with indexed doc
        /// </summary>
        [Patch("{indexId}")]
        Task PatchAsync([Path] string indexId, [JsonContent] JObject doc);

        /// <summary>
        /// Remove specified doc from index
        /// </summary>
        [Delete("{indexId}/{docId}")]
        Task DeleteAsync([Path] string indexId, [Path] string docId);

        /// <summary>
        /// Kick to index an doc with specified id from data source
        /// </summary>
        [Post("{indexId}/{docId}/kicker")]
        Task KickAsync([Path] string indexId, [Path] string docId);
    }
}

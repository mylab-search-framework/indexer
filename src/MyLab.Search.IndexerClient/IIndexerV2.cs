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
        /// Index new entity without id
        /// </summary>
        [Post("{indexId}")]
        Task PostAsync([Path] string indexId, [JsonContent] JObject entity);

        /// <summary>
        /// Index new entity
        /// </summary>
        [Post("{indexId}/{entityId}")]
        Task PostAsync([Path] string indexId, [Path] string entityId, [JsonContent] JObject entity);

        /// <summary>
        /// Index new entity or reindex if already indexed
        /// </summary>
        [Put("{indexId}/{entityId}")]
        Task PutAsync([Path] string indexId,[Path] string entityId, [JsonContent] JObject entity);

        /// <summary>
        /// Merge specified partial data with indexed entity
        /// </summary>
        [Patch("{indexId}/{entityId}")]
        Task PatchAsync([Path] string indexId, [Path] string entityId, [JsonContent] JObject entity);

        /// <summary>
        /// Remove specified entity from index
        /// </summary>
        [Delete("{indexId}/{entityId}")]
        Task DeleteAsync([Path] string indexId, [Path] string entityId);

        /// <summary>
        /// Kick to index an entity with specified id from data source
        /// </summary>
        [Post("{indexId}/{entityId}/kicker")]
        Task KickAsync([Path] string indexId, [Path] string entityId);
    }
}

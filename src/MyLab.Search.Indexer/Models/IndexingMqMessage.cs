using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#if SERVER_CODE
namespace MyLab.Search.Indexer.Models
#else
namespace MyLab.Search.IndexerClient
#endif
{
    /// <summary>
    /// Contains indexing details to be transmitted in the MQ
    /// </summary>
    public class IndexingMqMessage
    {
        /// <summary>
        /// Index identifier
        /// </summary>
        [JsonProperty("indexId")]
        public string IndexId { get; set; }
        /// <summary>
        /// Post-list, which contains entities for insert only
        /// </summary>
        [JsonProperty("post")]
        public JObject[] Post { get; set; }
        /// <summary>
        /// Put-list, which contains entities for insert or replace if already indexed
        /// </summary>
        [JsonProperty("put")]
        public JObject[] Put { get; set; }
        /// <summary>
        /// Patch-list, which contains entities for change already indexed entities
        /// </summary>
        [JsonProperty("patch")]
        public JObject[] Patch { get; set; }
        /// <summary>
        /// Delete-list, which contains an entity identifiers for removing from index
        /// </summary>
        [JsonProperty("delete")]
        public string[] Delete { get; set; }
        /// <summary>
        /// Kick-list, which contains an entity identifiers for indexing from database
        /// </summary>
        [JsonProperty("kick")]
        public string[] Kick { get; set; }
    }
}

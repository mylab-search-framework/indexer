using MyLab.Log;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyLab.Search.Indexer.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Services
{
    class FileIndexResourceProvider : IIndexResourceProvider
    {
        private readonly IndexerOptions _opts;

        public FileIndexResourceProvider(IOptions<IndexerOptions> opts)
            : this(opts.Value)
        {
            
        }
        
        public FileIndexResourceProvider(IndexerOptions opts)
        {
            _opts = opts;
        }

        public async Task<string> ProvideKickQueryAsync(string indexId)
        {
            var idxOpts = _opts.GetIndexOptions(indexId);

            if (idxOpts.KickDbQuery != null) 
                return idxOpts.KickDbQuery;

            return await ReadResourceFileAsync(indexId, "kick.sql");
        }

        public async Task<string> ProvideSyncQueryAsync(string indexId)
        {
            var idxOpts = _opts.GetIndexOptions(indexId);

            if (idxOpts.SyncDbQuery != null)
                return idxOpts.SyncDbQuery;

            return await ReadResourceFileAsync(indexId, "sync.sql");
        }

        public async Task<string> ProvideIndexSettingsAsync(string indexId)
        {
            string indexJson = null, commonIndexJson = null;

            var indexJsonPath = Path.Combine(_opts.ResourcesPath, indexId, "index.json");
            if (File.Exists(indexJsonPath))
            {
                indexJson = await File.ReadAllTextAsync(indexJsonPath);
            }

            var commonIndexJsonPath = Path.Combine(_opts.ResourcesPath, "index.json");
            if (File.Exists(commonIndexJsonPath))
            {
                commonIndexJson = await File.ReadAllTextAsync(commonIndexJsonPath);
            }

            if (indexJson == null && commonIndexJson == null)
                throw new FileNotFoundException("Resource not found")
                    .AndFactIs("index-id", indexId)
                    .AndFactIs("index-file", indexJsonPath)
                    .AndFactIs("common-file", commonIndexJsonPath);

            if (commonIndexJson == null)
                return indexJson;

            if (indexJson == null)
                return commonIndexJson;

            var indexJObj = JObject.Parse(indexJson);
            var resultJson = JObject.Parse(commonIndexJson);

            resultJson.Merge(indexJObj);

            return resultJson.ToString(Formatting.None);
        }

        async Task<string> ReadResourceFileAsync(string indexId, string filename)
        {
            var filePath = Path.Combine(_opts.ResourcesPath, indexId, filename);

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Resource file not found")
                    .AndFactIs("index-id", indexId)
                    .AndFactIs("full-path", filePath);

            return await File.ReadAllTextAsync(filePath);
        }
    }
}

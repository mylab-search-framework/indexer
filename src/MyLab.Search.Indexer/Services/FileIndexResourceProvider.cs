using MyLab.Log;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyLab.Search.Indexer.Options;

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

        async Task<string> ReadResourceFileAsync(string indexId, string filename)
        {
            var filePath = Path.Combine(_opts.ResourcePath, indexId, filename);

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Resource file not found")
                    .AndFactIs("index-id", indexId)
                    .AndFactIs("full-path", filePath);

            return await File.ReadAllTextAsync(filePath);
        }
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.Options;

namespace MyLab.Search.Indexer.Services
{
    class IndexResourceProvider : IIndexResourceProvider
    {
        private readonly IndexerOptions _options;
        private readonly IDslLogger _log;

        public IndexResourceProvider(IOptions<IndexerOptions> options,
            ILogger<IndexResourceProvider> logger = null)
            :this(options.Value, logger)
        {
        }
        public IndexResourceProvider(IndexerOptions options,
            ILogger<IndexResourceProvider> logger = null)
        {
            _options = options;
            _log = logger?.Dsl();
        }
        public Task<string> ReadFileAsync(string idxId, string filename)
        {
            var path = Path.Combine(_options.IndexPath, idxId, filename);

            if (!File.Exists(path))
            {
                var nsPath = Path.Combine(_options.NamespacesPath, idxId, filename);

                if (File.Exists(path))
                {
                    _log?.Warning("An old index resource path detected")
                        .AndFactIs("expected-path", path)
                        .AndFactIs("actual-path", nsPath)
                        .Write();

                    path = nsPath;
                }
                else
                {
                    throw new InvalidOperationException("Index file not found")
                        .AndFactIs("filepath", path);
                }
            }

            return File.ReadAllTextAsync(path);
        }
    }
}
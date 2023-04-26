using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.Search.EsAdapter.Tools;
using MyLab.Search.Indexer.Options;

namespace MyLab.Search.Indexer.Services.ResourceUploading
{
    class IndexUploader
    {
        private readonly IEsTools _esTools;
        private readonly IIndexResourceProvider _idxResProvider;
        private readonly IndexerOptions _opts;
        private readonly IDslLogger _log;

        public IndexUploader(
            IEsTools esTools,
            IIndexResourceProvider idxResProvider,
            IOptions<IndexerOptions> opts,
            ILogger<IndexUploader> logger = null)
        {
            _esTools = esTools;
            _idxResProvider = idxResProvider;
            _opts = opts.Value;
            _log = logger.Dsl();
        }

        public async Task UploadAsync(CancellationToken cancellationToken)
        {
            if (_opts.Indexes is not { Length: > 0 })
            {
                _log.Warning("Configured index list is empty").Write();
                return;
            }

            foreach (var idxOpts in _opts.Indexes)
            {
                try
                {
                    var indexName = _opts.GetEsIndexName(idxOpts.Id);

                    await TouchIndex(idxOpts.Id, indexName, cancellationToken);
                }
                catch (Exception e)
                {
                    _log?.Error("Check index error", e)
                        .AndFactIs("index", idxOpts.Id)
                        .AndFactIs("es-index", idxOpts.EsIndex)
                        .Write();
                }
            }
        }

        private async Task TouchIndex(string indexId, string esIndexName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(esIndexName))
            {
                _log?.Warning("Configured index has no Elasticsearch index name")
                    .AndFactIs("index", indexId)
                    .Write();
                return;
            }

            var exists = await _esTools.Index(esIndexName).ExistsAsync(cancellationToken);
            
            if (!exists)
            {
                var settingsStr = await _idxResProvider.ProvideIndexSettingsAsync(indexId);

                await _esTools.Index(esIndexName).CreateAsync(settingsStr, cancellationToken);
            }
            else
            {
                _log?.Action("Elasticsearch index already exist")
                    .AndFactIs("index", indexId)
                    .AndFactIs("es-index", esIndexName)
                    .Write();
            }
        }
    }
}
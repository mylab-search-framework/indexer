using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.Search.EsAdapter;
using MyLab.Search.Indexer.Options;

namespace MyLab.Search.Indexer.Services
{
    class IndexCreatorService : BackgroundService
    {
        private readonly IndexerOptions _opts;
        private readonly IEsIndexTools _esIndexTools;
        private readonly IIndexResourceProvider _idxResProvider;
        private readonly IDslLogger _log;

        public IndexCreatorService(
            IOptions<IndexerOptions> opts, 
            IEsIndexTools esIndexTools, 
            IIndexResourceProvider idxResProvider,
            ILogger<IndexCreatorService> logger)
            :this(opts.Value, esIndexTools, idxResProvider, logger)
        {
        }

        public IndexCreatorService(
            IndexerOptions opts, 
            IEsIndexTools esIndexTools,
            IIndexResourceProvider idxResProvider,
            ILogger<IndexCreatorService> logger)
        {
            _opts = opts;
            _esIndexTools = esIndexTools;
            _idxResProvider = idxResProvider;
            _log = logger.Dsl();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_opts.Indexes is not { Length: > 0 })
            {
                _log.Warning("Configured index list is empty").Write();
                return;
            }

            foreach (var idxOpts in _opts.Indexes)
            {
                if (string.IsNullOrEmpty(idxOpts.EsIndex))
                {
                    _log.Warning("Configured index has no Elasticsearch index name")
                        .AndFactIs("index", idxOpts.Id)
                        .Write();
                    break;
                }

                var exists = await _esIndexTools.IsIndexExistsAsync(idxOpts.EsIndex, stoppingToken);

                if (!exists)
                {
                    var settingsStr = await _idxResProvider.ProvideSyncQueryAsync(idxOpts.EsIndex);
                    await _esIndexTools.CreateIndexAsync(idxOpts.EsIndex, settingsStr, stoppingToken);

                    _log.Action("Elasticsearch index has been created")
                        .AndFactIs("index", idxOpts.Id)
                        .AndFactIs("es-index", idxOpts.EsIndex)
                        .Write();
                }
                else
                {
                    _log.Action("Elasticsearch index already exist")
                        .AndFactIs("index", idxOpts.Id)
                        .AndFactIs("es-index", idxOpts.EsIndex)
                        .Write();
                }
            }
                
        }
    }
}

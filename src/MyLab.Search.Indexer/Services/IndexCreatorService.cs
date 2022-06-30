using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.Search.EsAdapter;
using MyLab.Search.Indexer.Options;
using RabbitMQ.Client.Exceptions;

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
            ILogger<IndexCreatorService> logger = null)
            :this(opts.Value, esIndexTools, idxResProvider, logger)
        {
        }

        public IndexCreatorService(
            IndexerOptions opts, 
            IEsIndexTools esIndexTools,
            IIndexResourceProvider idxResProvider,
            ILogger<IndexCreatorService> logger = null)
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
                bool isBrokerUnreachableException = false;

                do
                {

                    try
                    {
                        var indexName = _opts.GetEsIndexName(idxOpts.Id);

                        await CheckIndex(stoppingToken, idxOpts.Id, indexName);
                    }
                    catch (Exception e)
                    {
                        isBrokerUnreachableException = e is BrokerUnreachableException;

                        _log?.Error("Check index error", e)
                            .AndFactIs("index", idxOpts.Id)
                            .AndFactIs("es-index", idxOpts.EsIndex)
                            .Write();
                    }

                    if (isBrokerUnreachableException)
                    {
                        await Task.Delay(1000, stoppingToken);
                    }

                } while (isBrokerUnreachableException);
            }
                
        }

        private async Task CheckIndex(CancellationToken stoppingToken, string indexId, string esIndexName)
        {
            if (string.IsNullOrEmpty(esIndexName))
            {
                _log?.Warning("Configured index has no Elasticsearch index name")
                    .AndFactIs("index", indexId)
                    .Write();
                return;
            }

            var exists = await _esIndexTools.IsIndexExistsAsync(esIndexName, stoppingToken);

            await Task.Delay(500, stoppingToken);

            if (!exists)
            {
                var settingsStr = await _idxResProvider.ProvideIndexSettingsAsync(indexId);
                await _esIndexTools.CreateIndexAsync(esIndexName, settingsStr, stoppingToken);

                _log?.Action("Elasticsearch index has been created")
                    .AndFactIs("index", indexId)
                    .AndFactIs("es-index", esIndexName)
                    .Write();
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

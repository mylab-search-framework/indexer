using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.Search.EsAdapter;
using MyLab.Search.Indexer.Options;

namespace MyLab.Search.Indexer.Services
{
    class IndexCreator : IIndexCreator
    {
        private readonly IIndexResourceProvider _idxResProvider;
        private readonly IEsIndexTools _esIndexTools;
        private readonly IDslLogger _log;
        private readonly IndexerOptions _opts;

        public IndexCreator(
            IOptions<IndexerOptions> opts,
            IIndexResourceProvider idxResProvider,
            IEsIndexTools esIndexTools,
            ILogger<IndexCreator> logger = null)
            :this(opts.Value, idxResProvider, esIndexTools, logger)
        {
        }

        public IndexCreator(
            IndexerOptions opts,
            IIndexResourceProvider idxResProvider,
            IEsIndexTools esIndexTools,
            ILogger<IndexCreator> logger = null)
        {
            _idxResProvider = idxResProvider;
            _esIndexTools = esIndexTools;
            _log = logger?.Dsl();
            _opts = opts;
        }

        public async Task CreateIndex(string idxId, string esIndexName, CancellationToken stoppingToken)
        {
            var settingsStr = await _idxResProvider.ProvideIndexSettingsAsync(idxId);
            await _esIndexTools.CreateIndexAsync(esIndexName, settingsStr, stoppingToken);

            _log?.Action("Elasticsearch index has been created")
                .AndFactIs("index", idxId)
                .AndFactIs("es-index", esIndexName)
                .Write();
        }
    }
}
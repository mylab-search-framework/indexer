using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.Search.EsAdapter;
using MyLab.Search.EsAdapter.Tools;
using MyLab.Search.Indexer.Options;

namespace MyLab.Search.Indexer.Services
{
    class IndexCreator : IIndexCreator
    {
        private readonly IResourceProvider _idxResProvider;
        private readonly IEsTools _esTools;
        private readonly IDslLogger _log;
        private readonly IndexerOptions _opts;

        public IndexCreator(
            IOptions<IndexerOptions> opts,
            IResourceProvider idxResProvider,
            IEsTools esTools,
            ILogger<IndexCreator> logger = null)
            :this(opts.Value, idxResProvider, esTools, logger)
        {
        }

        public IndexCreator(
            IndexerOptions opts,
            IResourceProvider idxResProvider,
            IEsTools esTools,
            ILogger<IndexCreator> logger = null)
        {
            _idxResProvider = idxResProvider;
            _esTools = esTools;
            _log = logger?.Dsl();
            _opts = opts;
        }

        public async Task CreateIndex(string idxId, string esIndexName, CancellationToken stoppingToken)
        {
            var settingsStr = await _idxResProvider.ProvideIndexSettingsAsync(idxId);
            await _esTools.Index(esIndexName).CreateAsync(settingsStr, stoppingToken);

            _log?.Action("Elasticsearch index has been created")
                .AndFactIs("index", idxId)
                .AndFactIs("es-index", esIndexName)
                .Write();
        }
    }
}
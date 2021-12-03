using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log;
using MyLab.Log.Dsl;
using MyLab.Search.EsAdapter;
using MyLab.Search.Indexer.LogicStrategy;
using MyLab.Search.Indexer.Tools;
using MyLab.TaskApp;

namespace MyLab.Search.Indexer.Services
{
    public class IndexerTaskLogic : ITaskLogic
    {
        private readonly IndexerOptions _indexerOptions;
        private readonly IDataSourceService _dataSourceService;
        private readonly ISeedService _seedService;
        private readonly IDataIndexer _indexer;
        private readonly INamespaceResourceProvider _namespaceResourceProvider;
        private readonly IDslLogger _log;

        public IndexerTaskLogic(
            IOptions<IndexerOptions> indexerOptions, 
            IOptions<ElasticsearchOptions> esOptions,
            IOptions<IndexerDbOptions> dbOptions,
            IDataSourceService dataSourceService, 
            ISeedService seedService,
            IDataIndexer indexer,
            INamespaceResourceProvider namespaceResourceProvider,
            ILogger<IndexerTaskLogic> logger = null)
            : this(indexerOptions.Value, dataSourceService, seedService, indexer, namespaceResourceProvider, logger)
        {
            
        }

        public IndexerTaskLogic(
            IndexerOptions indexerOptions,
            IDataSourceService dataSourceService, 
            ISeedService seedService,
            IDataIndexer indexer,
            INamespaceResourceProvider namespaceResourceProvider,
            ILogger<IndexerTaskLogic> logger = null)
        {
            _indexerOptions = indexerOptions;
            _dataSourceService = dataSourceService;
            _seedService = seedService;
            _indexer = indexer;
            _namespaceResourceProvider = namespaceResourceProvider;
            _log = logger?.Dsl();
        }

        public async Task Perform(CancellationToken cancellationToken)
        {
            if (_indexerOptions.Namespaces != null)
            {
                foreach (var indexerOptionsJob in _indexerOptions.Namespaces)
                {
                    try
                    {
                        await PerformJob(indexerOptionsJob, cancellationToken);
                    }
                    catch (Exception e)
                    {
                        _log?.Error("Indexer job performing failed", e)
                            .AndFactIs("job", indexerOptionsJob)
                            .Write();
                    }
                }
            }
            else
            {
                _log?
                    .Warning("No indexing job found")
                    .Write();
            }
        }

        private async Task PerformJob(NsOptions indexerOptionsNs, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _log.Action("Indexing started")
                .Write();

            int counter = 0;

            var strategy = CreateStrategy(indexerOptionsNs);
            var seedCalc = strategy.CreateSeedCalc();

            await seedCalc.StartAsync();
            var seedParameter = await strategy.CreateSeedDataParameterAsync();

            string query;

            if (indexerOptionsNs.SyncDbQuery != null)
            {
                query = indexerOptionsNs.SyncDbQuery;
            }
            else
            {
                query = await _namespaceResourceProvider.ReadFileAsync(indexerOptionsNs.NsId, "sync.sql");
            }

            var iterator = _dataSourceService.Read(indexerOptionsNs.NsId, query, seedParameter);

            var preproc = new DsEntityPreprocessor(indexerOptionsNs);

            await foreach (var batch in iterator.WithCancellation(cancellationToken))
            {
                _log.Debug("Next batch of source data loaded")
                    .AndFactIs("count", batch.Entities.Length)
                    .Write();

                counter += batch.Entities.Length;

                var entForIndex = batch.Entities
                    .Select(preproc.Process)
                    .ToArray();

                await _indexer.IndexAsync(indexerOptionsNs.NsId, entForIndex, cancellationToken);

                seedCalc.Update(batch.Entities);
            }

            await seedCalc.SaveAsync();

            stopwatch.Stop();

            _log.Action("Indexing completed")
                .AndFactIs("count", counter)
                .AndFactIs("elapsed", stopwatch.Elapsed)
                .AndFactIs("new-seed", seedCalc.GetLogValue())
                .Write();
        }

        private IIndexerLogicStrategy CreateStrategy(NsOptions nsOptions)
        {
            switch (nsOptions.NewUpdatesStrategy)
            {
                case NewUpdatesStrategy.Update:
                    return new UpdateModeIndexerLogicStrategy(nsOptions.NsId, nsOptions.LastChangeProperty, _seedService){ Log = _log};
                case NewUpdatesStrategy.Add:
                    return new AddModeIndexerLogicStrategy(nsOptions.NsId, nsOptions.IdPropertyName, _seedService) { Log = _log };
                case NewUpdatesStrategy.Undefined:
                    throw new InvalidOperationException("Indexer mode not defined");
                default:
                    throw new InvalidOperationException("Unsupported Indexer mode")
                        .AndFactIs("mode", nsOptions.NewUpdatesStrategy);
            }
        }
    }
}
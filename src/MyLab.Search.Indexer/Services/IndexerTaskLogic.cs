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
using MyLab.Search.Indexer.Options;
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
        private readonly IIndexResourceProvider _indexResourceProvider;
        private readonly IDslLogger _log;

        public IndexerTaskLogic(
            IOptions<IndexerOptions> indexerOptions, 
            IOptions<ElasticsearchOptions> esOptions,
            IOptions<IndexerDbOptions> dbOptions,
            IDataSourceService dataSourceService, 
            ISeedService seedService,
            IDataIndexer indexer,
            IIndexResourceProvider indexResourceProvider,
            ILogger<IndexerTaskLogic> logger = null)
            : this(indexerOptions.Value, dataSourceService, seedService, indexer, indexResourceProvider, logger)
        {
            
        }

        public IndexerTaskLogic(
            IndexerOptions indexerOptions,
            IDataSourceService dataSourceService, 
            ISeedService seedService,
            IDataIndexer indexer,
            IIndexResourceProvider indexResourceProvider,
            ILogger<IndexerTaskLogic> logger = null)
        {
            _indexerOptions = indexerOptions;
            _dataSourceService = dataSourceService;
            _seedService = seedService;
            _indexer = indexer;
            _indexResourceProvider = indexResourceProvider;
            _log = logger?.Dsl();
        }

        public async Task Perform(CancellationToken cancellationToken)
        {
            if (_indexerOptions.Indexes != null)
            {
                foreach (var idxOptions in _indexerOptions.Indexes)
                {
                    try
                    {
                        await PerformNamespaceIndexing(idxOptions, cancellationToken);
                    }
                    catch (Exception e)
                    {
                        _log?.Error("Indexing performing failed", e)
                            .AndFactIs("index", idxOptions)
                            .Write();
                    }
                }
            }
            else
            {
                _log?
                    .Warning("No index found")
                    .Write();
            }
        }

        private async Task PerformNamespaceIndexing(IdxOptions idxOptions, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _log.Action("Indexing started")
                .Write();

            int counter = 0;

            var strategy = CreateStrategy(idxOptions);
            var seedCalc = strategy.CreateSeedCalc();

            await seedCalc.StartAsync();
            var seedParameter = await strategy.CreateSeedDataParameterAsync();

            string query;

            if (idxOptions.SyncDbQuery != null)
            {
                query = idxOptions.SyncDbQuery;
            }
            else
            {
                query = await _indexResourceProvider.ReadFileAsync(idxOptions.Id, "sync.sql");
            }

            var iterator = _dataSourceService.Read(idxOptions.Id, query, seedParameter);

            var preproc = new DsEntityPreprocessor(idxOptions);

            await foreach (var batch in iterator.WithCancellation(cancellationToken))
            {
                _log.Debug("Next batch of source data loaded")
                    .AndFactIs("count", batch.Entities.Length)
                    .Write();

                counter += batch.Entities.Length;

                var entForIndex = batch.Entities
                    .Select(preproc.Process)
                    .ToArray();

                await _indexer.IndexAsync(idxOptions.Id, entForIndex, cancellationToken);

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

        private IIndexerLogicStrategy CreateStrategy(IdxOptions idxOptions)
        {
            switch (idxOptions.NewUpdatesStrategy)
            {
                case NewUpdatesStrategy.Update:
                    return new UpdateModeIndexerLogicStrategy(idxOptions.Id, idxOptions.LastChangeProperty, _seedService){ Log = _log};
                case NewUpdatesStrategy.Add:
                    return new AddModeIndexerLogicStrategy(idxOptions.Id, idxOptions.IdPropertyName, _seedService) { Log = _log };
                case NewUpdatesStrategy.Undefined:
                    throw new InvalidOperationException("Indexer mode not defined");
                default:
                    throw new InvalidOperationException("Unsupported Indexer mode")
                        .AndFactIs("mode", idxOptions.NewUpdatesStrategy);
            }
        }
    }
}
using System;
using System.Diagnostics;
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
        private readonly IndexerDbOptions _dbOptions;
        private readonly IDataSourceService _dataSourceService;
        private readonly ISeedService _seedService;
        private readonly IDataIndexer _indexer;
        private readonly IDslLogger _log;
        private DbCaseOptionsValidator _optionsValidator;

        public IndexerTaskLogic(
            IOptions<IndexerOptions> indexerOptions, 
            IOptions<IndexerDbOptions> dbOptions,
            IOptions<ElasticsearchOptions> esOptions,
            IDataSourceService dataSourceService, 
            ISeedService seedService,
            IDataIndexer indexer,
            ILogger<IndexerTaskLogic> logger = null)
            : this(indexerOptions.Value, dbOptions.Value, esOptions.Value, dataSourceService, seedService, indexer, logger)
        {
            
        }

        public IndexerTaskLogic(
            IndexerOptions indexerOptions,
            IndexerDbOptions dbOptions,
            ElasticsearchOptions esOptions,
            IDataSourceService dataSourceService, 
            ISeedService seedService,
            IDataIndexer indexer,
            ILogger<IndexerTaskLogic> logger = null)
        {
            _indexerOptions = indexerOptions;
            _dbOptions = dbOptions;
            _dataSourceService = dataSourceService;
            _seedService = seedService;
            _indexer = indexer;
            _log = logger?.Dsl();
            _optionsValidator = new DbCaseOptionsValidator(indexerOptions, dbOptions, esOptions);
        }

        public async Task Perform(CancellationToken cancellationToken)
        {
            _optionsValidator.Validate();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _log.Action("Indexing started")
                .Write();

            int counter = 0;

            var strategy = CreateStrategy();
            var seedCalc = strategy.CreateSeedCalc();

            await seedCalc.StartAsync();
            var seedParameter = await strategy.CreateSeedDataParameterAsync();

            var iterator = _dataSourceService.Read(_dbOptions.Query, seedParameter);

            await foreach (var batch in iterator.WithCancellation(cancellationToken))
            {
                _log.Debug("Next batch of source data loaded")
                    .AndFactIs("count", batch.Entities.Length)
                    .Write();

                counter += batch.Entities.Length;

                await _indexer.IndexAsync(batch.Entities, cancellationToken);

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

        private IIndexerLogicStrategy CreateStrategy()
        {
            switch (_dbOptions.Strategy)
            {
                case IndexerDbStrategy.Update:
                    return new UpdateModeIndexerLogicStrategy(_indexerOptions.LastChangeProperty, _seedService){ Log = _log};
                case IndexerDbStrategy.Add:
                    return new AddModeIndexerLogicStrategy(_indexerOptions.IdProperty, _seedService) { Log = _log };
                case IndexerDbStrategy.Undefined:
                    throw new InvalidOperationException("Indexer mode not defined");
                default:
                    throw new InvalidOperationException("Unsupported Indexer mode")
                        .AndFactIs("mode", _dbOptions.Strategy);
            }
        }
    }
}
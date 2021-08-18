using System;
using System.Diagnostics;
using System.IO;
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
        private readonly IJobResourceProvider _jobResourceProvider;
        private readonly IDslLogger _log;
        private readonly DbCaseOptionsValidator _optionsValidator;

        public IndexerTaskLogic(
            IOptions<IndexerOptions> indexerOptions, 
            IOptions<ElasticsearchOptions> esOptions,
            IOptions<IndexerDbOptions> dbOptions,
            IDataSourceService dataSourceService, 
            ISeedService seedService,
            IDataIndexer indexer,
            IJobResourceProvider jobResourceProvider,
            ILogger<IndexerTaskLogic> logger = null)
            : this(indexerOptions.Value, esOptions.Value, dbOptions.Value, dataSourceService, seedService, indexer, jobResourceProvider, logger)
        {
            
        }

        public IndexerTaskLogic(
            IndexerOptions indexerOptions,
            ElasticsearchOptions esOptions,
            IndexerDbOptions dbOptions,
            IDataSourceService dataSourceService, 
            ISeedService seedService,
            IDataIndexer indexer,
            IJobResourceProvider jobResourceProvider,
            ILogger<IndexerTaskLogic> logger = null)
        {
            _indexerOptions = indexerOptions;
            _dataSourceService = dataSourceService;
            _seedService = seedService;
            _indexer = indexer;
            _jobResourceProvider = jobResourceProvider;
            _log = logger?.Dsl();
            _optionsValidator = new DbCaseOptionsValidator(indexerOptions, dbOptions, esOptions);
        }

        public async Task Perform(CancellationToken cancellationToken)
        {
            _optionsValidator.Validate();

            if (_indexerOptions.Jobs != null)
            {
                foreach (var indexerOptionsJob in _indexerOptions.Jobs)
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

        private async Task PerformJob(JobOptions indexerOptionsJob, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _log.Action("Indexing started")
                .Write();

            int counter = 0;

            var strategy = CreateStrategy(indexerOptionsJob);
            var seedCalc = strategy.CreateSeedCalc();

            await seedCalc.StartAsync();
            var seedParameter = await strategy.CreateSeedDataParameterAsync();

            string query;

            if (indexerOptionsJob.Query != null)
            {
                query = indexerOptionsJob.Query;
            }
            else
            {
                query = await _jobResourceProvider.ReadFileAsync(indexerOptionsJob.JobId, "query.sql");
            }

            var iterator = _dataSourceService.Read(indexerOptionsJob.JobId, query, seedParameter);

            await foreach (var batch in iterator.WithCancellation(cancellationToken))
            {
                _log.Debug("Next batch of source data loaded")
                    .AndFactIs("count", batch.Entities.Length)
                    .Write();

                counter += batch.Entities.Length;

                await _indexer.IndexAsync(indexerOptionsJob.JobId, batch.Entities, cancellationToken);

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

        private IIndexerLogicStrategy CreateStrategy(JobOptions jobOptions)
        {
            switch (jobOptions.NewUpdatesStrategy)
            {
                case NewUpdatesStrategy.Update:
                    return new UpdateModeIndexerLogicStrategy(jobOptions.JobId, jobOptions.LastChangeProperty, _seedService){ Log = _log};
                case NewUpdatesStrategy.Add:
                    return new AddModeIndexerLogicStrategy(jobOptions.JobId, jobOptions.IdProperty, _seedService) { Log = _log };
                case NewUpdatesStrategy.Undefined:
                    throw new InvalidOperationException("Indexer mode not defined");
                default:
                    throw new InvalidOperationException("Unsupported Indexer mode")
                        .AndFactIs("mode", jobOptions.NewUpdatesStrategy);
            }
        }
    }
}
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.LogicStrategy;
using MyLab.TaskApp;

namespace MyLab.Search.Indexer.Services
{
    public class IndexerTaskLogic : ITaskLogic
    {
        private readonly IndexerOptions _options;
        private readonly IDataSourceService _dataSourceService;
        private readonly ISeedService _seedService;
        private readonly IDataIndexer _indexer;
        private readonly IDslLogger _log;

        public IndexerTaskLogic(
            IOptions<IndexerOptions> options, 
            IDataSourceService dataSourceService, 
            ISeedService seedService,
            IDataIndexer indexer,
            ILogger<IndexerTaskLogic> logger = null)
            : this(options.Value, dataSourceService, seedService, indexer, logger)
        {
            
        }

        public IndexerTaskLogic(
            IndexerOptions options, 
            IDataSourceService dataSourceService, 
            ISeedService seedService,
            IDataIndexer indexer,
            ILogger<IndexerTaskLogic> logger = null)
        {
            _options = options;
            _dataSourceService = dataSourceService;
            _seedService = seedService;
            _indexer = indexer;
            _log = logger?.Dsl();
        }

        public async Task Perform(CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _log.Action("Indexing started")
                .AndFactIs("query", _options.Query)
                .Write();

            int counter = 0;

            var strategy = CreateStrategy();
            var seedCalc = strategy.CreateSeedCalc();

            await seedCalc.StartAsync();
            var seedParameter = await strategy.CreateSeedDataParameterAsync();

            var iterator = _dataSourceService.Read(_options.Query, seedParameter);

            await foreach (var batch in iterator.WithCancellation(cancellationToken))
            {
                _log.Debug("Next batch of source data loaded")
                    .AndFactIs("count", batch.Entities.Length)
                    .AndFactIs("options", _options)
                    .Write();

                counter += batch.Entities.Length;

                await _indexer.IndexAsync(batch.Entities);

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
            switch (_options.Mode)
            {
                case IndexerMode.Update:
                    return new UpdateModeIndexerLogicStrategy(_options.LastModifiedFieldName, _seedService);
                case IndexerMode.Add:
                    return new AddModeIndexerLogicStrategy(_options.IdFieldName, _seedService);
                case IndexerMode.Undefined:
                    throw new InvalidOperationException("Indexer mode not defined");
                default:
                    throw new InvalidOperationException("Unsupported Indexer mode")
                        .AndFactIs("mode", _options.Mode);
            }
        }

        DateTime ExtractLastModified(DataSourceEntity e)
        {
            if(_options.LastModifiedFieldName == null)
                return DateTime.MinValue;

            if (e.Properties.TryGetValue(_options.LastModifiedFieldName, out var lastModifiedFieldValue))
            {
                if (DateTime.TryParse(lastModifiedFieldValue, out var lastModified))
                {
                    return lastModified;
                }
                else
                {
                    _log.Error("Can't parse lastModified date time value")
                        .AndFactIs("actual", lastModifiedFieldValue)
                        .Write();

                    return DateTime.MinValue;
                }
            }
            else
            {
                _log.Error("LastModified field not found")
                    .AndFactIs("Expected field name", _options.LastModifiedFieldName)
                    .Write();

                return DateTime.MinValue;
            }
        }
    }
}
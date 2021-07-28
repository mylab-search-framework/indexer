using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.TaskApp;

namespace MyLab.Search.Indexer
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
            var sql = await ProvideSql();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _log.Action("Indexing started")
                .AndFactIs("query", sql)
                .Write();

            int counter = 0;

            await foreach (var batch in _dataSourceService.Read(sql).WithCancellation(cancellationToken))
            {
                _log.Debug("Next batch of source data loaded")
                    .AndFactIs("query", batch.Query)
                    .AndFactIs("count", batch.Entities.Length)
                    .Write();

                counter += batch.Entities.Length;

                await _indexer.IndexAsync(batch.Entities);
            }

            stopwatch.Stop();

            _log.Action("Indexing completed")
                .AndFactIs("count", counter)
                .AndFactIs("elapsed", stopwatch.Elapsed)
                .Write();
        }

        private async Task<string> ProvideSql()
        {
            const string seedKey = "{seed}";

            if (!_options.Query.Contains(seedKey)) return _options.Query;

            var seed = await _seedService.ReadAsync();

            return _options.Query.Replace(seedKey, seed.ToString());
        }
    }
}
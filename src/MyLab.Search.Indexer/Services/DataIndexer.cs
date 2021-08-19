using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.Search.EsAdapter;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.Services
{
    class DataIndexer : IDataIndexer
    {
        private readonly IndexerOptions _options;
        private readonly IEsIndexer<IndexEntity> _esIndexer;
        private readonly IEsManager _esManager;
        private readonly IJobResourceProvider _jobResourceProvider;
        private readonly IDslLogger _log;

        static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0 ,0);

        public DataIndexer(
            IOptions<IndexerOptions> options,
            IEsIndexer<IndexEntity> esIndexer,
            IEsManager esManager,
            IJobResourceProvider jobResourceProvider,
            ILogger<DataIndexer> logger)
        :this(options.Value, esIndexer, esManager, jobResourceProvider, logger)
        {
        }

        public DataIndexer(
            IndexerOptions options,
            IEsIndexer<IndexEntity> esIndexer, 
            IEsManager esManager,
            IJobResourceProvider jobResourceProvider,
            ILogger<DataIndexer> logger)
        {
            _options = options;
            _esIndexer = esIndexer;
            _esManager = esManager;
            _jobResourceProvider = jobResourceProvider;
            _log = logger?.Dsl();
        }

        public async Task IndexAsync(string jobId, DataSourceEntity[] dataSourceEntities, CancellationToken cancellationToken)
        {
            var curJob = _options.Jobs?.FirstOrDefault(j => j.JobId == jobId) 
                         ?? throw new InvalidOperationException("Job not found");

            if (dataSourceEntities.Length == 0)
                return;

            bool indexExists = await _esManager.IsIndexExistsAsync(curJob.EsIndex, cancellationToken);
            
            if (!indexExists)
            {
                var factory = new CreateIndexStrategyFactory(curJob, _jobResourceProvider, dataSourceEntities.First())
                {
                    Log = _log
                };

                var createIndexStrategy = await factory.CreateAsync(cancellationToken);

                _log?.Warning("Index not found and will be created")
                    .AndFactIs("index-name", curJob.EsIndex)
                    .Write();

                await createIndexStrategy.CreateIndexAsync(_esManager, curJob.EsIndex, cancellationToken);

                _log?.Action("Index created")
                    .AndFactIs("index-name", curJob.EsIndex)
                    .Write();
            }

            var indexEntities = dataSourceEntities.Select(EntityToDynamic).ToArray();

            await  _esIndexer.IndexManyAsync(indexEntities, 
                (d, doc) => d
                    .Index(curJob.EsIndex)
                    .Id(doc[curJob.IdProperty].ToString())
                , cancellationToken);
        }

        private IndexEntity EntityToDynamic(DataSourceEntity arg)
        {
            return new IndexEntity(
                arg.Properties.ToDictionary(
                    v => v.Key, 
                    v => ValueToObject(v.Value)
            ));

            object ValueToObject(DataSourcePropertyValue val)
            {
                switch (val.Type)
                {
                    case DataSourcePropertyType.Numeric:
                        return long.Parse(val.Value);
                    case DataSourcePropertyType.Double:
                        return double.Parse(val.Value, CultureInfo.InvariantCulture);
                    case DataSourcePropertyType.DateTime:
                        return (DateTime.Parse(val.Value) - Epoch).TotalMilliseconds;
                    default:
                        return val.Value;

                }
            }
        }
    }

    
}
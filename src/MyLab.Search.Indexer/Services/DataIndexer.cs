using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.Log;
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
        private readonly IIndexMappingService _indexMappingService;
        private readonly IDslLogger _log;
        
        public DataIndexer(
            IOptions<IndexerOptions> options,
            IEsIndexer<IndexEntity> esIndexer,
            IEsManager esManager,
            IJobResourceProvider jobResourceProvider,
            IIndexMappingService indexMappingService,
            ILogger<DataIndexer> logger)
        :this(options.Value, esIndexer, esManager, jobResourceProvider, indexMappingService, logger)
        {
        }

        public DataIndexer(
            IndexerOptions options,
            IEsIndexer<IndexEntity> esIndexer, 
            IEsManager esManager,
            IJobResourceProvider jobResourceProvider,
            IIndexMappingService indexMappingService,
            ILogger<DataIndexer> logger)
        {
            _options = options;
            _esIndexer = esIndexer;
            _esManager = esManager;
            _jobResourceProvider = jobResourceProvider;
            _indexMappingService = indexMappingService;
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

            var mapping = await _indexMappingService.GetIndexMappingAsync(curJob.EsIndex);
            var entitiesConverter = new DataSourceToIndexEntityConverter(mapping)
            {
                Log = _log
            };
            var indexEntities = entitiesConverter.Convert(dataSourceEntities);

            await  _esIndexer.IndexManyAsync(indexEntities, 
                (d, doc) => d
                    .Index(curJob.EsIndex)
                    .Id(doc[curJob.IdProperty].ToString())
                , cancellationToken);
        }
    }

    
}
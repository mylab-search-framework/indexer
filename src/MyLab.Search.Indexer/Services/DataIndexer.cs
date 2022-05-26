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
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.Services
{
    class DataIndexer : IDataIndexer
    {
        private readonly IndexerOptions _options;
        private readonly IEsIndexer<IndexEntity> _esIndexer;
        private readonly IEsManager _esManager;
        private readonly IIndexResourceProvider _indexResourceProvider;
        private readonly IIndexMappingService _indexMappingService;
        private readonly IEsIndexToucher _esIndexToucher;
        private readonly IDslLogger _log;
        
        public DataIndexer(
            IOptions<IndexerOptions> options,
            IEsIndexer<IndexEntity> esIndexer,
            IEsManager esManager,
            IIndexResourceProvider indexResourceProvider,
            IIndexMappingService indexMappingService,
            IEsIndexToucher esIndexToucher,
            ILogger<DataIndexer> logger)
        :this(options.Value, esIndexer, esManager, indexResourceProvider, indexMappingService, esIndexToucher, logger)
        {
        }

        public DataIndexer(
            IndexerOptions options,
            IEsIndexer<IndexEntity> esIndexer, 
            IEsManager esManager,
            IIndexResourceProvider indexResourceProvider,
            IIndexMappingService indexMappingService,
            IEsIndexToucher esIndexToucher,
            ILogger<DataIndexer> logger)
        {
            _options = options;
            _esIndexer = esIndexer;
            _esManager = esManager;
            _indexResourceProvider = indexResourceProvider;
            _indexMappingService = indexMappingService;
            _esIndexToucher = esIndexToucher;
            _log = logger?.Dsl();
        }

        public async Task IndexAsync(string nsId, DataSourceEntity[] dataSourceEntities, CancellationToken cancellationToken)
        {
            IdxOptions curIdx;

            try
            {
                curIdx = _options.GetIndexOptions(nsId);
            }
            catch (NamespaceConfigException e)
            {
                curIdx = e.IndexOptionsFromNamespaceOptions;

                _log?.Warning(e).Write();
            }

            var indexName = _options.CreateEsIndexName(nsId);

            if (dataSourceEntities.Length == 0)
                return;

            await _esIndexToucher.TouchEsIndexAsync(curIdx, dataSourceEntities.First(), cancellationToken);
            
            var mapping = await _indexMappingService.GetIndexMappingAsync(indexName);
            var entitiesConverter = new DataSourceToIndexEntityConverter(mapping)
            {
                Log = _log
            };
            var indexEntities = entitiesConverter.Convert(dataSourceEntities);

            await  _esIndexer.IndexManyAsync(indexEntities, 
                (d, doc) => d
                    .Index(indexName)
                    .Id(doc[curIdx.IdPropertyName].ToString())
                , cancellationToken);
        }
    }

    
}
﻿using System;
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
        private readonly INamespaceResourceProvider _namespaceResourceProvider;
        private readonly IIndexMappingService _indexMappingService;
        private readonly IDslLogger _log;
        
        public DataIndexer(
            IOptions<IndexerOptions> options,
            IEsIndexer<IndexEntity> esIndexer,
            IEsManager esManager,
            INamespaceResourceProvider namespaceResourceProvider,
            IIndexMappingService indexMappingService,
            ILogger<DataIndexer> logger)
        :this(options.Value, esIndexer, esManager, namespaceResourceProvider, indexMappingService, logger)
        {
        }

        public DataIndexer(
            IndexerOptions options,
            IEsIndexer<IndexEntity> esIndexer, 
            IEsManager esManager,
            INamespaceResourceProvider namespaceResourceProvider,
            IIndexMappingService indexMappingService,
            ILogger<DataIndexer> logger)
        {
            _options = options;
            _esIndexer = esIndexer;
            _esManager = esManager;
            _namespaceResourceProvider = namespaceResourceProvider;
            _indexMappingService = indexMappingService;
            _log = logger?.Dsl();
        }

        public async Task IndexAsync(string nsId, DataSourceEntity[] dataSourceEntities, CancellationToken cancellationToken)
        {
            var curNs = _options.GetNsOptions(nsId);
            var indexName = _options.GetIndexName(nsId);

            if (dataSourceEntities.Length == 0)
                return;

            bool indexExists = await _esManager.IsIndexExistsAsync(indexName, cancellationToken);
            
            if (!indexExists)
            {
                var factory = new CreateIndexStrategyFactory(curNs, _namespaceResourceProvider, dataSourceEntities.First())
                {
                    Log = _log
                };

                var createIndexStrategy = await factory.CreateAsync(cancellationToken);

                _log?.Warning("IndexAsync not found and will be created")
                    .AndFactIs("index-name", indexName)
                    .Write();

                await createIndexStrategy.CreateIndexAsync(_esManager, indexName, cancellationToken);

                _log?.Action("IndexAsync created")
                    .AndFactIs("index-name", indexName)
                    .Write();
            }

            var mapping = await _indexMappingService.GetIndexMappingAsync(indexName);
            var entitiesConverter = new DataSourceToIndexEntityConverter(mapping)
            {
                Log = _log
            };
            var indexEntities = entitiesConverter.Convert(dataSourceEntities);

            await  _esIndexer.IndexManyAsync(indexEntities, 
                (d, doc) => d
                    .Index(indexName)
                    .Id(doc[curNs.IdPropertyName].ToString())
                , cancellationToken);
        }
    }

    
}
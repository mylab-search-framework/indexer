using MyLab.Log;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.Search.EsAdapter;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Tools;
using Nest;

namespace MyLab.Search.Indexer.Services
{
    class DataIndexer : IDataIndexer
    {
        private readonly IndexerOptions _options;
        private readonly IEsIndexer<object> _indexer;
        private readonly IEsManager _esManager;
        private readonly IDslLogger _log;

        public DataIndexer(
            IOptions<IndexerOptions> options,
            IEsIndexer<object> indexer,
            IEsManager esManager,
            ILogger<DataIndexer> logger)
        :this(options.Value, indexer, esManager, logger)
        {
        }

        public DataIndexer(
            IndexerOptions options,
            IEsIndexer<object> indexer, 
            IEsManager esManager,
            ILogger<DataIndexer> logger)
        {
            _options = options;
            _indexer = indexer;
            _esManager = esManager;
            _log = logger.Dsl();
        }

        public async Task IndexAsync(DataSourceEntity[] dataSourceEntities, CancellationToken cancellationToken)
        {
            if (dataSourceEntities.Length == 0)
                return;

            bool indexExists = await _esManager.IsIndexExistsAsync(_options.IndexName, cancellationToken);
            
            if (!indexExists)
            {
                var factory = new CreateIndexStrategyFactory(_options, dataSourceEntities.First())
                {
                    Log = _log
                };

                var createIndexStrategy = await factory.CreateAsync(cancellationToken);

                await createIndexStrategy.CreateIndexAsync(_esManager, _options.IndexName, cancellationToken);
            }
            
        }
    }

    
}
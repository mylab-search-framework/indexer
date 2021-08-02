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
        private readonly IEsIndexer<IndexEntity> _esIndexer;
        private readonly IEsManager _esManager;
        private readonly IDslLogger _log;

        public DataIndexer(
            IOptions<IndexerOptions> options,
            IEsIndexer<IndexEntity> esIndexer,
            IEsManager esManager,
            ILogger<DataIndexer> logger)
        :this(options.Value, esIndexer, esManager, logger)
        {
        }

        public DataIndexer(
            IndexerOptions options,
            IEsIndexer<IndexEntity> esIndexer, 
            IEsManager esManager,
            ILogger<DataIndexer> logger)
        {
            _options = options;
            _esIndexer = esIndexer;
            _esManager = esManager;
            _log = logger?.Dsl();
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

            var indexEntities = dataSourceEntities.Select(EntityToDynamic).ToArray();

            await  _esIndexer.IndexManyAsync(indexEntities, 
                (d, doc) => d
                    .Index(_options.IndexName)
                    .Id(ExtractId(doc))
                , cancellationToken);
        }

        private Id ExtractId(IndexEntity doc)
        {
            return doc[_options.IdFieldName].ToString();
        }

        private IndexEntity EntityToDynamic(DataSourceEntity arg)
        {
            return new IndexEntity(
                arg.Properties.ToDictionary(
                    v => v.Key, 
                    v => (object)v.Value.Value
            ));
        }
    }

    
}
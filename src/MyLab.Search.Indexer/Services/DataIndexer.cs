using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyLab.Elastic;
using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.Services
{
    class DataIndexer : IDataIndexer
    {
        private readonly IndexerOptions _options;
        private readonly IEsIndexer<IndexEntity> _indexer;
        private readonly IEsManager _esManager;

        public DataIndexer(
            IOptions<IndexerOptions> options,
            IEsIndexer<IndexEntity> indexer,
            IEsManager esManager)
        :this(options.Value, indexer, esManager)
        {
        }

        public DataIndexer(
            IndexerOptions options,
            IEsIndexer<IndexEntity> indexer, 
            IEsManager esManager)
        {
            _options = options;
            _indexer = indexer;
            _esManager = esManager;
        }

        public async Task IndexAsync(DataSourceEntity[] dataSourceEntities, CancellationToken cancellationToken)
        {
            if (dataSourceEntities.Length == 0)
                return;

            bool indexExists = await _esManager.IsIndexExistsAsync(_options.IndexName, cancellationToken);
            
            if (!indexExists)
            {
                var mapper = new EsMapper();

                await _esManager.CreateIndexAsync(_options.IndexName, 
                    c => c.Map(
                        mapper.CreateMapping(dataSourceEntities.First())
                        ), cancellationToken);
            }
        }
    }
}
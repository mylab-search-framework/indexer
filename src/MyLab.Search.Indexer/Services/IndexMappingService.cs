using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyLab.Log.Dsl;
using MyLab.Log;
using MyLab.Search.EsAdapter;
using Nest;

namespace MyLab.Search.Indexer.Services
{
    class IndexMappingService : IIndexMappingService
    {
        private readonly IEsClientProvider _esClientProvider;
        private readonly IDslLogger _log;
        private readonly ConcurrentDictionary<string, IndexMapping> _nsToIndexMapping = new ConcurrentDictionary<string, IndexMapping>();

        public IndexMappingService(
            IEsClientProvider esClientProvider,
            ILogger<IndexMappingService> logger = null)
        {
            _esClientProvider = esClientProvider;
            _log = logger?.Dsl();
        }

        public async Task<IndexMapping> GetIndexMappingAsync(string indexName)
        {
            if (_nsToIndexMapping.TryGetValue(indexName, out var currentMapping))
                return currentMapping;


            var client = _esClientProvider.Provide();

            var mappingResponse = await client.Indices.GetMappingAsync(new GetMappingRequest(indexName));
            
            if (!mappingResponse.Indices.TryGetValue(indexName, out var indexMapping))
                throw new InvalidOperationException("IndexAsync mapping not found")
                    .AndFactIs("index", indexName);

            var propertiesMapping = indexMapping?.Mappings?.Properties;

            if (propertiesMapping == null)
                throw new InvalidOperationException("IndexAsync properties mapping not found")
                    .AndFactIs("index", indexName);

            currentMapping = new IndexMapping(propertiesMapping.Values.Select(p => new IndexMappingProperty(p.Name.Name, p.Type)));
            _nsToIndexMapping.TryAdd(indexName, currentMapping);

            return currentMapping;
        }
    }
}
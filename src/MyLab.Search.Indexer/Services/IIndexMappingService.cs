using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Services
{
    interface IIndexMappingService
    {
        Task<IndexMapping> GetIndexMappingAsync(string indexName);
    }

    class IndexMapping
    {
        public IReadOnlyCollection<IndexMappingProperty> Props { get; }

        public IndexMapping(IEnumerable<IndexMappingProperty> props)
        {
            Props = new ReadOnlyCollection<IndexMappingProperty>(props.ToArray());
        }
    }

    class IndexMappingProperty
    {
        public string Name { get; }
        public string Type { get; }

        public IndexMappingProperty(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}

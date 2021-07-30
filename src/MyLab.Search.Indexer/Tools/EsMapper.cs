using System;
using MyLab.Search.Indexer.Services;
using Nest;

namespace MyLab.Search.Indexer.Tools
{
    class EsMapper
    {
        public Func<TypeMappingDescriptor<IndexEntity>, ITypeMapping> CreateMapping(DataSourceEntity dataSourceEntity)
        {
            return d =>
            {
                return d.Properties(propD =>
                {
                    var buffD = propD;

                    foreach (var property in dataSourceEntity.Properties)
                    {
                        
                    }

                    return buffD;
                });
            };
        }
    }
}

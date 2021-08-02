using System;
using System.Threading;
using System.Threading.Tasks;
using MyLab.Log;
using MyLab.Log.Dsl;
using MyLab.Search.EsAdapter;
using MyLab.Search.Indexer.DataContract;
using Nest;

namespace MyLab.Search.Indexer.Tools
{
    class AutoSettingsBasedCreateIndexStrategy : ICreateIndexStrategy
    {
        private readonly DataSourceEntity _entityExample;

        public IDslLogger Log { get; set; }

        public AutoSettingsBasedCreateIndexStrategy(DataSourceEntity entityExample)
        {
            _entityExample = entityExample;
        }

        public Task CreateIndexAsync(IEsManager esMgr, string name, CancellationToken cancellationToken)
        {
            return esMgr.CreateIndexAsync(name, d => d.Map(m => CreateMapping(m, _entityExample)), cancellationToken);
        }

        private ITypeMapping CreateMapping(TypeMappingDescriptor<object> typeMappingDescriptor, DataSourceEntity entityExample)
        {
            var md = typeMappingDescriptor;
            
            md.Properties(pd => CreatePropertiesDescriptions(pd, entityExample));

            return md;
        }

        private IPromise<IProperties> CreatePropertiesDescriptions(PropertiesDescriptor<object> pd, DataSourceEntity entityExample)
        {
            var d = pd;

            foreach (var property in entityExample.Properties)
            {
                switch (property.Value.Type)
                {
                    case DataSourcePropertyType.Boolean:
                        d = d.Boolean(td => td.Name(property.Key));
                        break;
                    case DataSourcePropertyType.Numeric:
                        d = d.Number(td => td.Name(property.Key).Type(NumberType.Long));
                        break;
                    case DataSourcePropertyType.Double:
                        d = d.Number(td => td.Name(property.Key).Type(NumberType.Double));
                        break;
                    case DataSourcePropertyType.DateTime:
                        d = d.Date(td => td.Name(property.Key));
                        break;
                    case DataSourcePropertyType.Undefined:
                    case DataSourcePropertyType.String:
                        d = d.Text(td => td.Name(property.Key));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Unexpected example entity property type", (Exception)null)
                            .AndFactIs("actual", property.Value.Type);
                }
            }

            return d;
        }
    }
}
using System;
using System.Linq;
using System.Text;
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
            
            md.Properties(pd =>
            {
                var res = CreatePropertiesDescriptions(pd, entityExample);

                var s = MappingToString(res.Value);

                Log?.Debug("Index mapping created")
                    .AndFactIs("mapping", res.Value.Count != 0 ? MappingToString(res.Value) : (object)"[empty]")
                    .AndFactIs("example", entityExample)
                    .Write();

                return res;
            });

            return md;
        }

        private string MappingToString(IProperties resValue)
        {
            var sb = new StringBuilder();

            foreach (var property in resValue)
            {
                sb.Append(property.Key.Name + "\n");

                if (property.Value.Name != null)
                {
                    var pn = property.Value.Name;

                    if (pn.Property != null || pn.Expression != null)
                    {
                        sb.Append("\tName:\n");

                        if (pn.Name != null)
                            sb.Append($"\t\tName: {pn.Name}\n");
                        if (pn.Property != null)
                            sb.Append($"\t\tProperty: {pn.Property.Name}\n");
                        if (pn.Expression != null)
                            sb.Append($"\t\tExpression: {pn.Expression}");
                    }
                    else
                    {
                        sb.Append($"\tName: {pn.Name}\n");
                    }
                }
                else
                {
                    sb.Append("\tName: [null]\n");
                }

                sb.Append($"\tType: {property.Value.Type ?? "[null]"}\n");

                if (property.Value.Meta != null)
                {
                    sb.Append("\tMeta:\n");
                    foreach (var meta in property.Value.Meta)
                    {
                        sb.Append($"\t\t'{meta.Key}':'{meta.Value}'\n");
                    }
                }

                if (property.Value.LocalMetadata!= null)
                {
                    sb.Append("\tLocalMetadata:\n");
                    foreach (var meta in property.Value.LocalMetadata)
                    {
                        sb.Append($"\t\t'{meta.Key}':'{meta.Value}'\n");
                    }
                }
            }

            return sb.ToString();
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
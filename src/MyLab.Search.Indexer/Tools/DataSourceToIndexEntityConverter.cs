using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MyLab.Log;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.Tools
{
    class DataSourceToIndexEntityConverter
    {
        static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0);

        private readonly IndexMapping _mapping;

        public IDslLogger Log { get; set; }

        public DataSourceToIndexEntityConverter(IndexMapping mapping)
        {
            _mapping = mapping;
        }

        public IndexEntity[] Convert(IEnumerable<DataSourceEntity> dataSourceEntities)
        {
            return dataSourceEntities.Select(Convert).ToArray();
        }

        IndexEntity Convert(DataSourceEntity dsEntity)
        {
            var props = new Dictionary<string, object>();

            foreach (var property in dsEntity.Properties)
            {
                if(property.Value.Value == null)
                    continue;

                object objVal;

                var mappingProperty = _mapping.Props.FirstOrDefault(p => p.Name == property.Key);
                if (mappingProperty == null)
                {
                    Log?.Debug("Property not found in index")
                        .AndFactIs("name", property.Key)
                        .AndFactIs("db-info", property.Value)
                        .Write();

                    objVal = ValueToObjectWithDataSourceType(property.Value.Value, property.Value.DbType);
                }
                else
                {
                    objVal = ValueToObjectWithMappingType(property.Value.Value, mappingProperty.Type);
                }

                props.Add(property.Key, objVal);
            }

            return new IndexEntity(props);

        }

        private object ValueToObjectWithMappingType(string strVal, string mappingType)
        {
            try
            {
                switch (mappingType)
                {
                    case "boolean": 
                        return bool.Parse(strVal);
                    case "long":
                    case "integer":
                    case "short":
                    case "byte":
                        return long.Parse(strVal);
                    case "double":
                    case "float":
                        return double.Parse(strVal, CultureInfo.InvariantCulture);
                    case "date":
                        return (DateTime.Parse(strVal) - Epoch).TotalMilliseconds;
                    default:
                        return strVal;
                }
            }
            catch (FormatException e)
            {
                e.AndFactIs("value", strVal)
                    .AndFactIs("mapping-type", mappingType);

                throw;
            }
            
        }

        object ValueToObjectWithDataSourceType(string strVal, DataSourcePropertyType dataSourceType)
        {
            try
            {
                switch (dataSourceType)
                {
                    case DataSourcePropertyType.Numeric:
                        return long.Parse(strVal);
                    case DataSourcePropertyType.Double:
                        return double.Parse(strVal, CultureInfo.InvariantCulture);
                    case DataSourcePropertyType.DateTime:
                        return (DateTime.Parse(strVal) - Epoch).TotalMilliseconds;
                    default:
                        return strVal;

                }
            }
            catch (FormatException e)
            {
                e.AndFactIs("value", strVal)
                    .AndFactIs("db-type", dataSourceType);

                throw;
            }
        }
    }
}

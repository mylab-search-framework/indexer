using System.Collections.Generic;
using System.Data;
using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.DataContract
{
    public class DataSourceEntity
    {
        public IDictionary<string, DataSourcePropertyValue> Properties { get; set; }

        public static DataSourceEntity ReadEntity(IDataReader reader)
        {
            var resEnt = new DataSourceEntity
            {
                Properties = new Dictionary<string, DataSourcePropertyValue>()
            };

            for (var index = 0; index < reader.FieldCount; index++)
            {
                var name = reader.GetName(index);
                var typeName = reader.GetDataTypeName(index);
                var value = new DataSourcePropertyValue
                {
                    DbType = DataSourcePropertyTypeConverter.Convert(typeName),
                    PropertyTypeReason = typeName
                };

                if (!reader.IsDBNull(index))
                    value.Value = reader.GetString(index);

                resEnt.Properties.Add(name, value);
            }

            return resEnt;
        }
    }
}
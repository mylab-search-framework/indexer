using System;
using System.Globalization;
using System.Linq;
using MyLab.Search.Indexer.DataContract;
using Newtonsoft.Json;

namespace MyLab.Search.Indexer.Tools
{
    class SourceEntityDeserializer
    {
        private readonly bool _detectTypes;

        public SourceEntityDeserializer(bool detectTypes)
        {
            _detectTypes = detectTypes;
        }

        public DataSourceEntity Deserialize(string stringData)
        {
            var entity = new DataSourceEntity();

            var xml = JsonConvert.DeserializeXNode(stringData, deserializeRootElementName: "root");

            if (xml?.Root != null)
            {
                entity.Properties = xml.Root
                    .Elements()
                    .ToDictionary(
                        e => e.Name.LocalName,
                        e => new DataSourcePropertyValue
                        {
                            Value = e.Value,
                            Type = _detectTypes ? DetectType(e.Value) : DataSourcePropertyType.Undefined,
                            PropertyTypeReason = _detectTypes ? e.Value : "[auto-detection-disabled]"
                        }
                    );
            }

            return entity;
        }

        private DataSourcePropertyType DetectType(string argValue)
        {
            var cult = CultureInfo.InvariantCulture;

            if (bool.TryParse(argValue, out _))
                return DataSourcePropertyType.Boolean;
            if (long.TryParse(argValue, NumberStyles.Any, cult, out _))
                return DataSourcePropertyType.Numeric;
            if (double.TryParse(argValue, NumberStyles.Any, cult, out _))
                return DataSourcePropertyType.Double;
            if (DateTime.TryParse(argValue, out _))
                return DataSourcePropertyType.DateTime;

            return DataSourcePropertyType.String;
        }
    }
}
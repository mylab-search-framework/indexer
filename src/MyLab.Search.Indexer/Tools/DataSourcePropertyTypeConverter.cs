using System;
using System.Linq;
using MyLab.Search.Indexer.DataContract;

namespace MyLab.Search.Indexer.Tools
{
    static class DataSourcePropertyTypeConverter
    {
        private static readonly string[] DoubleCases = new[] {"decimal", "double", "float", "single", "real"};
        

        public static DataSourcePropertyType Convert(string typeName)
        {
            var sc = StringComparison.InvariantCultureIgnoreCase;

            if (typeName.Contains("int", sc))
                return DataSourcePropertyType.Numeric;

            if (DoubleCases.Any(c => c.Equals(typeName, sc)))
                return DataSourcePropertyType.Double;

            if (typeName.Contains("bool", sc) || typeName.Equals("bit", sc))
                return DataSourcePropertyType.Boolean;

            if (typeName.Contains("date", sc))
                return DataSourcePropertyType.DateTime;

            if (typeName.Contains("char", sc))
                return DataSourcePropertyType.String;

            return DataSourcePropertyType.Undefined;
        }
    }
}
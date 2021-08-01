using System.Collections.Generic;

namespace MyLab.Search.Indexer.DataContract
{
    public class DataSourceEntity
    {
        public IDictionary<string, DataSourcePropertyValue> Properties { get; set; }
    }
}
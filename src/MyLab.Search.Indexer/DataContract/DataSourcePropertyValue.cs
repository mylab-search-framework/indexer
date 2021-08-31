namespace MyLab.Search.Indexer.DataContract
{
    public class DataSourcePropertyValue
    {
        public string PropertyTypeReason{ get; set; }
        public DataSourcePropertyType DbType { get; set; }
        public string Value { get; set; }
    }
}
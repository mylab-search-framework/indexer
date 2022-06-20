namespace MyLab.Search.Indexer.DataContract
{
    public class DataSourceBatch
    {
        public string Query { get; set; }
        public DataSourceEntity[] Entities { get; set; }
    }
}
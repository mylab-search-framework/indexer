using MyLab.Search.Indexer.DataContract;

namespace MyLab.Search.Indexer.Services
{
    public class DataSourceBatch
    {
        public string Query { get; set; }
        public DataSourceEntity[] Entities { get; set; }
    }
}
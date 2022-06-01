using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.Models
{
    public class DataSourceLoad
    {
        public DataSourceLoadBatch[] Batches { get; set; }
        public ISeedSaver SeedSaver { get; set; }
    }

    public class DataSourceLoadBatch
    {
        public IndexingEntity[] Entities { get; set; }
        public string Query { get; set; }
    }
}
using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.Models
{
    public class DataSourceDeltaLoad
    {
        public IndexingRequestEntity[] Entities { get; set; }

        public ISeedSaver SeedSaver { get; set; }
    }
}
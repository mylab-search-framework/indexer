using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.Models
{
    public class DataSourceLoad
    {
        public IndexingRequestEntity[] Entities { get; set; }

        public ISeedSaver SeedSaver { get; set; }
    }
}
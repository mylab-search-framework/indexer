using MyLab.Search.Indexer.Tools;
using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Models
{
    public class DataSourceLoad
    {
        public DataSourceLoadBatch Batch{ get; set; }
        public ISeedSaver SeedSaver { get; set; }
    }

    public class DataSourceLoadBatch
    {
        public JObject[] Docs { get; set; }
        public string Query { get; set; }
    }
}
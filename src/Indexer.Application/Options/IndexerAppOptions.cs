namespace Indexer.Application.Options
{
    public class IndexerAppOptions
    {
        public string? IndexPrefix { get; set; }
        public string? IndexSuffix { get; set; }

        public Dictionary<string,string>? IndexMap { get; set; }
    }
}

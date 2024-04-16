namespace MyLab.Search.Indexer.Model
{
    public class IndexingRequest
    {
        public LiteralId? IndexId { get; set; }
        public IndexingObject[]? PutList { get; set; }
        public IndexingObject[]? PatchList { get; set; }
        public LiteralId[]? DeleteList { get; set; }
    }
}

namespace MyLab.Search.Indexer.Model
{
    public class IndexInstructions
    {
        public required LiteralId IndexId { get; set; }
        public IndexingObject[]? PutList { get; set; }
        public IndexingObject[]? PatchList { get; set; }
        public LiteralId[]? DeleteList { get; set; }
    }
}

namespace MyLab.Search.Indexer.Model
{
    public class IndexInstructions
    {
        public required LiteralId IndexId { get; set; }
        public IReadOnlyList<IndexingObject>? PutList { get; set; }
        public IReadOnlyList<IndexingObject>? PatchList { get; set; }
        public IReadOnlyList<LiteralId>? DeleteList { get; set; }
    }
}

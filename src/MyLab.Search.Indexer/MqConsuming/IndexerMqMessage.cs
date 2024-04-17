using MyLab.Search.Indexer.Model;

namespace MyLab.Search.Indexer.MqConsuming
{
    public class IndexerMqMessage
    {
        public LiteralId? IndexId { get; set; }
        public IndexingObject[]? PutList { get; set; }
        public IndexingObject[]? PatchList { get; set; }
        public LiteralId[]? DeleteList { get; set; }
    }
}

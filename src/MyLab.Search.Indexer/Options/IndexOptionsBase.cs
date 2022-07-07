namespace MyLab.Search.Indexer.Options
{
    public class IndexOptionsBase
    {
        public IndexType IndexType { get; set; } = IndexType.Heap;
        public IdPropertyType IdPropertyType { get; set; }
    }
}
namespace MyLab.Search.Indexer
{
    public class IndexerOptions
    {
        public string DbProvider { get; set; }
        public int PageSize { get; set; }
        public string Query { get; set; }
        public string LastModifiedFieldName { get; set; }
        public string IdFieldName { get; set; }
        public bool EnablePaging { get; set; } = false;
        public IndexerMode Mode { get; set; }
    }

    public enum IndexerMode
    {
        Undefined,
        Update,
        Add
    }
}

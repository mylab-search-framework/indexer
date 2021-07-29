namespace MyLab.Search.Indexer
{
    public class IndexerOptions
    {
        public string DbProvider { get; set; }
        public int PageSize { get; set; }
        public string Query { get; set; }
        public string LastModifiedFieldName { get; set; }
        public bool EnablePaging { get; set; } = false;
    }
}

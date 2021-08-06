namespace MyLab.Search.Indexer
{
    public class IndexerDbOptions
    {
        public string Provider { get; set; }
        public int PageSize { get; set; }
        public bool EnablePaging { get; set; } = false;
        public IndexerDbStrategy Strategy { get; set; }
        public string Query { get; set; }
    }

    public class IndexerMqOptions
    {
        public string Queue { get; set; }
    }

    public class IndexerOptions
    {
        public string NewIndexRequestFile { get; set; } = "/etc/mylab-indexer/new-index-request.json";
        public NewIndexStrategy NewIndexStrategy { get; set; }
        public string LastChangeProperty { get; set; }
        public string IdProperty { get; set; }
    }

    public enum IndexerDbStrategy
    {
        Undefined,
        Update,
        Add
    }

    public enum NewIndexStrategy
    {
        Undefined,
        Auto,
        File
    }
}

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
        public string IndexSettingsPath { get; set; } = "/etc/mylab-indexer/index-settings.json";
        public EntityMappingMode EntityMappingMode { get; set; }
        public string LastModifiedFieldName { get; set; }
        public string IdFieldName { get; set; }
    }

    public enum IndexerDbStrategy
    {
        Undefined,
        Update,
        Add
    }

    public enum EntityMappingMode
    {
        Undefined,
        Auto,
        SettingsFile
    }
}

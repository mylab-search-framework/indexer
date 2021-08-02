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
        public IndexerScanMode ScanMode { get; set; }
        public string IndexName { get; set; }
        public string IndexSettingsPath { get; set; } = "/etc/mylab-indexer/index-settings.json";

        public EntityMappingMode EntityMappingMode { get; set; }

        public string MqQueue { get; set; }
    }

    public enum IndexerScanMode
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

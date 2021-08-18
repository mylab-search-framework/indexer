namespace MyLab.Search.Indexer
{
    public class IndexerDbOptions
    {
        public string Provider { get; set; }
    }

    public class IndexerOptions
    {
        public string JobPath { get; set; } = "/etc/mylab-indexer/jobs";
        public string SeedPath { get; set; } = "/var/lib/mylab-indexer/seeds";
        public JobOptions[] Jobs { get; set; }
    }

    public class JobOptions
    {
        public string JobId { get; set; }
        public string MqQueue { get; set; }
        public NewUpdatesStrategy NewUpdatesStrategy { get; set; }

        public NewIndexStrategy NewIndexStrategy { get; set; }
        public string LastChangeProperty { get; set; }
        public string IdProperty { get; set; }

        public int PageSize { get; set; }
        public bool EnablePaging { get; set; } = false;

        public string Query { get; set; }
    }

    public enum NewUpdatesStrategy
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

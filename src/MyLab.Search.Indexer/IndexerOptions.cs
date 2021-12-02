using System;
using System.Linq;
using MyLab.Log;

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
        public string IndexNamePrefix { get; set; }
        public string IndexNamePostfix { get; set; }
        public JobOptions[] Jobs { get; set; }

        public JobOptions GetJobOptions(string jobId)
        {
            var jobOptions = Jobs?.FirstOrDefault(n => n.JobId == jobId);
            if (jobOptions == null)
                throw new InvalidOperationException("Job options not found")
                    .AndFactIs("job-id", jobId);

            return jobOptions;
        }

        public string GetIndexName(string jobId)
        {
            var jobOptions = GetJobOptions(jobId);
            return $"{IndexNamePrefix ?? string.Empty}{jobOptions.EsIndex}{IndexNamePostfix ?? string.Empty}";
        }
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
        public string DbQuery { get; set; }
        public string EsIndex { get; set; }
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

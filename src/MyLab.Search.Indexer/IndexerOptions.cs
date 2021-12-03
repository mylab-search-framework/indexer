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
        public string NamespacesPath { get; set; } = "/etc/mylab-indexer/namespaces";
        public string SeedPath { get; set; } = "/var/lib/mylab-indexer/seeds";
        public string IndexNamePrefix { get; set; }
        public string IndexNamePostfix { get; set; }
        public NsOptions[] Namespaces { get; set; }

        public NsOptions GetNsOptions(string nsId)
        {
            var nsOptions = Namespaces?.FirstOrDefault(n => n.NsId == nsId);
            if (nsOptions == null)
                throw new InvalidOperationException("Namespace options not found")
                    .AndFactIs("namespace-id", nsId);

            return nsOptions;
        }

        public string GetIndexName(string nsId)
        {
            var nsOptions = GetNsOptions(nsId);
            return $"{IndexNamePrefix ?? string.Empty}{nsOptions.EsIndex}{IndexNamePostfix ?? string.Empty}";
        }
    }

    public class NsOptions
    {
        public string NsId { get; set; }
        public string MqQueue { get; set; }
        public NewUpdatesStrategy NewUpdatesStrategy { get; set; }
        public NewIndexStrategy NewIndexStrategy { get; set; }
        public string LastChangeProperty { get; set; }
        public string IdPropertyName { get; set; }
        public IdPropertyType IdPropertyType { get; set; }
        public int PageSize { get; set; }
        public bool EnablePaging { get; set; } = false;
        public string DbQuery { get; set; }
        public string KickQuery { get; set; }
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

    public enum IdPropertyType
    {
        Undefined,
        Text,
        Integer
    }
}

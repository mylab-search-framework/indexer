using System;
using System.Linq;

namespace MyLab.Search.Indexer.Options
{
    public class IndexerOptions
    {
        public string AppId { get; set; } = "mylab-indexer";
        public IndexOptions[] Indexes { get; set; }
        public IndexOptionsBase DefaultIndex { get; set; } = new();
        public string SeedPath { get; set; } = "/var/lib/mylab-indexer/seeds";
        public string ResourcesPath { get; set; } = "/etc/mylab-indexer";
        public string MqQueue { get; set; }
        public string EsNamePrefix { get; set; }
        public string EsNamePostfix { get; set; }
        public bool EnableEsIndexAutoCreation { get; set; } = false;
        public bool EnableEsStreamAutoCreation { get; set; } = false;

        public int SyncPageSize { get; set; } = 500;

        public IndexOptions GetIndexOptions(string indexId)
        {
            var foundOptions = GetIndexOptionsCore(indexId);

            if (foundOptions == null)
                throw new IndexOptionsNotFoundException(indexId);

            return foundOptions;
        }

        public string GetEsName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));

            return $"{EsNamePrefix?.ToLower()}{name.ToLower()}{EsNamePostfix?.ToLower()}";
        }

        public bool IsIndexAStream(string idxId)
        {
            var idxOpts = GetIndexOptionsCore(idxId);

            if (idxOpts != null)
                return idxOpts.IsStream;
            
            return DefaultIndex is { IsStream: true };
        }

        IndexOptions GetIndexOptionsCore(string indexId)
        {
            if (string.IsNullOrEmpty(indexId))
                throw new InvalidOperationException("Index id not specified");

            return Indexes?.FirstOrDefault(i => i.Id == indexId);
        }
    }
}

using System;
using System.Linq;
using MyLab.Log;

namespace MyLab.Search.Indexer.Options
{
    public class IndexerOptions
    {
        public IndexOptions[] Indexes { get; set; }
        public IndexOptionsBase DefaultIndexOptions { get; set; }
        public string SeedPath { get; set; } = "/var/lib/mylab-indexer/seeds";

        [Obsolete("Use ResourcesPath instead")]
        public string ResourcePath { get; set; } = "/etc/mylab-indexer/indexes";
        public string ResourcesPath { get; set; } = "/etc/mylab-indexer";
        public string MqQueue { get; set; }

        [Obsolete("Use EsNamePrefix instead")]
        public string EsIndexNamePrefix { get; set; }
        [Obsolete("Use EsNamePostfix instead")]
        public string EsIndexNamePostfix { get; set; }
        public string EsNamePrefix { get; set; }
        public string EsNamePostfix { get; set; }

        public IndexOptions GetIndexOptions(string indexId)
        {
            var foundOptions = GetIndexOptionsCore(indexId);

            if (foundOptions == null)
                throw new IndexOptionsNotFoundException(indexId);

            return foundOptions;
        }

        [Obsolete]
        public string GetEsIndexName(string idxId)
        {
            return GetEsName(idxId);
        }

        public string GetEsName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));

            var idxOpt = GetIndexOptionsCore(name);

            var totalIdxName = idxOpt?.EsIndex ?? name;

            return $"{(EsIndexNamePrefix ?? EsNamePrefix)?.ToLower()}{totalIdxName.ToLower()}{(EsIndexNamePostfix ?? EsNamePostfix)?.ToLower()}";
        }

        public IdPropertyType GetTotalIdPropertyType(string idxId)
        {
            var idxOpts = GetIndexOptionsCore(idxId);

            if (idxOpts != null)
                return idxOpts.IdPropertyType;

            if (DefaultIndexOptions == null)
                throw new InvalidOperationException("Unable to determine index id property type")
                    .AndFactIs("idx", idxId);

            return DefaultIndexOptions.IdPropertyType;
        }

        public IndexType GetTotalIndexType(string idxId)
        {
            var idxOpts = GetIndexOptionsCore(idxId);

            if (idxOpts != null)
                return idxOpts.IndexType;

            if (DefaultIndexOptions == null)
                return IndexType.Heap;

            return DefaultIndexOptions.IndexType;
        }

        IndexOptions GetIndexOptionsCore(string indexId)
        {
            if (string.IsNullOrEmpty(indexId))
                throw new InvalidOperationException("Index id not specified");

            return Indexes?.FirstOrDefault(i => i.Id == indexId);
        }
    }
}

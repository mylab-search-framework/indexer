using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MyLab.Log;

namespace MyLab.Search.Indexer.Options
{
    public class IndexerOptions
    {
        public IndexOptions[] Indexes { get; set; }
        public IndexOptionsBase DefaultIndexOptions { get; set; }
        public string SeedPath { get; set; } = "/var/libs/mylab-indexer/seeds";
        public string ResourcePath { get; set; } = "/etc/mylab-indexer/indexes";
        public string MqQueue { get; set; }
        public string EsIndexNamePrefix { get; set; }
        public string EsIndexNamePostfix { get; set; }

        public IndexOptions GetIndexOptions(string indexId)
        {
            var foundOptions = GetIndexOptionsCore(indexId);

            if (foundOptions == null)
                throw new IndexOptionsNotFoundException(indexId);

            return foundOptions;
        }

        public string GetEsIndexName(string idxId)
        {
            var idxOpt = GetIndexOptions(idxId);

            if (string.IsNullOrEmpty(idxOpt.EsIndex))
                throw new InvalidOperationException("Elasticsearch index name is not defined")
                    .AndFactIs("idx", idxId);

            return $"{EsIndexNamePrefix?.ToLower()}{idxOpt.EsIndex.ToLower()}{EsIndexNamePostfix?.ToLower()}";
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

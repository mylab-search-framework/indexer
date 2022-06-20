using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MyLab.Log;

namespace MyLab.Search.Indexer.Options
{
    public class IndexerOptions
    {
        public IndexOptions[] Indexes { get; set; }
        public string SeedPath { get; set; } = "/var/libs/mylab-indexer/seeds";
        public string ResourcePath { get; set; } = "/etc/mylab-indexer/indexes";
        public string MqQueue { get; set; }
        public string EsIndexNamePrefix { get; set; }
        public string EsIndexNamePostfix { get; set; }

        public IndexOptions GetIndexOptions(string indexId)
        {
            if (string.IsNullOrEmpty(indexId))
                throw new InvalidOperationException("Index id not specified");

            var foundOptions = Indexes?.FirstOrDefault(i => i.Id == indexId);

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

            return $"{EsIndexNamePrefix}{idxOpt.EsIndex}{EsIndexNamePostfix}";
        }
    }

    public class IndexOptions
    {
        public string Id { get; set; }
        public IndexType IndexType { get; set; } = IndexType.Heap;
        public string EsIndex { get; set; }
        public string KickDbQuery { get; set; }
        public string SyncDbQuery { get; set; }
        public IdPropertyType IdPropertyType { get; set; }
        public bool EnableSync { get; set; } = true;
        public int SyncPageSize { get; set; } = 500;

        public void ValidateIdPropertyType()
        {
            if (IdPropertyType == IdPropertyType.Undefined)
                throw new ValidationException("'" + nameof(IdPropertyType) + " index option is not defined")
                    .AndFactIs("index-id", Id);
        }
    }

    public class IndexOptionsNotFoundException : Exception
    {
        public string IndexName { get; }

        public IndexOptionsNotFoundException(string indexName)
        {
            IndexName = indexName;
            this.AndFactIs("index-name", indexName);
        }
    }

    public enum IdPropertyType
    {
        Undefined,
        String,
        Int
    }

    public enum IndexType
    {
        Undefined,
        Heap,
        Stream
    }
}

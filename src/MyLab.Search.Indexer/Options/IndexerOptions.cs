using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MyLab.Log;

namespace MyLab.Search.Indexer.Options
{
    public class IndexerOptions
    {
        public IndexOptions[] Indexes { get; set; }

        public string MqQueue { get; set; }

        public IndexOptions GetIndexOptions(string indexName)
        {
            var foundOptions = Indexes?.FirstOrDefault(i => i.Id == indexName);

            if (foundOptions == null)
                throw new IndexOptionsNotFoundException(indexName);

            return foundOptions;
        }
    }

    public class IndexOptions
    {
        public string Id { get; set; }
        public IndexType IndexType { get; set; }

        public string KickDbQuery { get; set; }
        public string SyncDbQuery { get; set; }

        public IdPropertyType IdPropertyType { get; set; }

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

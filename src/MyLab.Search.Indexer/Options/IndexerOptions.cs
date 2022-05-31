using System;
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
        public bool IsStream { get; set; }
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
}

using System;
using MyLab.Log;

namespace MyLab.Search.Indexer.Options
{
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
using System;

namespace MyLab.Search.Indexer.Tools
{
    static class UnderneathName
    {
        public static string New(string indexName)
        {
            return $@".ml-{indexName}-{Guid.NewGuid():N}";
        }

        public static string ToPattern(string indexName)
        {
            return $@".ml-{indexName}-*";
        }

    }
}

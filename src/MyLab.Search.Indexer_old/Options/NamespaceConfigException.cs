using System;

namespace MyLab.Search.Indexer.Options
{
    class NamespaceConfigException : Exception
    {
        public IdxOptions IndexOptionsFromNamespaceOptions { get; }
        
        public NamespaceConfigException(IdxOptions indexOptionsFromNamespaceOptions)
            :base("An old config with 'namespaces' instead of 'indexes' detected")
        {
            IndexOptionsFromNamespaceOptions = indexOptionsFromNamespaceOptions;
        }
    }
}
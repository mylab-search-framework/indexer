using System;
using System.Linq;
using MyLab.Log;

namespace MyLab.Search.Indexer.Options
{
    public class IndexerDbOptions
    {
        public string Provider { get; set; }
    }

    public class IndexerOptions
    {
        public string NamespacesPath { get; set; } = "/etc/mylab-indexer/namespaces";
        public string IndexPath { get; set; } = "/etc/mylab-indexer/indexes";
        public string SeedPath { get; set; } = "/var/lib/mylab-indexer/seeds";
        public string EsIndexNamePrefix { get; set; }
        [Obsolete]
        public string IndexNamePrefix { get; set; }
        public string EsIndexNamePostfix { get; set; }
        [Obsolete]
        public string IndexNamePostfix { get; set; }

        [Obsolete]
        public NsOptions[] Namespaces { get; set; }
        public IdxOptions[] Indexes { get; set; }

        public IdxOptions GetIndexOptions(string indexId)
        {
            var indexOptions = Indexes?.FirstOrDefault(n => n.Id == indexId);
            if (indexOptions == null)
            {
                var nsOptions = Namespaces?.FirstOrDefault(n => n.NsId == indexId);

                if (nsOptions == null)
                {
                    throw new IndexOptionsNotFoundException(indexId)
                        .AndFactIs("index-id", indexId);
                }

                indexOptions = new IdxOptions(nsOptions);

                throw new NamespaceConfigException(indexOptions);
            }

            return indexOptions;
        }

        public string CreateEsIndexName(string nsId)
        {
            IdxOptions nsOptions;

            try
            {
                nsOptions = GetIndexOptions(nsId);
            }
            catch (NamespaceConfigException e)
            {
                nsOptions = e.IndexOptionsFromNamespaceOptions;
            }

            return $"{EsIndexNamePrefix ?? IndexNamePrefix ?? string.Empty}{nsOptions.EsIndex}{EsIndexNamePostfix ?? IndexNamePostfix ?? string.Empty}";
        }
    }

    [Obsolete]
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
        public string SyncDbQuery { get; set; }
        public string KickDbQuery { get; set; }
        public string EsIndex { get; set; }
    }

    public class IdxOptions
    {
        public string Id { get; set; }
        public string MqQueue { get; set; }
        public NewUpdatesStrategy NewUpdatesStrategy { get; set; }
        public NewIndexStrategy NewIndexStrategy { get; set; }
        public string LastChangeProperty { get; set; }
        public string IdPropertyName { get; set; }
        public IdPropertyType IdPropertyType { get; set; }
        public int PageSize { get; set; }
        public bool EnablePaging { get; set; } = false;
        public string SyncDbQuery { get; set; }
        public string KickDbQuery { get; set; }
        public string EsIndex { get; set; }
        
        public IdxOptions()
        {
            
        }
        
        public IdxOptions(NsOptions nsOptions)
        {
            Id = nsOptions.NsId;
            MqQueue = nsOptions.NsId;
            NewUpdatesStrategy = nsOptions.NewUpdatesStrategy;
            NewIndexStrategy = nsOptions.NewIndexStrategy;
            LastChangeProperty = nsOptions.LastChangeProperty;
            IdPropertyName = nsOptions.IdPropertyName;
            IdPropertyType = nsOptions.IdPropertyType;
            PageSize = nsOptions.PageSize;
            EnablePaging = nsOptions.EnablePaging;
            SyncDbQuery = nsOptions.SyncDbQuery;
            KickDbQuery = nsOptions.KickDbQuery;
            EsIndex = nsOptions.EsIndex;
        }
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
        String,
        Int
    }

    class NamespaceConfigException : Exception
    {
        public IdxOptions IndexOptionsFromNamespaceOptions { get; }
        
        public NamespaceConfigException(IdxOptions indexOptionsFromNamespaceOptions)
            :base("An old config with 'namespaces' instead of 'indexes' detected")
        {
            IndexOptionsFromNamespaceOptions = indexOptionsFromNamespaceOptions;
        }
    }

    class IndexOptionsNotFoundException : Exception
    {
        public string IndexName { get; }

        public IndexOptionsNotFoundException(string indexName)
            :base("Index options not found")
        {
            IndexName = indexName;
        }
    }
}

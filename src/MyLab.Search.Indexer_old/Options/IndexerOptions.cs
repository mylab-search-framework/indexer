using System;
using System.Linq;
using MyLab.Log;

namespace MyLab.Search.Indexer.Options
{
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

                throw new NamespaceConfigException(indexOptions)
                    .AndFactIs("index-id", indexId);
            }

            return indexOptions;
        }

        public string CreateEsIndexName(string idxId)
        {
            IdxOptions idxOptions;

            try
            {
                idxOptions = GetIndexOptions(idxId);
            }
            catch (NamespaceConfigException e)
            {
                idxOptions = e.IndexOptionsFromNamespaceOptions;
            }

            return $"{EsIndexNamePrefix ?? IndexNamePrefix ?? string.Empty}{idxOptions.EsIndex}{EsIndexNamePostfix ?? IndexNamePostfix ?? string.Empty}";
        }
    }
}

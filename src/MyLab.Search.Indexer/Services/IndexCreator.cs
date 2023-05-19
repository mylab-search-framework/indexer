using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log;
using MyLab.Log.Dsl;
using MyLab.Log.Scopes;
using MyLab.Search.EsAdapter.Tools;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services.ResourceUploading;
using MyLab.Search.Indexer.Tools;
using Nest;

namespace MyLab.Search.Indexer.Services
{
    class IndexCreator : IIndexCreator
    {
        private readonly IIndexMappingProvider _indexMappingProvider;
        private readonly IEsTools _esTools;
        private readonly IDslLogger _log;
        private readonly IndexerOptions _opts;

        public IndexCreator(
            IOptions<IndexerOptions> opts,
            IIndexMappingProvider indexMappingProvider,
            IEsTools esTools,
            ILogger<IndexCreator> logger = null)
            :this(opts.Value, indexMappingProvider, esTools, logger)
        {
        }

        public IndexCreator(
            IndexerOptions opts,
            IIndexMappingProvider indexMappingProvider,
            IEsTools esTools,
            ILogger<IndexCreator> logger = null)
        {
            _indexMappingProvider = indexMappingProvider;
            _esTools = esTools;
            _log = logger?.Dsl();
            _opts = opts;
        }

        public async Task CreateIndexAsync(string idxId, string esIndexName, CancellationToken stoppingToken)
        {
            using (_log.BeginScope(new LabelLogScope(new Dictionary<string, string>
                   {
                       {"index-id", idxId},
                       {"index-name", esIndexName}
                   })))
            {
                try
                {
                    try
                    {
                        var mappingStr = await _indexMappingProvider.ProvideAsync(idxId);

                        if (!_opts.EnableEsIndexAutoCreation)
                            throw new IndexCreationDeniedException();

                        await CreateEsIndexCoreAsync(esIndexName, mappingStr, stoppingToken);
                    }
                    catch (FileNotFoundException)
                    {
                        var idxOpts = _opts.Indexes?.FirstOrDefault(i => i.Id == idxId);

                        var idxType = idxOpts?.IndexType ?? _opts.DefaultIndexOptions.IndexType;

                        switch (idxType)
                        {
                            case IndexType.Heap:
                            {
                                if (!_opts.EnableEsIndexAutoCreation)
                                    throw new IndexCreationDeniedException();
                                await CreateEsIndexCoreAsync(esIndexName, null, stoppingToken);
                            }
                                break;
                            case IndexType.Stream:
                            {
                                if (!_opts.EnableEsStreamAutoCreation)
                                    throw new IndexCreationDeniedException();
                                await CreateEsStreamCoreAsync(esIndexName, stoppingToken);
                            }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
                catch (IndexCreationDeniedException e)
                {
                    e.AndFactIs("index-id", idxId)
                        .AndFactIs("index-name", esIndexName);

                    throw;
                }
            }
        }

        private async Task CreateEsStreamCoreAsync(string esIndexName, CancellationToken stoppingToken)
        {
            await _esTools.Stream(esIndexName).CreateAsync(stoppingToken);

            _log?.Action("Elasticsearch stream has been created")
                .AndFactIs("stream-name", esIndexName)
                .Write();
        }

        private async Task CreateEsIndexCoreAsync(string esIndexName, IndexMappingDesc indexMapping, CancellationToken stoppingToken)
        {
            if (indexMapping != null)
            {
                var mappingMeta = new MappingMetadata
                {
                    Creator = new MappingMetadata.CreatorDesc
                    {
                        Owner = _opts.AppId,
                        SourceHash = indexMapping.SourceHash
                    }
                };
                
                var metaDict = indexMapping.Mapping.Meta ??= new Dictionary<string, object>();
                mappingMeta.Save(metaDict);

                ICreateIndexRequest req = new CreateIndexRequest(esIndexName)
                {
                    Mappings = indexMapping.Mapping
                };

                await _esTools.Index(esIndexName).CreateAsync(d => req, stoppingToken);
            }
            else
            {
                await _esTools.Index(esIndexName).CreateAsync(d => d
                        .Map(md => md)
                    , stoppingToken);
            }

            _log?.Action("Elasticsearch index has been created")
                .AndFactIs("index-name", esIndexName)
                .Write();
        }
    }

    public class IndexCreationDeniedException : Exception
    {
        public IndexCreationDeniedException() : base("Index creation was denied due to settings")
        {
            
        }
    }
}
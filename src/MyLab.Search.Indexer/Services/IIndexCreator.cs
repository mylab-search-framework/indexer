using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.Log.Scopes;
using MyLab.Search.EsAdapter.Tools;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services.ComponentUploading;
using Nest;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using MyLab.Log;

namespace MyLab.Search.Indexer.Services
{
    interface IIndexCreator
    {
        Task CreateIndexAsync(string idxId, CancellationToken stoppingToken);
    }

    class IndexCreator : IIndexCreator
    {
        private readonly IResourceProvider _resourceProvider;
        private readonly IEsTools _esTools;
        private readonly IDslLogger _log;
        private readonly IndexerOptions _opts;

        public IndexCreator(
            IOptions<IndexerOptions> opts,
            IResourceProvider resourceProvider,
            IEsTools esTools,
            ILogger<IndexCreator> logger = null)
            : this(opts.Value, resourceProvider, esTools, logger)
        {
        }

        public IndexCreator(
            IndexerOptions opts,
            IResourceProvider resourceProvider,
            IEsTools esTools,
            ILogger<IndexCreator> logger = null)
        {
            _resourceProvider = resourceProvider;
            _esTools = esTools;
            _log = logger?.Dsl();
            _opts = opts;
        }

        public async Task CreateIndexAsync(string idxId, CancellationToken stoppingToken)
        {
            string esIndexName = _opts.GetEsName(idxId);

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
                        var mappingObj = _resourceProvider.ProvideIndexMapping(idxId);

                        if (!_opts.EnableEsIndexAutoCreation)
                            throw new IndexCreationDeniedException();

                        await CreateEsIndexCoreAsync(esIndexName, mappingObj, stoppingToken);
                    }
                    catch (FileNotFoundException)
                    {
                        if (_opts.IsIndexAStream(idxId))
                        {
                            if (!_opts.EnableEsStreamAutoCreation)
                                throw new IndexCreationDeniedException();
                            await CreateEsStreamCoreAsync(esIndexName, stoppingToken);
                        }
                        else
                        {
                            if (!_opts.EnableEsIndexAutoCreation)
                                throw new IndexCreationDeniedException();
                            await CreateEsIndexCoreAsync(esIndexName, null, stoppingToken);
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

        private async Task CreateEsIndexCoreAsync(string esIndexName, IResource<TypeMapping> indexMapping, CancellationToken stoppingToken)
        {
            var mappingMeta = new MappingMetadata
            {
                Creator = new MappingMetadata.CreatorDesc
                {
                    Owner = _opts.AppId,
                    SourceHash = indexMapping?.Hash
                }
            };

            var metaDict = indexMapping?.Content.Meta ?? new Dictionary<string, object>();
            mappingMeta.Save(metaDict);

            if (indexMapping != null)
            {
                indexMapping.Content.Meta = metaDict;

                ICreateIndexRequest req = new CreateIndexRequest(esIndexName)
                {
                    Mappings = indexMapping.Content
                };

                await _esTools.Index(esIndexName).CreateAsync(d => req, stoppingToken);
            }
            else
            {
                await _esTools.Index(esIndexName).CreateAsync(d => d
                        .Map(md => md.Meta(new Dictionary<string, object>(metaDict)))
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

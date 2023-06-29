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
        Task<CreatedIndexDescription> CreateIndexAsync(string idxId, CancellationToken stoppingToken);
    }

    record CreatedIndexDescription(string Id, string Name, string Alias, bool IsStream, IAsyncDisposable Deleter);

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

        public async Task<CreatedIndexDescription> CreateIndexAsync(string idxId, CancellationToken stoppingToken)
        {
            if (!_opts.EnableAutoCreation)
                throw new IndexCreationDeniedException();

            string idxAlias = _opts.GetEsName(idxId);
            var idxName = idxAlias + "-" + Guid.NewGuid().ToString("N");

            bool streamCreated = false;
            IAsyncDisposable deleter;

            using (_log.BeginScope(new LabelLogScope(new Dictionary<string, string>
                   {
                       {"index-id", idxId},
                       {"index-name", idxName},
                       {"alias-name", idxAlias}
                   })))
            {
                var mappingObj = _resourceProvider.ProvideIndexMapping(idxId);

                if (mappingObj != null)
                {
                    deleter = await CreateEsIndexCoreAsync(idxAlias, idxName, mappingObj, stoppingToken);
                }
                else
                {
                    if (_opts.IsIndexAStream(idxId))
                    {
                        deleter = await CreateEsStreamCoreAsync(idxAlias, idxName, stoppingToken);
                        streamCreated = true;
                    }
                    else
                    {
                        deleter = await CreateEsIndexCoreAsync(idxAlias, idxName, null, stoppingToken);
                    }
                }
            }

            return new CreatedIndexDescription(idxId, idxName, idxAlias, streamCreated, deleter);
        }

        private async Task<IAsyncDisposable> CreateEsStreamCoreAsync(string alias, string name, CancellationToken stoppingToken)
        {
            var deleter = await _esTools.Stream(name).CreateAsync(stoppingToken);
            await _esTools.Stream(name).Alias(alias).PutAsync(null, stoppingToken);

            _log?.Action("A stream has been created").Write();

            return deleter;
        }

        private async Task<IAsyncDisposable> CreateEsIndexCoreAsync(string alias, string name, IResource<TypeMapping> indexMapping, CancellationToken stoppingToken)
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

            IAsyncDisposable deleter;

            if (indexMapping != null)
            {
                indexMapping.Content.Meta = metaDict;

                ICreateIndexRequest req = new CreateIndexRequest(name)
                {
                    Mappings = indexMapping.Content
                };

                deleter = await _esTools.Index(name).CreateAsync(d => req, stoppingToken);
            }
            else
            {
                deleter = await _esTools.Index(name).CreateAsync(d => d
                        .Map(md => md.Meta(new Dictionary<string, object>(metaDict)))
                    , stoppingToken);
            }

            await _esTools.Index(name).Alias(alias).PutAsync(null, stoppingToken);

            _log?.Action("An index has been created").Write();

            return deleter;
        }
    }

    public class IndexCreationDeniedException : Exception
    {
        public IndexCreationDeniedException() : base("Index creation was denied due to settings")
        {

        }
    }
}

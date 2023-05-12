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
using Nest;

namespace MyLab.Search.Indexer.Services
{
    class IndexCreator : IIndexCreator
    {
        private readonly IResourceProvider _idxResProvider;
        private readonly IEsTools _esTools;
        private readonly IDslLogger _log;
        private readonly IndexerOptions _opts;

        public IndexCreator(
            IOptions<IndexerOptions> opts,
            IResourceProvider idxResProvider,
            IEsTools esTools,
            ILogger<IndexCreator> logger = null)
            :this(opts.Value, idxResProvider, esTools, logger)
        {
        }

        public IndexCreator(
            IndexerOptions opts,
            IResourceProvider idxResProvider,
            IEsTools esTools,
            ILogger<IndexCreator> logger = null)
        {
            _idxResProvider = idxResProvider;
            _esTools = esTools;
            _log = logger?.Dsl();
            _opts = opts;
        }

        public async Task CreateIndex(string idxId, string esIndexName, CancellationToken stoppingToken)
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
                        var mappingStr = await _idxResProvider.ProvideIndexMappingAsync(idxId);

                        if (!_opts.EnableEsIndexAutoCreation)
                            throw new IndexNotFoundException();

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
                                    throw new IndexNotFoundException();
                                await CreateEsIndexCoreAsync(esIndexName, null, stoppingToken);
                            }
                                break;
                            case IndexType.Stream:
                            {
                                if (!_opts.EnableEsStreamAutoCreation)
                                    throw new IndexNotFoundException();
                                await CreateEsStreamCoreAsync(esIndexName, stoppingToken);
                            }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
                catch (IndexNotFoundException e)
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

        private async Task CreateEsIndexCoreAsync(string esIndexName, string settingsStr, CancellationToken stoppingToken)
        {
            if (settingsStr != null)
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(settingsStr));
                var mapping = _esTools.Serializer.Deserialize<TypeMapping>(stream);

                ICreateIndexRequest req = new CreateIndexRequest(esIndexName)
                {
                    Mappings = mapping
                };

                await _esTools.Index(esIndexName).CreateAsync(d => req, stoppingToken);
            }
            else
            {
                await _esTools.Index(esIndexName).CreateAsync(d => d, stoppingToken);
            }

            _log?.Action("Elasticsearch index has been created")
                .AndFactIs("index-name", esIndexName)
                .Write();
        }
    }

    public class IndexNotFoundException : Exception
    {
        public IndexNotFoundException() : base("Index not found")
        {
            
        }
    }
}
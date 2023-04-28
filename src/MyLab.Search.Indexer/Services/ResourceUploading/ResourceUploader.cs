using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.Log.Scopes;
using MyLab.Search.EsAdapter.Tools;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.Services.ResourceUploading
{
    class ResourceUploader<TEsComponent> : IResourceUploader
    {
        private readonly IResourceUploaderStrategy<TEsComponent> _strategy;
        private readonly IEsTools _esTools;
        private readonly IResourceProvider _resourceProvider;
        private readonly IDslLogger _log;
        private readonly IndexerOptions _opts;

        protected ResourceUploader(
            IResourceUploaderStrategy<TEsComponent> strategy,
            IEsTools esTools,
            IResourceProvider resourceProvider,
            IOptions<IndexerOptions> options,
            ILogger logger = null)
        {
            _strategy = strategy;
            _esTools = esTools;
            _resourceProvider = resourceProvider;
            _opts = options.Value;
            _log = logger?.Dsl();
        }

        public async Task UploadAsync(CancellationToken cancellationToken)
        {
            var resources = _strategy.GetResources(_resourceProvider);

            if (resources.Length != 0)
            {
                _log?.Action($"{_strategy.ResourceSetName} uploading")
                    .AndFactIs("list", resources.Select(p => p.Name).ToArray())
                    .Write();

                foreach (var resource in resources)
                {
                    using (_log?.BeginScope(new FactLogScope("resource-name", resource.Name)))
                    {
                        await TryUploadResourceAsync(resource, cancellationToken);
                    }
                }
            }
            else
            {
                _log?.Action($"No {_strategy.ResourceSetName.ToLower()} found").Write();
            }
        }

        private async Task TryUploadResourceAsync(IResource resource, CancellationToken cancellationToken)
        {
            var resId = _opts.GetEsName(resource.Name);

            try
            {
                await using var readStream = resource.OpenRead();
                var resourceComponent = _strategy.DeserializeComponent(_esTools.Serializer, readStream);

                if (_strategy.HasAbsentNode(resourceComponent, out var absentNodeName))
                {
                    _log?.Error($"{_strategy.OneResourceName} resource has no inner node")
                        .AndFactIs("absent-node", absentNodeName)
                        .Write();

                    return;
                }

                var resComponentMetaDict = _strategy.ProvideMeta(resourceComponent);
                var resComponentMetadata = ServiceMetadata.Extract(resComponentMetaDict);
                
                var esComponent = await _strategy.TryGetComponentFromEsAsync(resId, _esTools, cancellationToken);
                
                if (esComponent == null)
                {
                    _log?.Action($"{_strategy.OneResourceName} not found in ES and will be uploaded").Write();

                    var newInitialMetadata = new ServiceMetadata
                    {
                        Creator = ServiceMetadata.MyCreator,
                        Ver = resComponentMetadata?.Ver,
                        History = new ServiceMetadata.HistoryItem[]
                        {
                            new ()
                            {
                                ComponentVer = resComponentMetadata?.Ver,
                                ActorVer = GetMyActorVersion(),
                                ActDt = DateTime.Now,
                                Actor = ServiceMetadata.MyCreator
                            }
                        }
                    };
                    
                    _strategy.ApplyMetadata(resourceComponent, newInitialMetadata);

                    await _strategy.UploadComponentAsync(resId, resourceComponent, _esTools, cancellationToken);

                    _log?.Action($"{_strategy.OneResourceName} was uploaded").Write();

                    return;
                }

                var esMeta = _strategy.ProvideMeta(esComponent);
                var esComponentMetadata = ServiceMetadata.Extract(esMeta);

                if (esComponentMetadata == null || !esComponentMetadata.IsMyCreator())
                {
                    _log?.Warning($"The same {_strategy.ResourceSetName.ToLower()} from another creator was found")
                        .AndFactIs("creator", esComponentMetadata?.Creator)
                        .Write();

                    return;
                }

                if (resComponentMetadata?.Ver == esComponentMetadata.Ver)
                {
                    _log?.Action($"Upload canceled due to actual {_strategy.ResourceSetName.ToLower()} version")
                        .AndFactIs("ver", resComponentMetadata?.Ver)
                        .Write();

                    return;
                }

                var newHistory = new List<ServiceMetadata.HistoryItem>();

                if (esComponentMetadata.History is { Length: > 0 })
                {
                    newHistory.AddRange(esComponentMetadata.History);
                }

                newHistory.Add(new ServiceMetadata.HistoryItem
                {
                    Actor = ServiceMetadata.MyCreator,
                    ActDt = DateTime.Now,
                    ActorVer = GetMyActorVersion(),
                    ComponentVer = resComponentMetadata?.Ver
                });

                var newMetadata = new ServiceMetadata
                {
                    Creator = esComponentMetadata.Creator,
                    Ver = resComponentMetadata?.Ver,
                    History = newHistory.Count > 0 ? newHistory.ToArray() : null
                };

                _strategy.ApplyMetadata(resourceComponent, newMetadata);

                await _strategy.UploadComponentAsync(resId, resourceComponent, _esTools, cancellationToken);
            }
            catch (Exception e)
            {
                _log.Error($"Unable to upload {_strategy.ResourceSetName.ToLower()}", e)
                    .Write();
            }
        }

        string GetMyActorVersion() => typeof(ResourceUploader<>).Assembly.GetName().Version?.ToString();
    }
}
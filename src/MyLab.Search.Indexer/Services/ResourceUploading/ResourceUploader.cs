using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
        private readonly IndexerOptions _options;

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
            _options = options.Value;
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

        private async Task TryUploadResourceAsync(IResource<TEsComponent> resource, CancellationToken cancellationToken)
        {
            var resId = _options.GetEsName(resource.Name);

            try
            {
                var resourceComponent = resource.Content;

                if (_strategy.HasAbsentNode(resourceComponent, out var absentNodeName))
                {
                    _log?.Error($"{_strategy.OneResourceName} resource has no inner node")
                        .AndFactIs("absent-node", absentNodeName)
                        .Write();

                    return;
                }

                var esComponent = await _strategy.TryGetComponentFromEsAsync(resId, _esTools, cancellationToken);
                IDictionary<string, object> resultMeta = new Dictionary<string, object>(
                    _strategy.ProvideMeta(resourceComponent) ?? new Dictionary<string, object>());

                var componentMetadata = new ComponentMetadata
                {
                    Owner = _options.AppId
                };

                if (esComponent == null)
                {
                    _log?.Action($"{_strategy.OneResourceName} not found in ES and will be uploaded").Write();

                    componentMetadata.SourceHash = resource.Hash;
                    componentMetadata.Save(resultMeta);
                    
                    _strategy.SetMeta(resource.Name, _options.AppId, resourceComponent, resultMeta);

                    await _strategy.UploadComponentAsync(resId, resourceComponent, _esTools, cancellationToken);

                    _log?.Action($"{_strategy.OneResourceName} was uploaded").Write();
                    
                    return;
                }

                if(ComponentMetadata.TryGet(resultMeta, out var esSrvMetadata))
                {
                    if (esSrvMetadata.Owner != _options.AppId)
                    {
                        _log.Warning("Another owner component detected")
                            .AndFactIs("my-app-id", _options.AppId)
                            .AndFactIs("component-owner", esSrvMetadata.Owner)
                            .Write();

                        return;
                    }

                    if (HashCalculator.NormalizeHash(esSrvMetadata.SourceHash) == resource.Hash)
                    {

                        _log?.Action($"Uploading canceled due to actual {_strategy.ResourceSetName.ToLower()} version")
                            .AndFactIs("hash", resource.Hash)
                            .Write();

                        return;
                    }
                }

                _log?.Action($"{_strategy.OneResourceName} has different version and will be uploaded").Write();

                componentMetadata.SourceHash = resource.Hash;
                componentMetadata.Save(resultMeta);

                _strategy.SetMeta(resource.Name, _options.AppId, resourceComponent, resultMeta);

                await _strategy.UploadComponentAsync(resId, resourceComponent, _esTools, cancellationToken);

                _log?.Action($"{_strategy.OneResourceName} was uploaded").Write();
            }
            catch (Exception e)
            {
                _log.Error($"Unable to upload {_strategy.ResourceSetName.ToLower()}", e)
                    .Write();
            }
        }
    }
}
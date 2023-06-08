using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyLab.Log.Dsl;

namespace MyLab.Search.Indexer.Services.ComponentUploading
{
    class StartupService : BackgroundService
    {
        private readonly IResourceProvider _resourceProvider;
        private readonly IComponentUploader[] _resourceUploaders;
        private readonly IDslLogger _log;

        public StartupService(
            IServiceProvider serviceProvider,
            IResourceProvider resourceProvider,
            ILogger<StartupService> logger = null)
        {
            _resourceProvider = resourceProvider;
            _resourceUploaders = new IComponentUploader[]
            {
                ActivatorUtilities.CreateInstance<LifecyclePolicyUploader>(serviceProvider),
                ActivatorUtilities.CreateInstance<IndexTemplateUploader>(serviceProvider),
                ActivatorUtilities.CreateInstance<ComponentTemplateUploader>(serviceProvider)
            };

            _log = logger.Dsl();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await UploadComponents(stoppingToken);

            try
            {
                await _resourceProvider.LoadAsync(stoppingToken);
            }
            catch (Exception e)
            {
                _log.Error("Resource loading error", e).Write();
            }
        }

        private async Task UploadComponents(CancellationToken stoppingToken)
        {
            foreach (var uploader in _resourceUploaders)
            {
                if(stoppingToken.IsCancellationRequested)
                    return;

                try
                {
                    await uploader.UploadAsync(stoppingToken);
                }
                catch (Exception e)
                {
                    _log?.Error("Resources uploading error", e)
                        .AndFactIs("uploader", uploader.GetType().Name)
                        .Write();
                }
            }
        }
    }
}

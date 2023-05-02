using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyLab.Log.Dsl;

namespace MyLab.Search.Indexer.Services.ResourceUploading
{
    class StartupResourceUploaderService : BackgroundService
    {
        private readonly IResourceUploader[] _uploaders;
        private readonly IDslLogger _log;

        public StartupResourceUploaderService(
            IServiceProvider serviceProvider,
            ILogger<StartupResourceUploaderService> logger = null)
        {
            _uploaders = new IResourceUploader[]
            {
                ActivatorUtilities.CreateInstance<LifecyclePolicyUploader>(serviceProvider),
                ActivatorUtilities.CreateInstance<IndexTemplateUploader>(serviceProvider),
                ActivatorUtilities.CreateInstance<ComponentTemplateUploader>(serviceProvider)
            };

            _log = logger.Dsl();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var uploader in _uploaders)
            {
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

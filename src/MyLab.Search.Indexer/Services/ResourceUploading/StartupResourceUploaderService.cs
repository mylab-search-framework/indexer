using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MyLab.Search.Indexer.Services.ResourceUploading
{
    class StartupResourceUploaderService : BackgroundService
    {
        private readonly IResourceUploader[] _uploaders;

        public StartupResourceUploaderService(
            IServiceProvider serviceProvider)
        {
            _uploaders = new[]
            {
                ActivatorUtilities.CreateInstance<LifecyclePolicyUploader>(serviceProvider)
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var uploader in _uploaders)
                await uploader.UploadAsync(stoppingToken);
        }
    }
}

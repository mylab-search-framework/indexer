using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyLab.Search.EsAdapter;

namespace MyLab.Search.Indexer.Services.ResourceUploading
{
    class StartupResourceUploaderService : BackgroundService
    {
        private readonly IndexUploader _indexUploader;

        public StartupResourceUploaderService(
            IServiceProvider serviceProvider)
        {
            _indexUploader = ActivatorUtilities.CreateInstance<IndexUploader>(serviceProvider);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _indexUploader.UploadAsync(stoppingToken);
        }
    }
}

using System.Threading;
using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Services.ResourceUploading
{
    interface IResourceUploader
    {
        Task UploadAsync(CancellationToken cancellationToken);
    }
}
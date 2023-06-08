using System.Threading;
using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Services.ComponentUploading
{
    interface IComponentUploader
    {
        Task UploadAsync(CancellationToken cancellationToken);
    }
}
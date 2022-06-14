using System.Threading;
using System.Threading.Tasks;
using MyLab.Search.Indexer.Models;

namespace MyLab.Search.Indexer.Services
{
    public interface IIndexerService
    {
        Task IndexAsync(IndexingRequest req, CancellationToken cToken = default);
    }
}

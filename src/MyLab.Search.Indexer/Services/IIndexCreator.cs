using System.Threading;
using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Services
{
    interface IIndexCreator
    {
        Task CreateIndexAsync(string idxId, CancellationToken stoppingToken);
    }
}

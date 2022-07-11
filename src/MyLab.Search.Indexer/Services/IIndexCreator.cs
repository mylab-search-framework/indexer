using System.Threading;
using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Services
{
    interface IIndexCreator
    {
        Task CreateIndex(string idxId, string esIndexName, CancellationToken stoppingToken);
    }
}

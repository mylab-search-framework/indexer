using System.Threading.Tasks;
using MyLab.Search.Indexer.Models;

namespace MyLab.Search.Indexer.Services
{
    public interface IInputRequestProcessor
    {
        Task ProcessRequestAsync(IndexingRequest request);
    }
}

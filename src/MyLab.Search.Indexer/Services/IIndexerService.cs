using System.Threading.Tasks;
using MyLab.Search.Indexer.Models;

namespace MyLab.Search.Indexer.Services
{
    public interface IIndexerService
    {
        Task IndexEntities(IndexingRequest indexingRequest);
    }
}

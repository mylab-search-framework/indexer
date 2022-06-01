using System.Threading.Tasks;
using MyLab.Search.Indexer.Models;

namespace MyLab.Search.Indexer.Services
{
    public interface IDataSourceService
    {
        Task<IndexingRequestEntity[]> LoadEntitiesAsync(string indexId, string[] idList);
    }
}

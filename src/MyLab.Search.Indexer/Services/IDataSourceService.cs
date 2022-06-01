using System.Threading.Tasks;
using MyLab.Search.Indexer.Models;

namespace MyLab.Search.Indexer.Services
{
    public interface IDataSourceService
    {
        Task<IndexingRequestEntity[]> LoadByIdListAsync(string indexId, string[] idList);

        Task<DataSourceDeltaLoad> LoadDeltaAsync(string indexId);
    }
}

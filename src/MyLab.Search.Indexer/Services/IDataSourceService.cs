using System.Threading.Tasks;
using MyLab.Search.Indexer.Models;

namespace MyLab.Search.Indexer.Services
{
    public interface IDataSourceService
    {
        Task<DataSourceLoad> LoadKickAsync(string indexId, string[] idList);

        Task<DataSourceLoad> LoadSyncAsync(string indexId);
    }
}

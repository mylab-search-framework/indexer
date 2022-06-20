using System.Collections.Generic;
using System.Threading.Tasks;
using MyLab.Search.Indexer.Models;

namespace MyLab.Search.Indexer.Services
{
    public interface IDataSourceService
    {
        Task<DataSourceLoad> LoadKickAsync(string indexId, string[] idList);

        Task<IAsyncEnumerable<DataSourceLoad>> LoadSyncAsync(string indexId);
    }
}

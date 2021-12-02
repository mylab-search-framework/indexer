using System.Threading;
using System.Threading.Tasks;
using MyLab.Search.Indexer.DataContract;

namespace MyLab.Search.Indexer.Services
{
    public interface IDataIndexer
    {
        Task IndexAsync(string jobId, DataSourceEntity[] dataSourceEntities, CancellationToken cancellationToken);
    }
}
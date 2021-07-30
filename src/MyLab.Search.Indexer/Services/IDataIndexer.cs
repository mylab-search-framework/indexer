using System.Threading;
using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Services
{
    public interface IDataIndexer
    {
        Task IndexAsync(DataSourceEntity[] dataSourceEntities, CancellationToken cancellationToken);
    }
}

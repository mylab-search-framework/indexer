using System.Threading.Tasks;

namespace MyLab.Search.Indexer
{
    public interface IDataIndexer
    {
        Task IndexAsync(DataSourceEntity[] dataSourceEntities);
    }
}

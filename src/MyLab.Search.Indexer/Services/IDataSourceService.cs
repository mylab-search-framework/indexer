using System.Collections.Generic;
using System.Threading.Tasks;
using LinqToDB.Data;

namespace MyLab.Search.Indexer.Services
{
    public interface IDataSourceService
    {
        IAsyncEnumerable<DataSourceBatch> Read(string query, DataParameter seedParameter);
    }
}
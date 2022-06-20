using System.Collections.Generic;
using System.Threading.Tasks;
using LinqToDB.Data;
using MyLab.Search.Indexer.DataContract;

namespace MyLab.Search.Indexer.Services
{
    public interface IDataSourceService
    {
        IAsyncEnumerable<DataSourceBatch> Read(string nsId, string query, DataParameter seedParameter);

        Task<DataSourceBatch> ReadByIdAsync(string query, DataParameter idParameter);
    }
}
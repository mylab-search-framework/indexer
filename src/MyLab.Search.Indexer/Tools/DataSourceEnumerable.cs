using System.Collections.Generic;
using System.Threading;
using LinqToDB.Data;
using MyLab.Search.Indexer.Models;

namespace MyLab.Search.Indexer.Tools
{
    class DataSourceEnumerable : IAsyncEnumerable<DataSourceLoadBatch>
    {
        private readonly string _sql;
        private readonly DataParameter _seedParameter;
        private readonly DataConnection _connection;
        private readonly int _pageSize;

        public DataSourceEnumerable(
            string sql,
            DataParameter seedParameter,
            DataConnection connection,
            int pageSize)
        {
            _sql = sql;
            _seedParameter = seedParameter;
            _connection = connection;
            _pageSize = pageSize;
        }

        public IAsyncEnumerator<DataSourceLoadBatch> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new DataSourceEnumerator(_sql, _seedParameter, _connection, _pageSize, cancellationToken);
        }
    }
}

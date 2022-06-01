using System.Collections.Generic;
using System.Threading;
using LinqToDB.Data;
using MyLab.Db;
using MyLab.Search.Indexer.Models;

namespace MyLab.Search.Indexer.Tools
{
    class DataSourceLoadBatchEnumerable : IAsyncEnumerable<DataSourceLoadBatch>
    {
        private readonly IDbManager _dbManager;
        private readonly string _sql;
        private readonly DataParameter _seedParameter;
        private readonly int _pageSize;

        public DataSourceLoadBatchEnumerable(
            IDbManager dbManager,
            string sql,
            DataParameter seedParameter,
            int pageSize)
        {
            _dbManager = dbManager;
            _sql = sql;
            _seedParameter = seedParameter;
            _pageSize = pageSize;
        }

        public IAsyncEnumerator<DataSourceLoadBatch> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new DataSourceLoadBatchEnumerator(_dbManager, _sql, _seedParameter, _pageSize, cancellationToken);
        }
    }
}

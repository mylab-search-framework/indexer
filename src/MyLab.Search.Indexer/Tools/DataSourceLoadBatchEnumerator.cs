using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using MyLab.Db;
using MyLab.Search.Indexer.Models;

namespace MyLab.Search.Indexer.Tools
{
    class DataSourceLoadBatchEnumerator : IAsyncEnumerator<DataSourceLoadBatch>
    {
        private readonly IDbManager _dbManager;
        private readonly string _sql;
        private readonly DataParameter _seedParameter;
        private readonly int _pageSize;
        private int _pageIndex;
        private readonly CancellationToken _cancellationToken;

        public DataSourceLoadBatch Current { get; set; }
        
        public DataSourceLoadBatchEnumerator(
            IDbManager dbManager,
            string sql,
            DataParameter seedParameter,
            int pageSize,
            CancellationToken cancellationToken)
        {
            _dbManager = dbManager;
            _sql = sql;
            _seedParameter = seedParameter;
            _pageSize = pageSize;
            _cancellationToken = cancellationToken;
        }

        public ValueTask DisposeAsync()
        {
            _pageIndex = 0;

            return ValueTask.CompletedTask;
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            var queryParams = new []
            {
                new DataParameter(QueryParameterNames.Offset, _pageIndex * _pageSize, DataType.Int32),
                new DataParameter(QueryParameterNames.Limit, _pageSize, DataType.Int32),
                _seedParameter
            };

            await using var conn = _dbManager.Use();

            var entities = await conn.QueryToArrayAsync(IndexingDocDataReader.Read, _sql, _cancellationToken, queryParams);

            Current = new DataSourceLoadBatch
            {
                Entities = entities,
                Query = conn.LastQuery
            };
            
            _pageIndex += 1;

            return Current.Entities.Length != 0;
        }
    }
}
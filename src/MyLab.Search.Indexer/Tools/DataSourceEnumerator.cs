using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using MyLab.Search.Indexer.Models;

namespace MyLab.Search.Indexer.Tools
{
    class DataSourceEnumerator : IAsyncEnumerator<DataSourceLoadBatch>
    {
        private readonly string _sql;
        private readonly DataParameter _seedParameter;
        private readonly DataConnection _connection;
        private readonly int _pageSize;
        private int _pageIndex;
        private readonly CancellationToken _cancellationToken;

        public DataSourceLoadBatch Current { get; set; }
        
        public DataSourceEnumerator(
            string sql,
            DataParameter seedParameter,
            DataConnection connection,
            int pageSize,
            CancellationToken cancellationToken)
        {
            _sql = sql;
            _seedParameter = seedParameter;
            _connection = connection;
            _pageSize = pageSize;
            _cancellationToken = cancellationToken;
        }

        public async ValueTask DisposeAsync()
        {
            await _connection.DisposeAsync(_cancellationToken);
            _pageIndex = 0;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            var queryParams = new []
            {
                new DataParameter(QueryParameterNames.Offset, _pageIndex * _pageSize, DataType.Int32),
                new DataParameter(QueryParameterNames.Limit, _pageSize, DataType.Int32),
                _seedParameter
            };

            var entities = _connection.Query(IndexingEntityDataReader.Read, _sql, queryParams).ToArray();

            Current = new DataSourceLoadBatch
            {
                Entities = entities,
                Query = _connection.LastQuery
            };

            var res = Current.Entities.Length != 0;

            _pageIndex += 1;

            return new ValueTask<bool>(res);
        }
    }
}
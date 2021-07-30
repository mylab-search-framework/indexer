using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.Tools
{
    class DataSourceEnumerator : IAsyncEnumerator<DataSourceBatch>
    {
        private readonly string _sql;
        private readonly DataParameter _seedParameter;
        private readonly DataConnection _connection;
        private readonly int _pageSize;
        private int _pageIndex;
        private readonly CancellationToken _cancellationToken;

        public DataSourceBatch Current { get; set; }

        public bool EnablePaging { get; set; }

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
            var queryParams = new List<DataParameter>
            {
                new DataParameter(QueryParameterNames.Offset, _pageIndex * _pageSize, DataType.Int32),
                new DataParameter(QueryParameterNames.Limit, _pageSize, DataType.Int32)
            };

            if(_seedParameter != null)
                queryParams.Add(_seedParameter);

            var entities = _connection.Query(ReadEntity, _sql, queryParams.ToArray()).ToArray();

            Current = new DataSourceBatch
            {
                Entities = entities,
                Query = _connection.LastQuery
            };

            var res = EnablePaging
                ? Current.Entities.Length != 0
                : _pageIndex == 0;

            _pageIndex += 1;

            return new ValueTask<bool>(res);
        }

        private static DataSourceEntity ReadEntity(IDataReader reader)
        {
            var resEnt = new DataSourceEntity
            {
                Properties = new Dictionary<string, string>()
            };

            for (var index = 0; index < reader.FieldCount; index++)
            {
                resEnt.Properties.Add(reader.GetName(index), reader.GetString(index));
            }

            return resEnt;
        }
    }
}
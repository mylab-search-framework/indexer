using System;
using System.Collections.Generic;
using System.Threading;
using LinqToDB.Data;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.Tools
{
    class DataSourceEnumerable : IAsyncEnumerable<DataSourceBatch>
    {
        private readonly string _sql;
        private readonly DataParameter _seedParameter;
        private readonly DataConnection _connection;
        private readonly int _pageSize;
        public bool EnablePaging { get; set; }

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

        public IAsyncEnumerator<DataSourceBatch> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            return new DataSourceEnumerator(_sql, _seedParameter, _connection, _pageSize, cancellationToken)
            {
                EnablePaging = EnablePaging
            };
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Options;
using MyLab.Db;

namespace MyLab.Search.Indexer
{
    interface IDataSourceService
    {
        IAsyncEnumerable<DataSourceEntity[]> Read(string query);
    }

    class DbDataSourceService : IDataSourceService
    {
        private readonly IDbManager _dbManager;
        private readonly IndexerOptions _options;

        public DbDataSourceService(IDbManager dbManager, IOptions<IndexerOptions> options)
            : this(dbManager, options.Value)
        {
        }

        public DbDataSourceService(IDbManager dbManager, IndexerOptions options)
        {
            _dbManager = dbManager;
            _options = options;
        }

        public IAsyncEnumerable<DataSourceEntity[]> Read(string query)
        {
            return new DataSourceEnumerable(query, _dbManager.Use(), _options.PageSize);
        }
    }

    class DataSourceEnumerable : IAsyncEnumerable<DataSourceEntity[]>
    {
        private readonly string _sql;
        private readonly DataConnection _connection;
        private readonly int _pageSize;

        public DataSourceEnumerable(string sql, DataConnection connection, int pageSize)
        {
            _sql = sql;
            _connection = connection;
            _pageSize = pageSize;
        }

        public IAsyncEnumerator<DataSourceEntity[]> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            return new DataSourceEnumerator(_sql, _connection, _pageSize, cancellationToken);
        }
    }

    class DataSourceEnumerator : IAsyncEnumerator<DataSourceEntity[]>
    {
        private const string OffsetKey = "{offset}";
        private const string LimitKey = "{limit}";

        private readonly string _sql;
        private readonly DataConnection _connection;
        private readonly int _pageSize;
        private int _pageIndex;
        private readonly CancellationToken _cancellationToken;
        private readonly bool _hasPaging;

        public DataSourceEntity[] Current { get; set; }

        public DataSourceEnumerator(string sql, DataConnection connection, int pageSize, CancellationToken cancellationToken)
        {
            _sql = sql;
            _hasPaging = sql.Contains(OffsetKey);
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
            var pagedSql = _sql
                .Replace(OffsetKey, (_pageIndex * _pageSize).ToString())
                .Replace(LimitKey, _pageSize.ToString());

            Current = _connection.Query(reader =>
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

            }, pagedSql).ToArray();

            var res = _hasPaging
                ? Current.Length != 0
                : _pageIndex == 0;

            _pageIndex += 1;

            return new ValueTask<bool>(res);
        }
    }
}
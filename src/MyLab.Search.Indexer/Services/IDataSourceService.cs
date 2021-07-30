using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Options;
using MyLab.Db;

namespace MyLab.Search.Indexer.Services
{
    public interface IDataSourceService
    {
        Task<IAsyncEnumerable<DataSourceBatch>> Read(string query);
    }

    public class DataSourceBatch
    {
        public string Query { get; set; }
        public DataSourceEntity[] Entities { get; set; }
    }

    class DbDataSourceService : IDataSourceService
    {
        private readonly IDbManager _dbManager;
        private readonly ISeedService _seedService;
        private readonly IndexerOptions _options;

        public DbDataSourceService(
            IDbManager dbManager, 
            ISeedService seedService,
            IOptions<IndexerOptions> options)
            : this(dbManager, seedService, options.Value)
        {
        }

        public DbDataSourceService(
            IDbManager dbManager, 
            ISeedService seedService,
            IndexerOptions options)
        {
            _dbManager = dbManager;
            _seedService = seedService;
            _options = options;
        }

        public async Task<IAsyncEnumerable<DataSourceBatch>> Read(string query)
        {
            return new DataSourceEnumerable(query, _dbManager.Use(), _options.PageSize)
            {
                Seed = await _seedService.ReadDateTimeAsync(),
                EnablePaging = _options.EnablePaging
            };
        }
    }

    class DataSourceEnumerable : IAsyncEnumerable<DataSourceBatch>
    {
        private readonly string _sql;
        private readonly DataConnection _connection;
        private readonly int _pageSize;

        public DateTime Seed { get; set; }
        public bool EnablePaging { get; set; }

        public DataSourceEnumerable(string sql, DataConnection connection, int pageSize)
        {
            _sql = sql;
            _connection = connection;
            _pageSize = pageSize;
        }

        public IAsyncEnumerator<DataSourceBatch> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            return new DataSourceEnumerator(_sql, _connection, _pageSize, cancellationToken)
            {
                Seed = Seed,
                EnablePaging = EnablePaging
            };
        }
    }

    class DataSourceEnumerator : IAsyncEnumerator<DataSourceBatch>
    {
        private const string OffsetParamName = "offset";
        private const string LimitParamName = "limit";
        private const string SeedParamName = "seed";

        private readonly string _sql;
        private readonly DataConnection _connection;
        private readonly int _pageSize;
        private int _pageIndex;
        private readonly CancellationToken _cancellationToken;
        
        public DataSourceBatch Current { get; set; }

        public DateTime Seed { get; set; }

        public bool EnablePaging { get; set; }

        public DataSourceEnumerator(string sql, DataConnection connection, int pageSize, CancellationToken cancellationToken)
        {
            _sql = sql;
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
                new DataParameter(OffsetParamName, _pageIndex * _pageSize, DataType.Int32),
                new DataParameter(LimitParamName, _pageSize, DataType.Int32),
                new DataParameter(SeedParamName, Seed, DataType.DateTime) 
            };

            var entities = _connection.Query(ReadEntity, _sql, queryParams).ToArray();

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
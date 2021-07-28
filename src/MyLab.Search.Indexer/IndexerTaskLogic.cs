using System;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Options;
using MyLab.Db;
using MyLab.TaskApp;

namespace MyLab.Search.Indexer
{
    public class IndexerTaskLogic : ITaskLogic
    {
        private readonly IndexerOptions _options;
        private readonly IDbManager _dbManager;
        private readonly ISeedService _seedService;

        public IndexerTaskLogic(IOptions<IndexerOptions> options, IDbManager dbManager, ISeedService seedService)
            :this(options.Value, dbManager, seedService)
        {
            
        }

        public IndexerTaskLogic(IndexerOptions options, IDbManager dbManager, ISeedService seedService)
        {
            _options = options;
            _dbManager = dbManager;
            _seedService = seedService;
        }

        public async Task Perform(CancellationToken cancellationToken)
        {
            await using var connection = _dbManager.Use();

            var sql = await ProvideSql();
            int pageIndex = 0;
            DataSourceEntity[] found;

            do
            {

                found = await connection.FromSql<DataSourceEntity>(sql)
                    .Skip(() => pageIndex * _options.PageSize)
                    .Take(() => _options.PageSize)
                    .ToArrayAsync(cancellationToken);
                

            } while (found.Length < _options.PageSize);
        }

        private async Task<string> ProvideSql()
        {
            const string seedKey = "{seed}";

            if (!_options.Query.Contains(seedKey)) return _options.Query;

            var seed = await _seedService.ReadAsync();

            return _options.Query.Replace(seedKey, seed.ToString());
        }
    }
}
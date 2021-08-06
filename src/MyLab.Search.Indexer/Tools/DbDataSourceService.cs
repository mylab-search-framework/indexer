using System.Collections.Generic;
using System.Threading.Tasks;
using LinqToDB.Data;
using Microsoft.Extensions.Options;
using MyLab.Db;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.Tools
{
    class DbDataSourceService : IDataSourceService
    {
        private readonly IDbManager _dbManager;
        private readonly IndexerDbOptions _options;

        public DbDataSourceService(
            IDbManager dbManager, 
            IOptions<IndexerDbOptions> options)
            : this(dbManager, options.Value)
        {
        }

        public DbDataSourceService(
            IDbManager dbManager,
            IndexerDbOptions options)
        {
            _dbManager = dbManager;
            _options = options;
        }

        public IAsyncEnumerable<DataSourceBatch> Read(string query, DataParameter seedParameter = null)
        {
            return new DataSourceEnumerable(query, seedParameter, _dbManager.Use(), _options.PageSize)
            {
                EnablePaging = _options.EnablePaging
            };
        }
    }
}
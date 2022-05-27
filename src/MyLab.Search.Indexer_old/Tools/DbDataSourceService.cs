using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Db;
using MyLab.Log;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.Tools
{
    class DbDataSourceService : IDataSourceService
    {
        private readonly IDbManager _dbManager;
        private readonly IndexerOptions _options;
        private readonly IDslLogger _log;

        public DbDataSourceService(
            IDbManager dbManager, 
            IOptions<IndexerOptions> options,
            ILogger<DbDataSourceService> logger = null)
            : this(dbManager, options.Value)
        {
            _log = logger?.Dsl();
        }

        public DbDataSourceService(
            IDbManager dbManager,
            IndexerOptions options,
            ILogger<DbDataSourceService> logger = null)
        {
            _dbManager = dbManager;
            _options = options;
            _log = logger?.Dsl();
        }

        public IAsyncEnumerable<DataSourceBatch> Read(string nsId, string query, DataParameter seedParameter = null)
        {
            IdxOptions foundIdx;

            try
            {
                foundIdx = _options.GetIndexOptions(nsId);
            }
            catch (NamespaceConfigException e)
            {
                foundIdx = e.IndexOptionsFromNamespaceOptions;

                _log?.Warning(e).Write();
            }

            return new DataSourceEnumerable(query, seedParameter, _dbManager.Use(), foundIdx.PageSize)
            {
                EnablePaging = foundIdx.EnablePaging
            };
        }

        public Task<DataSourceBatch> ReadByIdAsync(string query, DataParameter idParameter)
        {
            using var c = _dbManager.Use();

            var entities = c.Query(DataSourceEntity.ReadEntity, query, idParameter).ToArray();

            var res = new DataSourceBatch
            {
                Entities = entities,
                Query = c.LastQuery
            };

            return Task.FromResult(res);
        }
    }
}
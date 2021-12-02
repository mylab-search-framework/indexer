using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Data;
using Microsoft.Extensions.Options;
using MyLab.Db;
using MyLab.Log;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.Tools
{
    class DbDataSourceService : IDataSourceService
    {
        private readonly IDbManager _dbManager;
        private readonly IndexerOptions _options;

        public DbDataSourceService(
            IDbManager dbManager, 
            IOptions<IndexerOptions> options)
            : this(dbManager, options.Value)
        {
        }

        public DbDataSourceService(
            IDbManager dbManager,
            IndexerOptions options)
        {
            _dbManager = dbManager;
            _options = options;
        }

        public IAsyncEnumerable<DataSourceBatch> Read(string jobId, string query, DataParameter seedParameter = null)
        {
            var foundJob = _options.GetJobOptions(jobId);

            return new DataSourceEnumerable(query, seedParameter, _dbManager.Use(), foundJob.PageSize)
            {
                EnablePaging = foundJob.EnablePaging
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
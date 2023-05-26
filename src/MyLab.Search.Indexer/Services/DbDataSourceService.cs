using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Db;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.Services
{
    class DbDataSourceService : IDataSourceService
    {
        private readonly IDbManager _dbManager;
        private readonly ISeedService _seedService;
        private readonly IResourceProvider _resourceProvider;
        private readonly IndexerOptions _options;
        private readonly IDslLogger _log;

        public DbDataSourceService(
            IDbManager dbManager,
            ISeedService seedService,
            IResourceProvider resourceProvider,
            IOptions<IndexerOptions> options,
            ILogger<DbDataSourceService> logger = null)
        : this(dbManager, seedService, resourceProvider, options.Value, logger)
        {
        }

        public DbDataSourceService(
            IDbManager dbManager,
            ISeedService seedService,
            IResourceProvider resourceProvider,
            IndexerOptions options,
            ILogger<DbDataSourceService> logger = null
        )
        {
            _dbManager = dbManager;
            _seedService = seedService;
            _resourceProvider = resourceProvider;
            _options = options;
            _log = logger?.Dsl();
        }

        public async Task<DataSourceLoad> LoadKickAsync(string indexId, string[] idList)
        {
            string kickQueryPattern = _resourceProvider.ProvideKickQuery(indexId)?.Content;
            
            await using var conn = _dbManager.Use();
            
            var kickQuery = KickQuery.Build(kickQueryPattern, idList);
            
            var docs = await conn.QueryToArrayAsync(
                IndexingDocDataReader.Read, 
                kickQuery.Query, 
                kickQuery.Parameters);

            return new DataSourceLoad
            {
                Batch = new DataSourceLoadBatch
                {
                    Docs = docs,
                    Query = conn.LastQuery
                }
            };
        }

        public async Task<IAsyncEnumerable<DataSourceLoad>> LoadSyncAsync(string indexId)
        {
            string syncQuery = _resourceProvider.ProvideSyncQuery(indexId)?.Content;

            var totalIndexType = _options.GetTotalIndexType(indexId);
            var seed = await _seedService.LoadSeedAsync(indexId);

            if (totalIndexType == IndexType.Heap && !seed.IsDateTime)
                throw new InvalidOperationException("A DateTime seed is not suitable for Heap index");

            if (totalIndexType == IndexType.Stream && !seed.IsLong)
                throw new InvalidOperationException("A Long seed is not suitable for Stream index");

            var seedParameter = new DataParameter(
                QueryParameterNames.Seed, 
                seed.IsLong ? seed.Long : seed.DataTime, 
                seed.IsLong ? DataType.Int64 : DataType.DateTime
            );

            var batchEnumerable = new DataSourceLoadBatchEnumerable(_dbManager, syncQuery, seedParameter, _options.SyncPageSize);

            _log?.Action("Sync data loading")
                .AndFactIs("seed", seed.ToString())
                .Write();

            return new DataSourceLoadEnumerable(indexId, totalIndexType, _seedService, batchEnumerable);
        }
    }
}
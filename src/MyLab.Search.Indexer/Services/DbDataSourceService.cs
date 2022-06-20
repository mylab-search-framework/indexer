using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Db;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Tools;
using SQLitePCL;

namespace MyLab.Search.Indexer.Services
{
    class DbDataSourceService : IDataSourceService
    {
        private readonly IDbManager _dbManager;
        private readonly ISeedService _seedService;
        private readonly IIndexResourceProvider _indexResourceProvider;
        private readonly IndexerOptions _options;
        private readonly IDslLogger _log;

        public DbDataSourceService(
            IDbManager dbManager,
            ISeedService seedService,
            IIndexResourceProvider indexResourceProvider,
            IOptions<IndexerOptions> options,
            ILogger<DbDataSourceService> logger = null)
        : this(dbManager, seedService, indexResourceProvider, options.Value, logger)
        {
        }

        public DbDataSourceService(
            IDbManager dbManager,
            ISeedService seedService,
            IIndexResourceProvider indexResourceProvider,
            IndexerOptions options,
            ILogger<DbDataSourceService> logger = null
        )
        {
            _dbManager = dbManager;
            _seedService = seedService;
            _indexResourceProvider = indexResourceProvider;
            _options = options;
            _log = logger?.Dsl();
        }

        public async Task<DataSourceLoad> LoadKickAsync(string indexId, string[] idList)
        {
            var idxOpts = _options.GetIndexOptions(indexId);
            idxOpts.ValidateIdPropertyType();

            var kickQueryPattern = await _indexResourceProvider.ProvideKickQueryAsync(indexId);

            await using var conn = _dbManager.Use();
            
            var kickQuery = KickQuery.Build(kickQueryPattern, idList, idxOpts.IdPropertyType);
            
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
            var idxOpts = _options.GetIndexOptions(indexId);

            var syncQuery = await _indexResourceProvider.ProvideSyncQueryAsync(indexId);
            
            DataParameter seedParameter;
            string seedStrValue;
            
            switch (idxOpts.IndexType)
            {
                case IndexType.Heap:
                    {
                        var dtSeed = await _seedService.LoadDtSeedAsync(indexId);
                        seedStrValue = dtSeed.ToString("O");
                        seedParameter = new DataParameter(QueryParameterNames.Seed, dtSeed, DataType.DateTime);
                    }
                    break;
                case IndexType.Stream:
                    {
                        var idSeed = await _seedService.LoadIdSeedAsync(indexId);
                        seedStrValue = idSeed.ToString();
                        seedParameter = new DataParameter(QueryParameterNames.Seed, idSeed, DataType.Int64);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            var batchEnumerable = new DataSourceLoadBatchEnumerable(_dbManager, syncQuery, seedParameter, idxOpts.SyncPageSize);

            _log?.Action("Sync data loading")
                .AndFactIs("seed", seedStrValue)
                .Write();

            return new DataSourceLoadEnumerable(indexId, idxOpts.IndexType, _seedService, batchEnumerable);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Options;
using MyLab.Db;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.Services
{
    class DataSourceService : IDataSourceService
    {
        private readonly IDbManager _dbManager;
        private readonly ISeedService _seedService;
        private readonly IIndexResourceProvider _indexResourceProvider;
        private readonly IndexerOptions _options;

        public DataSourceService(
            IDbManager dbManager,
            ISeedService seedService,
            IIndexResourceProvider indexResourceProvider,
            IOptions<IndexerOptions> options)
        : this(dbManager, seedService, indexResourceProvider, options.Value)
        {
        }

        public DataSourceService(
            IDbManager dbManager,
            ISeedService seedService,
            IIndexResourceProvider indexResourceProvider,
            IndexerOptions options
        )
        {
            _dbManager = dbManager;
            _seedService = seedService;
            _indexResourceProvider = indexResourceProvider;
            _options = options;
        }

        public async Task<DataSourceLoad> LoadKickAsync(string indexId, string[] idList)
        {
            var idxOpts = _options.GetIndexOptions(indexId);

            idxOpts.ValidateIdPropertyType();

            throw new NotImplementedException();
        }

        public async Task<IAsyncEnumerable<DataSourceLoad>> LoadSyncAsync(string indexId)
        {
            var idxOpts = _options.GetIndexOptions(indexId);

            var syncQuery = await _indexResourceProvider.ProvideSyncQueryAsync(indexId);
            
            DataParameter seedParameter;

            if (idxOpts.IsStream)
            {
                var idSeed = await _seedService.LoadIdSeedAsync(indexId);
                seedParameter = new DataParameter(QueryParameterNames.Seed, idSeed, DataType.Int64);
            }
            else
            {
                var dtSeed = await _seedService.LoadDtSeedAsync(indexId);
                seedParameter = new DataParameter(QueryParameterNames.Seed, dtSeed, DataType.DateTime);
            }
            
            var batchEnumerable = new DataSourceLoadBatchEnumerable(_dbManager, syncQuery, seedParameter, idxOpts.SyncPageSize);

            return new DataSourceLoadEnumerable(indexId, idxOpts.IsStream, _seedService, batchEnumerable);
        }
    }
}
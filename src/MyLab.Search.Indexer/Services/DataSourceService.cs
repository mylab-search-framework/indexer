using System;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;
using Microsoft.Extensions.Options;
using MyLab.Db;
using MyLab.Log;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Tools;
using Newtonsoft.Json.Linq;

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
            IOptions<IndexerOptions> options
            )
        {
            _dbManager = dbManager;
            _seedService = seedService;
            _indexResourceProvider = indexResourceProvider;
            _options = options.Value;
        }

        public async Task<DataSourceLoad> LoadKickAsync(string indexId, string[] idList)
        {
            var idxOpts = _options.GetIndexOptions(indexId);

            idxOpts.ValidateIdPropertyType();

            throw new NotImplementedException();
        }

        public async Task<DataSourceLoad> LoadSyncAsync(string indexId)
        {
            var idxOpts = _options.GetIndexOptions(indexId);

            var syncQuery = await _indexResourceProvider.ProvideSyncQueryAsync(indexId);
            
            await using var conn =  _dbManager.Use();

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
            
            var enumerable = new DataSourceEnumerable(syncQuery, seedParameter, conn, idxOpts.SyncPageSize);

            var loadBatches = await enumerable.ToArrayAsync();

            ISeedSaver seedSaver = null;

            if (loadBatches is { Length: > 0 })
            {
                if (idxOpts.IsStream)
                {
                    var allLoadIds = loadBatches
                        .SelectMany(b => b.Entities)
                        .Select(e => new
                        {
                            OriginId = e.Id, 
                            ParsedId = ulong.TryParse(e.Id, out ulong parsedId) 
                                ? (ulong?)parsedId 
                                : null
                        })
                        .ToArray();

                    var badIds = allLoadIds
                        .Where(id => !id.ParsedId.HasValue)
                        .ToArray();

                    if (badIds.Length > 0)
                        throw new InvalidOperationException("Can't parse entity identifiers as 'ulong'")
                            .AndFactIs("bad-id-list", badIds.Select(id => id.OriginId).ToArray());

                    var maxId = allLoadIds.Max(id => id.ParsedId.GetValueOrDefault());

                    seedSaver = new IdSeedSaver(indexId, maxId, _seedService);
                }
                else
                {
                    seedSaver = new DtSeedSaver(indexId, DateTime.Now, _seedService);
                }
            }

            return new DataSourceLoad
            {
                Batches = loadBatches,
                SeedSaver = seedSaver
            };
        }
    }
}
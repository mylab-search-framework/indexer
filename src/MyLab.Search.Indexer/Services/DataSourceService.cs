using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyLab.Db;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;

namespace MyLab.Search.Indexer.Services
{
    class DataSourceService : IDataSourceService
    {
        private readonly IDbManager _dbManager;
        private readonly IndexerOptions _options;

        public DataSourceService(
            IDbManager dbManager, 
            IOptions<IndexerOptions> options
            )
        :this(dbManager, options.Value)
        {
            
        }

        public DataSourceService(
            IDbManager dbManager,
            IndexerOptions options
        )
        {
            _dbManager = dbManager;
            _options = options;
        }

        public Task<IndexingRequestEntity[]> LoadByIdListAsync(string indexId, string[] idList)
        {
            throw new NotImplementedException();
        }

        public Task<DataSourceDeltaLoad> LoadDeltaAsync(string indexId)
        {
            throw new NotImplementedException();
        }
    }
}
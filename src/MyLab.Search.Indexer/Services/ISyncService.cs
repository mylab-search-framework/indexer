using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;

namespace MyLab.Search.Indexer.Services
{
    public interface ISyncService
    {
        Task<bool> IsSyncEnabledAsync(string indexName);

        Task SyncAsync(string idxName, IndexType idxType, CancellationToken cancellationToken = default);
    }

    class SyncService : ISyncService
    {
        private readonly IDataSourceService _dataSource;
        private readonly IIndexerService _indexer;
        private readonly IResourceProvider _resourceProvider;
        private readonly IndexerOptions _options;
        private readonly IDslLogger _log;

        public SyncService(
            IOptions<IndexerOptions> options,
            IDataSourceService dataSource,
            IIndexerService indexer,
            IResourceProvider resourceProvider,
            ILogger<SyncService> logger = null)
        {
            _dataSource = dataSource;
            _indexer = indexer;
            _resourceProvider = resourceProvider;
            _options = options.Value;
            _log = logger?.Dsl();
        }

        public async Task<bool> IsSyncEnabledAsync(string indexName)
        {
            if (_options.Indexes.Any(idx => idx.Id == indexName && idx.KickDbQuery != null))
                return true;

            try
            {
                var kickQuery = await _resourceProvider.ProvideKickQueryAsync(indexName);

                return kickQuery != null;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        public async Task SyncAsync(string idxName, IndexType idxType, CancellationToken cancellationToken = default)
        {
            _log.Action("Index sync started")
                .AndFactIs("idx", idxName)
                .Write();

            int syncCount = 0;

            var dataEnum = await _dataSource.LoadSyncAsync(idxName);
            if (dataEnum != null)
            {
                await foreach (var data in dataEnum.WithCancellation(cancellationToken))
                {
                    var idxReq = new IndexingRequest
                    {
                        IndexId = idxName
                    };

                    switch (idxType)
                    {
                        case IndexType.Heap:
                            {
                                idxReq.PutList = data.Batch.Docs;
                            }
                            break;
                        case IndexType.Stream:
                            {
                                idxReq.PostList = data.Batch.Docs;
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    await _indexer.IndexAsync(idxReq, cancellationToken);

                    await data.SeedSaver.SaveAsync();

                    syncCount += data.Batch.Docs.Length;

                    _log.Debug("Sync data has been indexed")
                        .AndFactIs("idx", idxName)
                        .AndFactIs("sql", data.Batch.Query)
                        .AndFactIs("count", data.Batch.Docs.Length)
                        .Write();
                }
            }

            if (syncCount != 0)
            {
                _log.Action("Index sync completed")
                    .AndFactIs("idx", idxName)
                    .AndFactIs("count", syncCount)
                    .Write();
            }
            else
            {
                _log.Action("No sync data found")
                    .AndFactIs("idx", idxName)
                    .Write();
            }
        }
    }
}

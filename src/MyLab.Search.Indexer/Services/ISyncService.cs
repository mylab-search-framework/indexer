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
        bool IsSyncEnabled(string indexId);

        Task SyncAsync(string idxName, CancellationToken cancellationToken = default);
    }

    class SyncService : ISyncService
    {
        private readonly IDataSourceService _dataSource;
        private readonly IIndexerService _indexer;
        private readonly IResourceProvider _resourceProvider;
        private readonly IDslLogger _log;

        public SyncService(
            IDataSourceService dataSource,
            IIndexerService indexer,
            IResourceProvider resourceProvider,
            ILogger<SyncService> logger = null)
        {
            _dataSource = dataSource;
            _indexer = indexer;
            _resourceProvider = resourceProvider;
            _log = logger?.Dsl();
        }

        public bool IsSyncEnabled(string indexId)
        {
            return _resourceProvider.IndexDirectory.Named.TryGetValue(indexId, out var idxRes) &&
                   idxRes.KickQuery != null;
        }

        public async Task SyncAsync(string idxName, CancellationToken cancellationToken = default)
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

                    idxReq.PutList = data.Batch.Docs;

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

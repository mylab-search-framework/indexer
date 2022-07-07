using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
using MyLab.TaskApp;

namespace MyLab.Search.Indexer.Services
{
    class SyncTaskLogic : ITaskLogic
    {
        private readonly IndexerOptions _opts;
        private readonly IDataSourceService _dataSource;
        private readonly IIndexerService _indexer;
        private readonly IDslLogger _log;

        public SyncTaskLogic(IOptions<IndexerOptions> opts, IDataSourceService dataSource, IIndexerService indexer, ILogger<SyncTaskLogic> logger = null)
            :this(opts.Value, dataSource, indexer, logger)
        {
            
        }

        public SyncTaskLogic(IndexerOptions opts, IDataSourceService dataSource, IIndexerService indexer, ILogger<SyncTaskLogic> logger = null)
        {
            _opts = opts;
            _dataSource = dataSource;
            _indexer = indexer;
            _log = logger?.Dsl();
        }

        public async Task Perform(CancellationToken cancellationToken)
        {
            _log.Action("Indexes sync started")
                .Write();

            if (_opts.Indexes is not { Length: > 0 })
            {
                _log.Warning("Configured indexes not found")
                    .Write();
                return;
            }

            foreach (var idx in _opts.Indexes)
            {
                if (!idx.EnableSync)
                {
                    _log.Action("Index sync is disabled")
                        .AndFactIs("idx", idx.Id)
                        .Write();
                }

                try
                {
                    var totalIndexType = _opts.GetTotalIndexType(idx.Id);

                    await SyncIndexAsync(cancellationToken, idx.Id, totalIndexType);
                }
                catch (Exception e)
                {
                    _log.Error(e)
                        .AndFactIs("index", idx.Id)
                        .Write();
                }
            }

            _log.Action("Indexes sync completed")
                .Write();
        }

        private async Task SyncIndexAsync(CancellationToken cancellationToken, string idxId, IndexType idxType)
        {
            _log.Action("Index sync started")
                .AndFactIs("idx", idxId)
                .Write();

            int syncCount = 0;

            var dataEnum = await _dataSource.LoadSyncAsync(idxId);
            if (dataEnum != null)
            {
                await foreach (var data in dataEnum.WithCancellation(cancellationToken))
                {
                    var idxReq = new IndexingRequest
                    {
                        IndexId = idxId
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
                        .AndFactIs("idx", idxId)
                        .AndFactIs("sql", data.Batch.Query)
                        .AndFactIs("count", data.Batch.Docs.Length)
                        .Write();
                }
            }

            if (syncCount != 0)
            {
                _log.Action("Index sync completed")
                    .AndFactIs("idx", idxId)
                    .AndFactIs("count", syncCount)
                    .Write();
            }
            else
            {
                _log.Action("No sync data found")
                    .AndFactIs("idx", idxId)                  
                    .Write();
            }
        }
    }
}

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
            if (_opts.Indexes is not { Length: > 0 })
            {
                _log.Warning("Configured indexes not found")
                    .Write();
                return;
            }

            foreach (var idx in _opts.Indexes)
            {
                try
                {
                    await SyncIndexAsync(cancellationToken, idx);
                }
                catch (Exception e)
                {
                    _log.Error(e)
                        .AndFactIs("index", idx.Id)
                        .Write();
                }
            }
        }

        private async Task SyncIndexAsync(CancellationToken cancellationToken, IndexOptions idx)
        {
            int syncCount = 0;

            var dataEnum = await _dataSource.LoadSyncAsync(idx.Id);
            if (dataEnum != null)
            {
                await foreach (var data in dataEnum.WithCancellation(cancellationToken))
                {
                    var idxReq = new IndexingRequest
                    {
                        IndexId = idx.Id
                    };

                    switch (idx.IndexType)
                    {
                        case IndexType.Heap:
                        {
                            idxReq.PutList = data.Batch.Entities;
                        }
                            break;
                        case IndexType.Stream:
                        {
                            idxReq.PostList = data.Batch.Entities;
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    await _indexer.IndexAsync(idxReq, cancellationToken);

                    await data.SeedSaver.SaveAsync();

                    syncCount += data.Batch.Entities.Length;

                    _log.Debug("Sync data has been indexed")
                        .AndFactIs("idx", idx.Id)
                        .AndFactIs("sql", data.Batch.Query)
                        .AndFactIs("count", data.Batch.Entities.Length)
                        .Write();
                }
            }

            if (syncCount != 0)
            {
                _log.Action("Index sync completed")
                    .AndFactIs("idx", idx.Id)
                    .AndFactIs("count", syncCount)
                    .Write();
            }
            else
            {
                _log.Action("No sync data found")
                    .AndFactIs("idx", idx.Id)                  
                    .Write();
            }
        }
    }
}

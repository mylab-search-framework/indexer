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
        private readonly ISyncService _syncService;
        private readonly IndexerOptions _opts;
        private readonly IDslLogger _log;
        
        public SyncTaskLogic(
            IOptions<IndexerOptions> opts,
            ISyncService syncService,
            ILogger<SyncTaskLogic> logger = null)
        {
            _syncService = syncService;
            _opts = opts.Value;
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
                var idxSyncEnabled = await _syncService.IsSyncEnabledAsync(idx.Id);

                if (!idxSyncEnabled)
                {
                    _log.Action("Index sync is disabled")
                        .AndFactIs("idx", idx.Id)
                        .Write();
                }

                try
                {
                    var totalIndexType = _opts.GetTotalIndexType(idx.Id);

                    await _syncService.SyncAsync(idx.Id, totalIndexType, cancellationToken);
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
    }
}

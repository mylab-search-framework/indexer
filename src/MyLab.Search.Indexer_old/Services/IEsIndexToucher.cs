using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyLab.Log.Dsl;
using MyLab.Search.EsAdapter;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.Services
{
    public interface IEsIndexToucher
    {
        Task TouchEsIndexAsync(IdxOptions idxOptions, DataSourceEntity example, CancellationToken cancellationToken);
    }

    class EsIndexToucher : IEsIndexToucher
    {
        private readonly IEsManager _esManager;
        private readonly IIndexResourceProvider _indexResourceProvider;
        private readonly List<string> _touchedIndexes = new();
        private readonly IDslLogger _log;

        public EsIndexToucher(
            IEsManager esManager, 
            IIndexResourceProvider indexResourceProvider,
            ILogger<EsIndexToucher> logger = null)
        {
            _esManager = esManager;
            _indexResourceProvider = indexResourceProvider;
            _log = logger?.Dsl();
        }

        public async Task TouchEsIndexAsync(IdxOptions idxOptions, DataSourceEntity example, CancellationToken cancellationToken)
        {
            if (_touchedIndexes.Contains(idxOptions.Id))
                return;

            bool esIndexExists = await _esManager.IsIndexExistsAsync(idxOptions.EsIndex, cancellationToken);

            if (!esIndexExists)
            {
                var factory = new CreateIndexStrategyFactory(idxOptions, _indexResourceProvider, example)
                {
                    Log = _log
                };

                var createIndexStrategy = await factory.CreateAsync(cancellationToken);

                _log?.Warning("ES index not found and will be created")
                    .AndFactIs("es-index", idxOptions.EsIndex)
                    .AndFactIs("index-id", idxOptions.Id)
                    .Write();

                await createIndexStrategy.CreateIndexAsync(_esManager, idxOptions.EsIndex, cancellationToken);

                _log?.Action("ES index created")
                    .AndFactIs("es-index", idxOptions.EsIndex)
                    .AndFactIs("index-id", idxOptions.Id)
                    .Write();
            }

            _touchedIndexes.Add(idxOptions.Id);
        }
    }
}

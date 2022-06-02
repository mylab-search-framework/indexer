using System.Collections.Generic;
using System.Threading;
using MyLab.Db;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.Tools
{
    class DataSourceLoadEnumerable : IAsyncEnumerable<DataSourceLoad>
    {
        private readonly string _indexId;
        private readonly DataSourceLoadBatchEnumerable _batchEnumerable;
        private readonly IndexType _indexType;
        private readonly ISeedService _seedService;

        public DataSourceLoadEnumerable(
            string indexId,
            IndexType indexType,
            ISeedService seedService,
            DataSourceLoadBatchEnumerable batchEnumerable)
        {
            _indexId = indexId;
            _batchEnumerable = batchEnumerable;
            _indexType = indexType;
            _seedService = seedService;
        }

        public IAsyncEnumerator<DataSourceLoad> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            var batchEnumerator = _batchEnumerable.GetAsyncEnumerator(cancellationToken);
            return new DataSourceLoadEnumerator(_indexId, _indexType, _seedService, batchEnumerator);
        }
    }
}
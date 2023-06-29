using System.Collections.Generic;
using System.Threading;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.Tools
{
    class IndexDataSourceLoadEnumerable : IAsyncEnumerable<DataSourceLoad>
    {
        private readonly string _indexId;
        private readonly DataSourceLoadBatchEnumerable _batchEnumerable;
        private readonly ISeedService _seedService;

        public IndexDataSourceLoadEnumerable(
            string indexId,
            ISeedService seedService,
            DataSourceLoadBatchEnumerable batchEnumerable)
        {
            _indexId = indexId;
            _batchEnumerable = batchEnumerable;
            _seedService = seedService;
        }

        public IAsyncEnumerator<DataSourceLoad> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            var batchEnumerator = _batchEnumerable.GetAsyncEnumerator(cancellationToken);
            return new IndexDataSourceLoadEnumerator(_indexId, _seedService, batchEnumerator);
        }
    }

    class StreamDataSourceLoadEnumerable : IAsyncEnumerable<DataSourceLoad>
    {
        private readonly string _indexId;
        private readonly DataSourceLoadBatchEnumerable _batchEnumerable;
        private readonly ISeedService _seedService;

        public StreamDataSourceLoadEnumerable(
            string indexId,
            ISeedService seedService,
            DataSourceLoadBatchEnumerable batchEnumerable)
        {
            _indexId = indexId;
            _batchEnumerable = batchEnumerable;
            _seedService = seedService;
        }

        public IAsyncEnumerator<DataSourceLoad> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            var batchEnumerator = _batchEnumerable.GetAsyncEnumerator(cancellationToken);
            return new StreamDataSourceLoadEnumerator(_indexId, _seedService, batchEnumerator);
        }
    }
}
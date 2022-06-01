using System.Collections.Generic;
using System.Threading;
using MyLab.Db;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.Tools
{
    class DataSourceLoadEnumerable : IAsyncEnumerable<DataSourceLoad>
    {
        private readonly string _indexId;
        private readonly DataSourceLoadBatchEnumerable _batchEnumerable;
        private readonly bool _indexIsStream;
        private readonly ISeedService _seedService;

        public DataSourceLoadEnumerable(
            string indexId,
            bool indexIsStream,
            ISeedService seedService,
            DataSourceLoadBatchEnumerable batchEnumerable)
        {
            _indexId = indexId;
            _batchEnumerable = batchEnumerable;
            _indexIsStream = indexIsStream;
            _seedService = seedService;
        }

        public IAsyncEnumerator<DataSourceLoad> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            var batchEnumerator = _batchEnumerable.GetAsyncEnumerator(cancellationToken);
            return new DataSourceLoadEnumerator(_indexId, _indexIsStream, _seedService, batchEnumerator);
        }
    }
}
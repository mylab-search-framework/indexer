using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyLab.Log;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Services;
using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Tools
{
    abstract class DataSourceLoadEnumerator : IAsyncEnumerator<DataSourceLoad>
    {
        private readonly IAsyncEnumerator<DataSourceLoadBatch> _batchEnumerator;
        public DataSourceLoad Current { get; set; }
        
        protected DataSourceLoadEnumerator(
            IAsyncEnumerator<DataSourceLoadBatch> batchEnumerator)
        {
            _batchEnumerator = batchEnumerator;
        }

        public ValueTask DisposeAsync()
        {
            return _batchEnumerator.DisposeAsync();
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            var hasResult = await _batchEnumerator.MoveNextAsync();
            if (!hasResult) return false;

            var batchDocs = _batchEnumerator.Current.Docs;

            ISeedSaver seedSaver = null;

            if (batchDocs is { Length: > 0 })
            {
                seedSaver = CreateSeedSaver(batchDocs);
            }

            Current = new DataSourceLoad
            {
                Batch = _batchEnumerator.Current,
                SeedSaver = seedSaver
            };

            return true;
        }

        protected abstract SeedSaver CreateSeedSaver(JObject[] batchDocs);
    }

    class IndexDataSourceLoadEnumerator : DataSourceLoadEnumerator
    {
        private readonly string _indexId;
        private readonly ISeedService _seedService;

        public IndexDataSourceLoadEnumerator(string indexId, ISeedService seedService, IAsyncEnumerator<DataSourceLoadBatch> batchEnumerator)
            : base(batchEnumerator)
        {
            _indexId = indexId;
            _seedService = seedService;
        }

        protected override SeedSaver CreateSeedSaver(JObject[] batchDocs)
        {
            return new SeedSaver(_indexId, DateTime.Now, _seedService);
        }
    }

    class StreamDataSourceLoadEnumerator : DataSourceLoadEnumerator
    {
        private readonly string _indexId;
        private readonly ISeedService _seedService;

        public StreamDataSourceLoadEnumerator(string indexId, ISeedService seedService, IAsyncEnumerator<DataSourceLoadBatch> batchEnumerator)
            : base(batchEnumerator)
        {
            _indexId = indexId;
            _seedService = seedService;
        }

        protected override SeedSaver CreateSeedSaver(JObject[] batchDocs)
        {
            var allLoadIds = batchDocs
                .Select(e => new
                {
                    OriginId = e.GetIdProperty(),
                    ParsedId = long.TryParse(e.GetIdProperty(), out long parsedId)
                        ? (long?)parsedId
                        : null
                })
                .ToArray();

            var badIds = allLoadIds
                .Where(id => !id.ParsedId.HasValue)
                .ToArray();

            if (badIds.Length > 0)
                throw new InvalidOperationException("Can't parse doc identifiers as 'long'")
                    .AndFactIs("bad-id-list", badIds.Select(id => id.OriginId).ToArray());

            var maxId = allLoadIds.Max(id => id.ParsedId.GetValueOrDefault());

            return new SeedSaver(_indexId, maxId, _seedService);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyLab.Db;
using MyLab.Log;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.Tools
{
    class DataSourceLoadEnumerator : IAsyncEnumerator<DataSourceLoad>
    {
        private readonly string _indexId;
        private readonly IAsyncEnumerator<DataSourceLoadBatch> _batchEnumerator;
        private readonly IndexType _indexType;
        private readonly ISeedService _seedService;
        public DataSourceLoad Current { get; set; }
        
        public DataSourceLoadEnumerator(
            string indexId,
            IndexType indexType,
            ISeedService seedService,
            IAsyncEnumerator<DataSourceLoadBatch> batchEnumerator)
        {
            _indexId = indexId;
            _batchEnumerator = batchEnumerator;
            _indexType = indexType;
            _seedService = seedService;
        }

        public ValueTask DisposeAsync()
        {
            return _batchEnumerator.DisposeAsync();
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            var hasResult = await _batchEnumerator.MoveNextAsync();
            if (!hasResult) return false;

            var batchEntities = _batchEnumerator.Current.Entities;

            ISeedSaver seedSaver = null;

            if (batchEntities is { Length: > 0 })
            {
                switch (_indexType)
                {
                    case IndexType.Heap:
                        {
                            seedSaver = new DtSeedSaver(_indexId, DateTime.Now, _seedService);
                        }
                        break;
                    case IndexType.Stream:
                        {
                            var allLoadIds = batchEntities
                                .Select(e => new
                                {
                                    OriginId = e.Id,
                                    ParsedId = long.TryParse(e.Id, out long parsedId)
                                        ? (long?)parsedId
                                        : null
                                })
                                .ToArray();

                            var badIds = allLoadIds
                                .Where(id => !id.ParsedId.HasValue)
                                .ToArray();

                            if (badIds.Length > 0)
                                throw new InvalidOperationException("Can't parse entity identifiers as 'long'")
                                    .AndFactIs("bad-id-list", badIds.Select(id => id.OriginId).ToArray());

                            var maxId = allLoadIds.Max(id => id.ParsedId.GetValueOrDefault());

                            seedSaver = new IdSeedSaver(_indexId, maxId, _seedService);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Current = new DataSourceLoad
            {
                Batch = _batchEnumerator.Current,
                SeedSaver = seedSaver
            };

            return true;
        }
    }
}
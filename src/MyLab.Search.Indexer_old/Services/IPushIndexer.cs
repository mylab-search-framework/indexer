using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MyLab.Log;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.Services
{
    public interface IPushIndexer
    {
        Task IndexAsync(string strEntity, string sourceId, IdxOptions idxOptions, CancellationToken cancellationToken);
    }

    class PushIndexer : IPushIndexer
    {
        private readonly IDataIndexer _indexer;

        public PushIndexer(IDataIndexer indexer)
        {
            _indexer = indexer;
        }

        public Task IndexAsync(string strEntity, string sourceId, IdxOptions idxOptions, CancellationToken cancellationToken)
        {
            if (sourceId == null) throw new ArgumentNullException(nameof(sourceId));
            if (idxOptions == null) throw new ArgumentNullException(nameof(idxOptions));

            if (string.IsNullOrWhiteSpace(strEntity))
                throw new BadIndexingRequestException("Entity object is empty");
            
            var sourceEntityDeserializer = new SourceEntityDeserializer(idxOptions.NewIndexStrategy == NewIndexStrategy.Auto);

            DataSourceEntity entity;

            try
            {
                entity = sourceEntityDeserializer.Deserialize(strEntity);
            }
            catch (Exception e)
            {
                throw new BadIndexingRequestException("Input string-entity parsing error", e)
                    .AndFactIs("dump", TrimDump(strEntity))
                    .AndFactIs("source", sourceId);
            }

            if (entity.Properties == null || entity.Properties.Count == 0)
                throw new BadIndexingRequestException("Cant detect properties in the entity object")
                    .AndFactIs("dump", TrimDump(strEntity))
                    .AndFactIs("source", sourceId);

            if (entity.Properties.Keys.All(k => k != idxOptions.IdPropertyName))
                throw new BadIndexingRequestException("Cant find ID property in the entity object")
                    .AndFactIs("dump", TrimDump(strEntity))
                    .AndFactIs("source", sourceId);

            var preproc = new DsEntityPreprocessor(idxOptions);
            var entForIndex = new[] { preproc.Process(entity) };

            return _indexer.IndexAsync(idxOptions.Id, entForIndex, cancellationToken);
        }

        string TrimDump(string dump)
        {
            const int limit = 1000;
            if (dump == null)
                return "[null]";

            if (dump.Length < limit + 1)
                return dump;

            return dump.Remove(limit) + "...";
        }
    }
}
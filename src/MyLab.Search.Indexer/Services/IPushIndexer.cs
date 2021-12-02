using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MyLab.Log;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.Services
{
    public interface IPushIndexer
    {
        Task Index(string strEntity, string sourceId, JobOptions jobOptions);
    }

    class PushIndexer : IPushIndexer
    {
        private readonly IDataIndexer _indexer;

        public PushIndexer(IDataIndexer indexer)
        {
            _indexer = indexer;
        }

        public Task Index(string strEntity, string sourceId, JobOptions jobOptions)
        {
            if (sourceId == null) throw new ArgumentNullException(nameof(sourceId));
            if (jobOptions == null) throw new ArgumentNullException(nameof(jobOptions));

            if (string.IsNullOrWhiteSpace(strEntity))
                throw new InputEntityValidationException("Entity object is empty");
            
            var sourceEntityDeserializer = new SourceEntityDeserializer(jobOptions.NewIndexStrategy == NewIndexStrategy.Auto);

            DataSourceEntity entity;

            try
            {
                entity = sourceEntityDeserializer.Deserialize(strEntity);
            }
            catch (Exception e)
            {
                throw new InputEntityValidationException("Input string-entity parsing error", e)
                    .AndFactIs("dump", TrimDump(strEntity))
                    .AndFactIs("source", sourceId);
            }

            if (entity.Properties == null || entity.Properties.Count == 0)
                throw new InputEntityValidationException("Cant detect properties in the entity object")
                    .AndFactIs("dump", TrimDump(strEntity))
                    .AndFactIs("source", sourceId);

            if (entity.Properties.Keys.All(k => k != jobOptions.IdProperty))
                throw new InputEntityValidationException("Cant find ID property in the entity object")
                    .AndFactIs("dump", TrimDump(strEntity))
                    .AndFactIs("source", sourceId);

            var preproc = new DsEntityPreprocessor(jobOptions);
            var entForIndex = new[] { preproc.Process(entity) };

            return _indexer.IndexAsync(jobOptions.JobId, entForIndex, CancellationToken.None);
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
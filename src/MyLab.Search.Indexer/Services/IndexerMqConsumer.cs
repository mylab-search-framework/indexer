using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyLab.RabbitClient.Consuming;
using MyLab.Search.EsAdapter;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Tools;
using MyLab.Log;

namespace MyLab.Search.Indexer.Services
{
    public class IndexerMqConsumer : RabbitConsumer<string>
    {
        private readonly IndexerOptions _options;
        private readonly IDataIndexer _indexer;
        private readonly MqCaseOptionsValidator _optionsValidator;

        public IndexerMqConsumer(
            IOptions<IndexerOptions> options,
            IOptions<ElasticsearchOptions> esOptions,
            IDataIndexer indexer)
            :this(options.Value, esOptions.Value, indexer)
        {
            
        }

        public IndexerMqConsumer(
            IndexerOptions options,
            ElasticsearchOptions esOptions,
            IDataIndexer indexer)
        {
            _options = options;
            _indexer = indexer;
            _optionsValidator = new MqCaseOptionsValidator(options, esOptions);
        }

        protected override Task ConsumeMessageAsync(ConsumedMessage<string> consumedMessage)
        {
            _optionsValidator.Validate();

            string strContent = consumedMessage.Content;

            if (strContent == null)
                throw new InvalidOperationException("Empty message payload detected");

            var jobOpts = _options.Jobs.FirstOrDefault(j => j.MqQueue == consumedMessage.Queue);
            if(jobOpts == null)
                throw new InvalidOperationException("Job not found for queue")
                    .AndFactIs("queue", consumedMessage.Queue);

            var sourceEntityDeserializer = new SourceEntityDeserializer(jobOpts.NewIndexStrategy == NewIndexStrategy.Auto);

            DataSourceEntity entity;

            try
            {
                entity = sourceEntityDeserializer.Deserialize(strContent);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Input MQ message parsing error", e)
                    .AndFactIs("dump", TrimDump(strContent));
            }

            if (entity.Properties == null || entity.Properties.Count == 0)
                throw new InvalidOperationException("Cant detect properties in message object")
                    .AndFactIs("dump", TrimDump(strContent));

            if (entity.Properties.Keys.All(k => k != jobOpts.IdProperty))
                throw new InvalidOperationException("Cant find ID property in message object")
                    .AndFactIs("dump", TrimDump(strContent));

            return _indexer.IndexAsync(jobOpts.JobId, new[] { entity }, CancellationToken.None);
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
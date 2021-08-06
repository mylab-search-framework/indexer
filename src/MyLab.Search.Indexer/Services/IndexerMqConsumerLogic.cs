using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using Microsoft.Extensions.Options;
using MyLab.Log;
using MyLab.Mq;
using MyLab.Mq.PubSub;
using MyLab.Search.EsAdapter;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.Services
{
    public class IndexerMqConsumerLogic : IMqConsumerLogic<string>
    {
        private readonly IndexerOptions _options;
        private readonly IDataIndexer _indexer;
        private readonly SourceEntityDeserializer _sourceEntityDeserializer;
        private readonly MqCaseOptionsValidator _optionsValidator;

        public IndexerMqConsumerLogic(
            IOptions<IndexerOptions> options,
            IOptions<ElasticsearchOptions> esOptions,
            IDataIndexer indexer)
            :this(options.Value, esOptions.Value, indexer)
        {
            
        }

        public IndexerMqConsumerLogic(
            IndexerOptions options,
            ElasticsearchOptions esOptions,
            IDataIndexer indexer)
        {
            _options = options;
            _indexer = indexer;
            _sourceEntityDeserializer = new SourceEntityDeserializer(options.NewIndexStrategy == NewIndexStrategy.Auto);
            _optionsValidator = new MqCaseOptionsValidator(options, esOptions);
        }

        public Task Consume(MqMessage<string> message)
        {
            _optionsValidator.Validate();

            if(message.Payload == null)
                throw new InvalidOperationException("Empty message payload detected");

            DataSourceEntity entity;

            try
            {
                entity= _sourceEntityDeserializer.Deserialize(message.Payload);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Input MQ message parsing error", e)
                    .AndFactIs("dump", TrimDump(message.Payload));
            }

            if (entity.Properties == null || entity.Properties.Count == 0)
                throw new InvalidOperationException("Cant detect properties in message object")
                    .AndFactIs("dump", TrimDump(message.Payload));

            if (entity.Properties.Keys.All(k => k != _options.IdProperty))
                throw new InvalidOperationException("Cant find ID property in message object")
                    .AndFactIs("dump", TrimDump(message.Payload));

            return _indexer.IndexAsync(new[] {entity}, CancellationToken.None);
        }

        string TrimDump(string dump)
        {
            const int limit = 1000;
            if (dump == null)
                return "[null]";

            if (dump.Length < limit+1)
                return dump;

            return dump.Remove(limit) + "...";
        }
    }
}
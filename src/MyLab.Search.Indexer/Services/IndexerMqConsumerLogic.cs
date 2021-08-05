using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyLab.Log;
using MyLab.Mq;
using MyLab.Mq.PubSub;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.Services
{
    public class IndexerMqConsumerLogic : IMqConsumerLogic<string>
    {
        private readonly IndexerOptions _options;
        private readonly IDataIndexer _indexer;
        private readonly SourceEntityDeserializer _sourceEntityDeserializer;

        public IndexerMqConsumerLogic(
            IOptions<IndexerOptions> options,
            IDataIndexer indexer)
            :this(options.Value, indexer)
        {
            
        }

        public IndexerMqConsumerLogic(
            IndexerOptions options,
            IDataIndexer indexer)
        {
            _options = options;
            _indexer = indexer;
            _sourceEntityDeserializer = new SourceEntityDeserializer(options.EntityMappingMode == EntityMappingMode.Auto);
        }

        public Task Consume(MqMessage<string> message)
        {
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
                    .AndFactIs("message (500max)", message.Payload.Remove(500));
            }

            if (entity.Properties == null || entity.Properties.Count == 0)
                throw new InvalidOperationException("Cant detect properties in message object")
                    .AndFactIs("message (500max)", message.Payload.Remove(500));

            if (entity.Properties.Keys.All(k => k != _options.IdFieldName))
                throw new InvalidOperationException("Cant find ID property in message object")
                    .AndFactIs("message (500max)", message.Payload.Remove(500));

            return _indexer.IndexAsync(new[] {entity}, CancellationToken.None);
        }
    }
}
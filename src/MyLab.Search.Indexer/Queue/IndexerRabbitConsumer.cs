using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyLab.Log.Dsl;
using MyLab.RabbitClient.Consuming;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Services;
using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Queue
{
    public class IndexerRabbitConsumer : RabbitConsumer<IndexingMqMessage>
    {
        private readonly IInputRequestProcessor _inputRequestProcessor;
        private readonly IDslLogger _log;

        public IndexerRabbitConsumer(IInputRequestProcessor inputRequestProcessor, ILogger<IndexerRabbitConsumer> logger)
        {
            _inputRequestProcessor = inputRequestProcessor;
            _log = logger?.Dsl();
        }

        protected override Task ConsumeMessageAsync(ConsumedMessage<IndexingMqMessage> consumedMessage)
        {
            _log?.Debug("Incoming MQ message")
                .AndFactIs("body", JObject.FromObject(consumedMessage.Content))
                .Write();

            consumedMessage.Content.Validate();

            var indexingRequest = consumedMessage.Content.ExtractIndexingRequest();
            
            return _inputRequestProcessor.IndexAsync(indexingRequest);
        }
    }
}

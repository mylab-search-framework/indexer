using System.Linq;
using System.Threading.Tasks;
using MyLab.RabbitClient.Consuming;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Services;
using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Queue
{
    public class IndexerRabbitConsumer : RabbitConsumer<IndexingMqMessage>
    {
        private readonly IInputRequestProcessor _inputRequestProcessor;

        public IndexerRabbitConsumer(IInputRequestProcessor inputRequestProcessor)
        {
            _inputRequestProcessor = inputRequestProcessor;
        }

        protected override Task ConsumeMessageAsync(ConsumedMessage<IndexingMqMessage> consumedMessage)
        {
            consumedMessage.Content.Validate();

            var indexingRequest = consumedMessage.Content.ExtractIndexingRequest();
            
            return _inputRequestProcessor.IndexAsync(indexingRequest);
        }
    }
}

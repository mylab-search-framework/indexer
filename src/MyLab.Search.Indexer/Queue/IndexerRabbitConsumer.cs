using System.Threading.Tasks;
using MyLab.RabbitClient.Consuming;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.Queue
{
    public class IndexerRabbitConsumer : RabbitConsumer<IndexingRequest>
    {
        private readonly IInputRequestProcessor _inputRequestProcessor;

        public IndexerRabbitConsumer(IInputRequestProcessor inputRequestProcessor)
        {
            _inputRequestProcessor = inputRequestProcessor;
        }

        protected override Task ConsumeMessageAsync(ConsumedMessage<IndexingRequest> consumedMessage)
        {
            return _inputRequestProcessor.IndexAsync(consumedMessage.Content);
        }
    }
}

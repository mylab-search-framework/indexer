using AutoMapper;
using MediatR;
using MyLab.Log.Dsl;
using MyLab.RabbitClient.Consuming;
using MyLab.Search.Indexer.Handlers.IndexInstructions;
using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.MqConsuming
{
    public class IndexerMqConsumer(IMediator mediator, IMapper mapper, ILogger<IndexerMqConsumer>? logger = null) : RabbitConsumer<IndexerMqMessage>
    {
        private readonly IDslLogger? _log = logger?.Dsl();
        
        protected override Task ConsumeMessageAsync(ConsumedMessage<IndexerMqMessage> consumedMessage)
        {
            _log?.Debug("Incoming MQ message")
                .AndFactIs("body", JObject.FromObject(consumedMessage.Content))
                .Write();

            var cmd= mapper.Map<IndexerMqMessage, IndexInstructionsCommand>(consumedMessage.Content);

            return mediator.Send(cmd);
        }
    }
}

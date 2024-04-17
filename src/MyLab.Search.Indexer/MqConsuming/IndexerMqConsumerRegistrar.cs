using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.RabbitClient.Consuming;
using MyLab.Search.Indexer.Options;

namespace MyLab.Search.Indexer.MqConsuming
{
    class IndexerMqConsumerRegistrar : IRabbitConsumerRegistrar
    {
        public void Register(IRabbitConsumerRegistry registry, IServiceProvider serviceProvider)
        {
            var indexerOptions = serviceProvider.GetService<IOptions<IndexerOptions>>();

            var mqQueue = indexerOptions?.Value.Queue;

            if (string.IsNullOrEmpty(mqQueue))
            {
                var logger = serviceProvider.GetService<ILogger<IndexerMqConsumerRegistrar>>();
                if (logger != null)
                {
                    logger.Dsl().Warning("MqQueue configuration is not specified and will not connected").Write();
                }
            }
            else
            {
                registry.Register(mqQueue, new TypedConsumerProvider<IndexerMqConsumer>());
            }
        }
    }
}
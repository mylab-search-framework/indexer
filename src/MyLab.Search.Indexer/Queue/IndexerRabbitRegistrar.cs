using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.RabbitClient.Consuming;

namespace MyLab.Search.Indexer.Queue
{
    class IndexerRabbitRegistrar : IRabbitConsumerRegistrar
    {
        public void Register(IRabbitConsumerRegistry registry, IServiceProvider serviceProvider)
        {
            var indexerOptions = serviceProvider.GetService<IOptions<IndexerOptions>>();

            var mqQueue = indexerOptions?.Value?.MqQueue;

            if (string.IsNullOrEmpty(mqQueue))
            {
                var logger = serviceProvider.GetService<ILogger<IndexerRabbitRegistrar>>();
                if (logger != null)
                {
                    logger.Dsl().Warning("MqQueue configuration is not specified and will not connected").Write();
                }
            }
            else
            {
                registry.Register(mqQueue, new TypedConsumerProvider<IndexerRabbitConsumer>());
            }
        }
    }
}
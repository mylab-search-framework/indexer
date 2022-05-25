using System;
using System.Linq;
using Microsoft.Extensions.Options;
using MyLab.RabbitClient.Consuming;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.Tools
{
    public class IndexerConsumerRegistrar : IRabbitConsumerRegistrar
    {
        private readonly IndexerOptions _opts;

        public IndexerConsumerRegistrar(IOptions<IndexerOptions> opts)
            :this(opts.Value)
        {
            
        }

        public IndexerConsumerRegistrar(IndexerOptions opts)
        {
            _opts = opts;
        }

        public void Register(IRabbitConsumerRegistry registry, IServiceProvider serviceProvider)
        {
            foreach (var ns in _opts.Indexes.Where(j => j.MqQueue != null))
            {
                registry.Add(ns.MqQueue, new TypedConsumerProvider<IndexerMqConsumer>());
            }
        }
    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyLab.RabbitClient.Consuming;
using MyLab.Log;

namespace MyLab.Search.Indexer.Services
{
    public class IndexerMqConsumer : RabbitConsumer<string>
    {
        private readonly IndexerOptions _options;
        private readonly IPushIndexer _pushIndexer;

        public IndexerMqConsumer(
            IOptions<IndexerOptions> options,
            IPushIndexer pushIndexer)
            :this(options.Value, pushIndexer)
        {
            
        }

        public IndexerMqConsumer(
            IndexerOptions options,
            IPushIndexer pushIndexer)
        {
            _options = options;
            _pushIndexer = pushIndexer;
        }

        protected override Task ConsumeMessageAsync(ConsumedMessage<string> consumedMessage)
        {
            if (consumedMessage.Content == null)
                throw new InvalidOperationException("Empty message payload detected");

            var jobOpts = _options.Jobs.FirstOrDefault(j => j.MqQueue == consumedMessage.Queue);
            if(jobOpts == null)
                throw new InvalidOperationException("Job not found for queue")
                    .AndFactIs("queue", consumedMessage.Queue);

            try
            {
                return _pushIndexer.IndexAsync(consumedMessage.Content, "mq", jobOpts, CancellationToken.None);
            }
            catch (Exception e)
            {
                e.AndFactIs("queue", consumedMessage.Queue);
                throw;
            }
        }
    }
}
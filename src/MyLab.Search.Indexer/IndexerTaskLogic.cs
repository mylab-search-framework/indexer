using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyLab.TaskApp;

namespace MyLab.Search.Indexer
{
    public class IndexerTaskLogic : ITaskLogic
    {
        private readonly IndexerOptions _options;

        public IndexerTaskLogic(IOptions<IndexerOptions> options)
            :this(options.Value)
        {
            
        }

        public IndexerTaskLogic(IndexerOptions options)
        {
            _options = options;
        }

        public Task Perform(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
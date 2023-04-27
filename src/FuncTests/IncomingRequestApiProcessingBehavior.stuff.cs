using System.Threading.Tasks;
using MyLab.ApiClient.Test;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Services;
using MyLab.Search.IndexerClient;
using Xunit.Abstractions;

namespace FuncTests
{
    public partial class IncomingRequestApiProcessingBehavior
    {
        private readonly ITestOutputHelper _output;
        private readonly TestApi<Startup, IIndexerV2Api> _testApi;

        public IncomingRequestApiProcessingBehavior(ITestOutputHelper output)
        {
            _output = output;
            _testApi = new TestApi<Startup, IIndexerV2Api>
            {
                Output = output
            };
        }

        public void Dispose()
        {
            _testApi?.Dispose();
        }

        class TestInputRequestProcessor : IInputRequestProcessor
        {
            public InputIndexingRequest LastRequest { get; private set; }

            public Task IndexAsync(InputIndexingRequest inputRequest)
            {
                LastRequest = inputRequest;

                return Task.CompletedTask;
            }
        }
    }
}
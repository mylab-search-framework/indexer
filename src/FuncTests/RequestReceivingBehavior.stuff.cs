using System.Threading.Tasks;
using MyLab.ApiClient.Test;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.Services;
using MyLab.Search.IndexerClient;
using Xunit.Abstractions;
using IndexingRequest = MyLab.Search.Indexer.Models.IndexingRequest;

namespace FuncTests
{
    public partial class RequestReceivingBehavior
    {
        private readonly ITestOutputHelper _output;
        private readonly TestApi<Startup, IIndexerV2> _testApi;

        public RequestReceivingBehavior(ITestOutputHelper output)
        {
            _output = output;
            _testApi = new TestApi<Startup, IIndexerV2>
            {
                Output = output
            };
        }

        public void Dispose()
        {
            _testApi?.Dispose();
        }

        private class TestInputRequestProcessor : IInputRequestProcessor
        {
            public IndexingRequest LastRequest { get; private set; }

            public Task ProcessRequestAsync(IndexingRequest request)
            {
                LastRequest = request;

                return Task.CompletedTask;
            }
        }

        private class TestEntity
        {
            public string Id { get; set; }
        }
    }
}
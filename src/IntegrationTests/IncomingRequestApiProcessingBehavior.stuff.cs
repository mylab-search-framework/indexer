using MyLab.ApiClient.Test;
using MyLab.Search.Indexer;
using MyLab.Search.IndexerClient;
using Xunit.Abstractions;

namespace IntegrationTests
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
    }
}
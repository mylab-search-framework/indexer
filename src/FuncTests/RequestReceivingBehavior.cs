using MyLab.ApiClient.Test;
using MyLab.Search.Indexer;
using MyLab.Search.IndexerClient;
using Xunit.Abstractions;

namespace FuncTests
{
    public class RequestReceivingBehavior 
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
    }
}

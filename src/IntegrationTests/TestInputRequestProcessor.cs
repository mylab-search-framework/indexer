using System.Threading.Tasks;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Services;

namespace IntegrationTests
{
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
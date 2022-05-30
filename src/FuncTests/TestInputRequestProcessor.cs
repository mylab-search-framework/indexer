using System.Threading.Tasks;
using MyLab.Search.Indexer.Services;

namespace FuncTests
{
    class TestInputRequestProcessor : IInputRequestProcessor
    {
        public IndexingRequest LastRequest { get; private set; }

        public Task IndexAsync(IndexingRequest request)
        {
            LastRequest = request;

            return Task.CompletedTask;
        }
    }
}
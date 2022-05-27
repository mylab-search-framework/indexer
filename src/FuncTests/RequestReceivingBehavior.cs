using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MyLab.ApiClient.Test;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.Services;
using MyLab.Search.IndexerClient;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;
using IndexingRequest = MyLab.Search.Indexer.Models.IndexingRequest;

namespace FuncTests
{
    public class RequestReceivingBehavior : IDisposable
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

        [Fact]
        public async Task ShouldGetRequestThroughApi()
        {
            //Arrange
            var inputSrvProc = new TestInputRequestProcessor();

            var api = _testApi.StartWithProxy(srv => 
                    srv.AddSingleton<IInputRequestProcessor>(inputSrvProc)
                );

            var testEntity = new TestEntity
            {
                Id = Guid.NewGuid().ToString("N")
            };
            var testReq = new MyLab.Search.IndexerClient.IndexingRequest
            {
                IndexId = "foo",
                Post = new [] { JObject.FromObject(testEntity) },
                Kick = new[] { "bar" }
            };

            //Act
            await api.IndexAsync(testReq);

            var actualRequest = inputSrvProc.LastRequest;

            //Assert
            Assert.NotNull(actualRequest);
            Assert.Equal("foo", actualRequest.IndexId);
            Assert.Equal(testEntity.Id, actualRequest.Post?.FirstOrDefault()?.ToObject<TestEntity>()?.Id);
            Assert.Equal("bar", actualRequest.Kick?.FirstOrDefault());
        }

        public void Dispose()
        {
            _testApi?.Dispose();
        }

        class TestInputRequestProcessor : IInputRequestProcessor
        {
            public IndexingRequest LastRequest { get; private set; }

            public Task ProcessRequestAsync(IndexingRequest request)
            {
                LastRequest = request;

                return Task.CompletedTask;
            }
        }

        class TestEntity
        {
            public string Id { get; set; }
        }
    }
}

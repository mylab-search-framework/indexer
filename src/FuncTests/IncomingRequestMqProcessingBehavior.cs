using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.RabbitClient.Publishing;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.Configuration;
using MyLab.Search.Indexer.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace FuncTests
{
    public class IncomingRequestMqProcessingBehavior
    {
        private readonly ITestOutputHelper _output;

        public IncomingRequestMqProcessingBehavior(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ShouldGetRequestThroughQueue()
        {
            //Arrange
            var postWithoutIdEnt = new TestEntity(){Content = Guid.NewGuid().ToString("N")};
            var postEnt = TestEntity.Generate();
            var putEnt = TestEntity.Generate();
            var patchEnt = TestEntity.Generate();

            var deleteId = Guid.NewGuid().ToString("N");
                
            var testReq = new MyLab.Search.IndexerClient.IndexingMqMessage
            {
                IndexId = "foo-index",
                Post = new[]
                {
                    JObject.FromObject(postEnt),
                    JObject.FromObject(postWithoutIdEnt)
                },
                Put = new []
                {
                    JObject.FromObject(putEnt) 
                },
                Patch = new[]
                {
                    JObject.FromObject(patchEnt)
                },
                Delete = new []
                {
                    deleteId
                }
            };

            var config = new ConfigurationBuilder()
                .Build();

            var srvCollection = new ServiceCollection();

            var startup = new Startup(config);
            startup.ConfigureServices(srvCollection);
            srvCollection.AddRabbitEmulation();

            srvCollection.AddLogging(l => l.AddFilter(lvl => true).AddXUnit(_output));

            var inputSrvProc = new TestInputRequestProcessor();
            srvCollection.AddSingleton<IInputRequestProcessor>(inputSrvProc);

            srvCollection.Configure<IndexerOptions>(opt => { opt.MqQueue = "foo-queue"; });

            var serviceProvider = srvCollection.BuildServiceProvider();
            var publisher = serviceProvider.GetRequiredService<IRabbitPublisher>();

            //Act
            publisher
                .IntoQueue("foo-queue")
                .SendJson(testReq)
                .Publish();

            var actualRequest = inputSrvProc.LastRequest;

            var actualPostEnt = actualRequest.PostList?.FirstOrDefault(e => e.Id == postEnt.Id);
            var actualPostEntWithoutId = actualRequest.PostList?.FirstOrDefault(e => e.Id == null);
            var actualPutEnt = actualRequest.PutList?.FirstOrDefault(e => e.Id == putEnt.Id);
            var actualPatchEnt = actualRequest.PatchList?.FirstOrDefault(e => e.Id == patchEnt.Id);

            //Assert
            Assert.NotNull(actualRequest);
            Assert.Equal("foo-index", actualRequest.IndexId);
            
        }

        private class TestEntity
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            public string Content { get; set; }

            public static TestEntity Generate()
            {
                return new TestEntity
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Content = Guid.NewGuid().ToString("N")
                };
            }
            
        }
    }
}
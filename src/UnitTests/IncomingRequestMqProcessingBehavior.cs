using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.RabbitClient.Publishing;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests
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

            srvCollection.Configure<IndexerOptions>(opt => opt.MqQueue = "foo-queue");

            var serviceProvider = srvCollection.BuildServiceProvider();
            var publisher = serviceProvider.GetRequiredService<IRabbitPublisher>();

            //Act
            publisher
                .IntoQueue("foo-queue")
                .SetJsonContent(testReq)
                .Publish();

            var actualRequest = inputSrvProc.LastRequest;

            var actualPostIndexEnt = actualRequest.PostList?.FirstOrDefault(e => e.Id == postEnt.Id);
            var actualPostIndexEntWithoutId = actualRequest.PostList?.FirstOrDefault(e => e.Id == null);
            var actualPutIndexEnt = actualRequest.PutList?.FirstOrDefault(e => e.Id == putEnt.Id);
            var actualPatchIndexEnt = actualRequest.PatchList?.FirstOrDefault(e => e.Id == patchEnt.Id);

            var actualPostEnt = actualPostIndexEnt?.Entity?.ToObject<TestEntity>();
            var actualPutEnt = actualPutIndexEnt?.Entity?.ToObject<TestEntity>();
            var actualPatchEnt = actualPatchIndexEnt?.Entity?.ToObject<TestEntity>();
            var actualPostEntWithoutId = actualPostIndexEntWithoutId?.Entity?.ToObject<TestEntity>();

            //Assert
            Assert.Equal("foo-index", actualRequest.IndexId);
            
            Assert.NotNull(actualPostIndexEnt);
            Assert.Equal(postEnt.Id, actualPostIndexEnt.Id);
            Assert.NotNull(actualPostEnt);
            Assert.Equal(postEnt.Id, actualPostEnt.Id);
            Assert.Equal(postEnt.Content, actualPostEnt.Content);

            Assert.NotNull(actualPostIndexEntWithoutId);
            Assert.Equal(postWithoutIdEnt.Id, actualPostIndexEntWithoutId.Id);
            Assert.NotNull(actualPostEntWithoutId);
            Assert.Null(actualPostEntWithoutId.Id);
            Assert.Equal(postWithoutIdEnt.Content, actualPostEntWithoutId.Content);

            Assert.NotNull(actualPutIndexEnt);
            Assert.Equal(putEnt.Id, actualPutIndexEnt.Id);
            Assert.NotNull(actualPutEnt);
            Assert.Equal(putEnt.Id, actualPutEnt.Id);
            Assert.Equal(putEnt.Content, actualPutEnt.Content);

            Assert.NotNull(actualPatchIndexEnt);
            Assert.Equal(patchEnt.Id, actualPatchIndexEnt.Id);
            Assert.NotNull(actualPatchEnt);
            Assert.Equal(patchEnt.Id, actualPatchEnt.Id);
            Assert.Equal(patchEnt.Content, actualPatchEnt.Content);

            Assert.Single(actualRequest.DeleteList);
            Assert.Equal(deleteId, actualRequest.DeleteList[0]);
        }

        
    }
}
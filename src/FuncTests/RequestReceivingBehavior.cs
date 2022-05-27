using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.RabbitClient;
using MyLab.RabbitClient.Consuming;
using MyLab.RabbitClient.Publishing;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Services;
using Newtonsoft.Json.Linq;
using Xunit;

namespace FuncTests
{
    public partial class RequestReceivingBehavior : IDisposable
    {
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

        [Fact]
        public void ShouldGetRequestThroughQueue()
        {
            //Arrange
            var testEntity = new TestEntity
            {
                Id = Guid.NewGuid().ToString("N")
            };
            var testReq = new MyLab.Search.IndexerClient.IndexingRequest
            {
                IndexId = "foo",
                Post = new[] { JObject.FromObject(testEntity) },
                Kick = new[] { "bar" }
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

            srvCollection.Configure<IndexerOptions>(opt =>
            {
                opt.MqQueue = "foo-queue";
            });

            var serviceProvider = srvCollection.BuildServiceProvider();
            var publisher = serviceProvider.GetRequiredService<IRabbitPublisher>();

            //Act
            publisher
                .IntoQueue("foo-queue")
                .SendJson(testReq)
                .Publish();

            var actualRequest = inputSrvProc.LastRequest;

            //Assert
            Assert.NotNull(actualRequest);
            Assert.Equal("foo", actualRequest.IndexId);
            Assert.Equal(testEntity.Id, actualRequest.Post?.FirstOrDefault()?.ToObject<TestEntity>()?.Id);
            Assert.Equal("bar", actualRequest.Kick?.FirstOrDefault());
        }
    }
}

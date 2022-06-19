using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyLab.RabbitClient.Model;
using MyLab.Search.EsAdapter;
using MyLab.Search.EsAdapter.Indexing;
using MyLab.Search.EsAdapter.Search;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.Options;
using MyLab.Search.IndexerClient;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace FuncTests
{
    public class IndexerMqBehavior :
        IClassFixture<EsFixture<TestEsFixtureStrategy>>,
        IAsyncLifetime,
        IDisposable
    {
        private readonly RabbitQueue _queue;
        private readonly IWebHost _host;
        private readonly EsIndexer<TestDoc> _indexer;
        private readonly EsSearcher<TestDoc> _searcher;

        public IndexerMqBehavior(EsFixture<TestEsFixtureStrategy> esFxt, ITestOutputHelper output)
        {
            esFxt.Output = output;

            var esIndexName = Guid.NewGuid().ToString("N");
            var indexNameProvider = new SingleIndexNameProvider(esIndexName);

            _indexer = new EsIndexer<TestDoc>(esFxt.Indexer, indexNameProvider);
            _searcher = new EsSearcher<TestDoc>(esFxt.Searcher, indexNameProvider);

            var queueFactory = new RabbitQueueFactory(TestTools.RabbitChannelProvider);
            _queue = queueFactory.CreateWithRandomId();

            var config = new ConfigurationBuilder().Build();

            var hostBldr = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<Startup>()
                .UseConfiguration(config)
                .ConfigureServices(srv =>
                {
                    srv.Configure<IndexerOptions>(opt =>
                        {
                            opt.ResourcePath = Path.Combine(Directory.GetCurrentDirectory(), "resources");
                            opt.Indexes = new[]
                            {
                                new IndexOptions
                                {
                                    Id = "baz",
                                    EsIndex = esIndexName
                                }
                            };
                            opt.MqQueue = _queue.Name;
                        })
                        .ConfigureEsTools(opt => { opt.Url = TestTools.EsUrl; })
                        .ConfigureRabbit(opt =>
                        {
                            opt.Host = "localhost";
                            opt.User = "guest";
                            opt.Password = "guest";
                        })
                        .AddLogging(l => l
                            .ClearProviders()
                            .AddFilter(f => true)
                            .AddXUnit(output)
                        );
                });

            _host = hostBldr.Build();

            _host.Start();
        }

        [Fact]
        public async Task ShouldPost()
        {
            //Arrange
            var doc = TestDoc.Generate();

            var mqMsg = new IndexingMqMessage
            {
                IndexId = "baz",
                Post = new[] { JObject.FromObject(doc) }
            };

            //Act
            _queue.Publish(mqMsg);

            await Task.Delay(1000);

            var found = await SearchByIdAsync(doc.Id);

            //Assert
            Assert.Single(found);
            Assert.Equal(doc.Id, found[0].Id);
            Assert.Equal(doc.Content, found[0].Content);
        }

        [Fact]
        public async Task ShouldPutCreate()
        {
            //Arrange
            var doc = TestDoc.Generate();

            var mqMsg = new IndexingMqMessage
            {
                IndexId = "baz",
                Put = new[] { JObject.FromObject(doc) }
            };

            //Act
            _queue.Publish(mqMsg);

            await Task.Delay(1000);

            var found = await SearchByIdAsync(doc.Id);

            //Assert
            Assert.Single(found);
            Assert.Equal(doc.Id, found[0].Id);
            Assert.Equal(doc.Content, found[0].Content);
        }

        [Fact]
        public async Task ShouldPutUpdate()
        {
            //Arrange
            var docV1 = TestDoc.Generate();
            var docV2 = TestDoc.Generate(docV1.Id);
            
            var mqMsg = new IndexingMqMessage
            {
                IndexId = "baz",
                Put = new[] { JObject.FromObject(docV2) }
            };

            await _indexer.IndexAsync(docV1);

            await Task.Delay(500);

            //Act
            _queue.Publish(mqMsg);

            await Task.Delay(1000);

            var found = await SearchByIdAsync(docV1.Id);

            //Assert
            Assert.Single(found);
            Assert.Equal(docV2.Content, found[0].Content);
        }

        [Fact]
        public async Task ShouldPatch()
        {
            //Arrange
            var docV1 = TestDoc.Generate();
            var docV2 = TestDoc.Generate(docV1.Id);

            var mqMsg = new IndexingMqMessage
            {
                IndexId = "baz",
                Patch = new[] { JObject.FromObject(docV2) }
            };

            await _indexer.IndexAsync(docV1);

            await Task.Delay(500);

            //Act
            _queue.Publish(mqMsg);

            await Task.Delay(1000);

            var found = await SearchByIdAsync(docV1.Id);

            //Assert
            Assert.Single(found);
            Assert.Equal(docV2.Content, found[0].Content);
        }

        [Fact]
        public async Task ShouldDelete()
        {
            //Arrange
            var doc = TestDoc.Generate();

            var mqMsg = new IndexingMqMessage
            {
                IndexId = "baz",
                Delete = new []{ doc.Id.ToString() }
            };

            await _indexer.IndexAsync(doc);

            await Task.Delay(500);

            //Act
            _queue.Publish(mqMsg);

            await Task.Delay(1000);

            var found = await SearchByIdAsync(doc.Id);

            //Assert
            Assert.Empty(found);
        }

        Task<EsFound<TestDoc>> SearchByIdAsync(int id)
        {
            return _searcher.SearchAsync(new EsSearchParams<TestDoc>(q => q.Ids(idd => idd.Values(id))));
        }

        public void Dispose()
        {
            _queue.Remove();
            _host.Dispose();
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await _host.StopAsync();
        }
    }
}

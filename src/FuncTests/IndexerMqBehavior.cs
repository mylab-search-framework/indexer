using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyLab.ApiClient;
using MyLab.ApiClient.Test;
using MyLab.Db;
using MyLab.DbTest;
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
        IClassFixture<TmpDbFixture<TestDbInitializer>>,
        IAsyncLifetime,
        IDisposable
    {
        private RabbitQueue _queue;
        private EsIndexer<TestDoc> _indexer;
        private EsSearcher<TestDoc> _searcher;
        private readonly TmpDbFixture<TestDbInitializer> _dbFxt;
        private readonly ITestOutputHelper _output;
        private readonly EsFixture<TestEsFixtureStrategy> _esFxt;
        private TestApi<Startup, IApiKickerContract> _testApi;
        private IApiKickerContract _kickApi;
        private IDbManager _dbMgr;

        public IndexerMqBehavior(
            TmpDbFixture<TestDbInitializer> dbFxt, 
            EsFixture<TestEsFixtureStrategy> esFxt, 
            ITestOutputHelper output)
        {
            _dbFxt = dbFxt;
            _output = output;
            _dbFxt.Output = output;

            _esFxt = esFxt;
            _esFxt.Output = output;
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
            await Task.Delay(500);
            await _kickApi.KickAsync();
            await Task.Delay(1000);

            var found = await SearchByIdAsync(doc.Id);

            //Assert
            Assert.Single(found);
            Assert.Equal(doc.Id, found[0].Id);
            Assert.Equal(doc.Content, found[0].Content);
        }

        [Fact]
        public async Task ShouldPutNew()
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
            await Task.Delay(500);
            await _kickApi.KickAsync();
            await Task.Delay(1000);

            var found = await SearchByIdAsync(doc.Id);

            //Assert
            Assert.Single(found);
            Assert.Equal(doc.Id, found[0].Id);
            Assert.Equal(doc.Content, found[0].Content);
        }

        [Fact]
        public async Task ShouldPutIndexed()
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
            await Task.Delay(500);
            await _kickApi.KickAsync();
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
            await Task.Delay(500);
            await _kickApi.KickAsync();
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
            await Task.Delay(500);
            await _kickApi.KickAsync();
            await Task.Delay(1000);

            var found = await SearchByIdAsync(doc.Id);

            //Assert
            Assert.Empty(found);
        }

        [Fact]
        public async Task ShouldKickNew()
        {
            //Arrange
            var doc = TestDoc.Generate();

            var mqMsg = new IndexingMqMessage
            {
                IndexId = "baz",
                Kick = new []{ doc.Id.ToString() }
            };
            
            var insertedCount = await _dbMgr.DoOnce().InsertAsync(doc);
            
            //Act
            _queue.Publish(mqMsg);
            await Task.Delay(500);
            await _kickApi.KickAsync();
            await Task.Delay(1000);
            
            var found = await SearchByIdAsync(doc.Id);

            //Assert
            Assert.Equal(1, insertedCount);
            Assert.Single(found);
            Assert.Equal(doc.Id, found[0].Id);
            Assert.Equal(doc.Content, found[0].Content);
        }

        [Fact]
        public async Task ShouldKickIndexed()
        {
            //Arrange
            var docV1 = TestDoc.Generate();
            var docV2 = TestDoc.Generate(docV1.Id);

            var mqMsg = new IndexingMqMessage
            {
                IndexId = "baz",
                Kick = new[] { docV1.Id.ToString() }
            };

            await _indexer.IndexAsync(docV1);
            
            var insertedCount = await _dbMgr.DoOnce().InsertAsync(docV2);

            await Task.Delay(500);

            //Act
            _queue.Publish(mqMsg);
            await Task.Delay(500);
            await _kickApi.KickAsync();
            await Task.Delay(1000);

            var found = await SearchByIdAsync(docV1.Id);

            //Assert
            Assert.Equal(1, insertedCount);
            Assert.Single(found);
            Assert.Equal(docV2.Content, found[0].Content);
        }

        Task<EsFound<TestDoc>> SearchByIdAsync(int id)
        {
            return _searcher.SearchAsync(new EsSearchParams<TestDoc>(q => q.Ids(idd => idd.Values(id))));
        }

        public void Dispose()
        {
            _queue.Remove();
            _testApi.Dispose();
        }

        public async Task InitializeAsync()
        {
            _dbMgr = await _dbFxt.CreateDbAsync();

            var esIndexName = Guid.NewGuid().ToString("N");
            var indexNameProvider = new SingleIndexNameProvider(esIndexName);

            _indexer = new EsIndexer<TestDoc>(_esFxt.Indexer, indexNameProvider);
            _searcher = new EsSearcher<TestDoc>(_esFxt.Searcher, indexNameProvider);

            var queueFactory = new RabbitQueueFactory(TestTools.RabbitChannelProvider);
            _queue = queueFactory.CreateWithRandomId();

            _testApi = new TestApi<Startup, IApiKickerContract>
            {
                Output = _output,
                ServiceOverrider = srv =>
                {
                    srv.Configure<IndexerOptions>(opt =>
                        {
                            opt.ResourcePath = Path.Combine(Directory.GetCurrentDirectory(), "resources");
                            opt.Indexes = new[]
                            {
                                new IndexOptions
                                {
                                    Id = "baz",
                                    EsIndex = esIndexName,
                                    IdPropertyType = IdPropertyType.Int
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
                            .AddXUnit(_output)
                        )
                        .Configure<IndexerDbOptions>(opt => opt.Provider = "sqlite")
                        .AddSingleton(_dbMgr);
                }
            };

            _kickApi = _testApi.StartWithProxy();
        }

        public async Task DisposeAsync()
        {
        }

        [Api]
        interface IApiKickerContract
        {
            [Post]
            [ExpectedCode(HttpStatusCode.NotFound)]
            Task KickAsync();
        }
    }
}

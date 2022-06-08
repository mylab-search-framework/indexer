using System;
using System.Threading.Tasks;
using MyLab.Search.EsAdapter;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class IndexerServiceBehavior : IClassFixture<EsFixture<TestEsFixtureStrategy>>, IAsyncLifetime
    {
        private readonly EsFixture<TestEsFixtureStrategy> _esFxt;
        private readonly IEsSearcher<TestEntity> _searcher;

        public IndexerServiceBehavior(EsFixture<TestEsFixtureStrategy> esFixture, ITestOutputHelper output)
        {
            _esFxt = esFixture;
            _searcher = _esFxt.CreateSearcher<TestEntity>();
            _esFxt.Output = output;
        }

        [Fact]
        public async Task ShouldPostEntities()
        {
            //Arrange
            var esIndexName = Guid.NewGuid().ToString("N");

            await _esFxt.Manager.CreateIndexAsync(esIndexName);
            await Task.Delay(500);

            var opts = new IndexerOptions
            {
                Indexes = new []
                {
                    new IndexOptions
                    {
                        Id = "foo",
                        EsIndex = esIndexName,
                        IdPropertyType = IdPropertyType.String
                    }
                }
            };

            var indexer = new IndexerService(_esFxt.EsClient, opts);

            var req = new IndexingRequest
            {
                IndexId = "foo",
                PostList = new []
                {
                    new IndexingEntity
                    {
                        Id = "bar",
                        Entity = JObject.FromObject(new TestEntity("bar", "baz"))
                    }
                }
            };

            //Act
            await indexer.IndexAsync(req);
            await Task.Delay(500);
            
            var resp = await _searcher.SearchAsync(esIndexName, new SearchParams<TestEntity>(d => d.Ids(idQDesc => idQDesc.Values("bar"))));

            //Assert
            Assert.NotNull(resp);
            Assert.Single(resp);
            Assert.Equal("bar", resp[0].Id);
            Assert.Equal("baz", resp[0].Content);
        }

        [Fact]
        public async Task ShouldDeleteEntities()
        {
            //Arrange
            var esIndexName = Guid.NewGuid().ToString("N");

            await _esFxt.Manager.CreateIndexAsync(esIndexName);
            await Task.Delay(500);

            var opts = new IndexerOptions
            {
                Indexes = new[]
                {
                    new IndexOptions
                    {
                        Id = "foo",
                        EsIndex = esIndexName,
                        IdPropertyType = IdPropertyType.String
                    }
                }
            };

            var indexer = new IndexerService(_esFxt.EsClient, opts);

            var postReq = new IndexingRequest
            {
                IndexId = "foo",
                PostList = new[]
                {
                    new IndexingEntity
                    {
                        Id = "bar",
                        Entity = JObject.FromObject(new TestEntity("bar", "baz"))
                    }
                }
            };

            var delReq = new IndexingRequest
            {
                IndexId = "foo",
                DeleteList = new[]
                {
                    "bar"
                }
            };

            //Act
            await indexer.IndexAsync(postReq);
            await Task.Delay(1000);
            await indexer.IndexAsync(delReq);
            await Task.Delay(1000);

            var resp = await _searcher.SearchAsync(esIndexName, new SearchParams<TestEntity>(d => d.Ids(idQDesc => idQDesc.Values("bar"))));

            //Assert
            Assert.NotNull(resp);
            Assert.Empty(resp);
        }

        [Fact]
        public async Task ShouldPutNewEntities()
        {
            //Arrange
            var esIndexName = Guid.NewGuid().ToString("N");

            await _esFxt.Manager.CreateIndexAsync(esIndexName);
            await Task.Delay(500);

            var opts = new IndexerOptions
            {
                Indexes = new[]
                {
                    new IndexOptions
                    {
                        Id = "foo",
                        EsIndex = esIndexName,
                        IdPropertyType = IdPropertyType.String
                    }
                }
            };

            var indexer = new IndexerService(_esFxt.EsClient, opts);

            var req = new IndexingRequest
            {
                IndexId = "foo",
                PutList = new[]
                {
                    new IndexingEntity
                    {
                        Id = "bar",
                        Entity = JObject.FromObject(new TestEntity("bar", "baz"))
                    }
                }
            };

            //Act
            await indexer.IndexAsync(req);
            await Task.Delay(500);

            var resp = await _searcher.SearchAsync(esIndexName, new SearchParams<TestEntity>(d => d.Ids(idQDesc => idQDesc.Values("bar"))));

            //Assert
            Assert.NotNull(resp);
            Assert.Single(resp);
            Assert.Equal("bar", resp[0].Id);
            Assert.Equal("baz", resp[0].Content);
        }

        [Fact]
        public async Task ShouldPutEditEntities()
        {
            //Arrange
            var esIndexName = Guid.NewGuid().ToString("N");

            await _esFxt.Manager.CreateIndexAsync(esIndexName);
            await Task.Delay(500);

            var opts = new IndexerOptions
            {
                Indexes = new[]
                {
                    new IndexOptions
                    {
                        Id = "foo",
                        EsIndex = esIndexName,
                        IdPropertyType = IdPropertyType.String
                    }
                }
            };

            var indexer = new IndexerService(_esFxt.EsClient, opts);

            var postReq = new IndexingRequest
            {
                IndexId = "foo",
                PostList = new[]
                {
                    new IndexingEntity
                    {
                        Id = "bar",
                        Entity = JObject.FromObject(new TestEntity("bar", "baz"))
                    }
                }
            };

            var putReq = new IndexingRequest
            {
                IndexId = "foo",
                PutList = new[]
                {
                    new IndexingEntity
                    {
                        Id = "bar",
                        Entity = JObject.FromObject(new TestEntity("bar", "edited-baz"))
                    }
                }
            };

            //Act
            await indexer.IndexAsync(postReq);
            await Task.Delay(1000);
            await indexer.IndexAsync(putReq);
            await Task.Delay(1000);

            var resp = await _searcher.SearchAsync(esIndexName, new SearchParams<TestEntity>(d => d.Ids(idQDesc => idQDesc.Values("bar"))));

            //Assert
            Assert.NotNull(resp);
            Assert.Single(resp);
            Assert.Equal("bar", resp[0].Id);
            Assert.Equal("edited-baz", resp[0].Content);
        }

        [Fact]
        public async Task ShouldPatchEntities()
        {
            //Arrange
            var esIndexName = Guid.NewGuid().ToString("N");

            await _esFxt.Manager.CreateIndexAsync(esIndexName);
            await Task.Delay(500);

            var opts = new IndexerOptions
            {
                Indexes = new[]
                {
                    new IndexOptions
                    {
                        Id = "foo",
                        EsIndex = esIndexName,
                        IdPropertyType = IdPropertyType.String
                    }
                }
            };

            var indexer = new IndexerService(_esFxt.EsClient, opts);

            var postReq = new IndexingRequest
            {
                IndexId = "foo",
                PostList = new[]
                {
                    new IndexingEntity
                    {
                        Id = "bar",
                        Entity = JObject.FromObject(new TestEntity("bar", "baz"))
                    }
                }
            };

            var patchReq = new IndexingRequest
            {
                IndexId = "foo",
                PatchList = new[]
                {
                    new IndexingEntity
                    {
                        Id = "bar",
                        Entity = JObject.FromObject(new TestEntity("bar", "patched-baz"))
                    }
                }
            };

            //Act
            await indexer.IndexAsync(postReq);
            await Task.Delay(1000);
            await indexer.IndexAsync(patchReq);
            await Task.Delay(1000);

            var resp = await _searcher.SearchAsync(esIndexName, new SearchParams<TestEntity>(d => d.Ids(idQDesc => idQDesc.Values("bar"))));

            //Assert
            Assert.NotNull(resp);
            Assert.Single(resp);
            Assert.Equal("bar", resp[0].Id);
            Assert.Equal("patched-baz", resp[0].Content);
        }

        [Fact]
        public async Task ShouldNotPatchEntitiesIfNotExists()
        {
            //Arrange
            var esIndexName = Guid.NewGuid().ToString("N");

            await _esFxt.Manager.CreateIndexAsync(esIndexName);
            await Task.Delay(500);

            var opts = new IndexerOptions
            {
                Indexes = new[]
                {
                    new IndexOptions
                    {
                        Id = "foo",
                        EsIndex = esIndexName,
                        IdPropertyType = IdPropertyType.String
                    }
                }
            };

            var indexer = new IndexerService(_esFxt.EsClient, opts);
            
            var patchReq = new IndexingRequest
            {
                IndexId = "foo",
                PatchList = new[]
                {
                    new IndexingEntity
                    {
                        Id = "bar",
                        Entity = JObject.FromObject(new TestEntity("bar", "patched-baz"))
                    }
                }
            };

            //Act
            await indexer.IndexAsync(patchReq);
            
            //Assert
            
        }

        public async Task InitializeAsync()
        {
            await _esFxt.InitializeAsync();
        }

        public async Task DisposeAsync()
        {
            await _esFxt.DisposeAsync();
        }
    }
}

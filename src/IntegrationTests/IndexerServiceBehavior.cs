using System;
using System.Threading.Tasks;
using MyLab.Search.EsAdapter.Inter;
using MyLab.Search.EsAdapter.Search;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using Nest;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;
using IndexOptions = MyLab.Search.Indexer.Options.IndexOptions;

namespace IntegrationTests
{
    public class IndexerServiceBehavior : 
        IClassFixture<EsIndexFixture<TestDoc, TestEsFixtureStrategy>>, 
        IClassFixture<EsFixture<TestEsFixtureStrategy>>, 
        IAsyncLifetime
    {
        private readonly EsIndexFixture<TestDoc, TestEsFixtureStrategy> _idxFxt;
        private readonly EsFixture<TestEsFixtureStrategy> _fxt;

        public IndexerServiceBehavior(
            EsIndexFixture<TestDoc, TestEsFixtureStrategy> idxFxt, 
            EsFixture<TestEsFixtureStrategy> fxt, 
            ITestOutputHelper output)
        {
            _idxFxt = idxFxt;
            _fxt = fxt;
            _idxFxt.Output = output;
            _fxt.Output = output;
        }

        [Fact]
        public async Task ShouldPostEntities()
        {
            //Arrange
            var opts = new IndexerOptions
            {
                Indexes = new []
                {
                    new IndexOptions
                    {
                        Id = "foo",
                        EsIndex = _idxFxt.IndexName,
                        IdPropertyType = IdPropertyType.String
                    }
                }
            };

            var indexer = new IndexerService(_fxt.Indexer, opts);

            var doc = TestDoc.Generate();
            var req = new IndexingRequest
            {
                IndexId = "foo",
                PostList = new []
                {
                    JObject.FromObject(doc)
                }
            };

            //Act
            await indexer.IndexAsync(req);
            await Task.Delay(1000);
            
            var resp = await _idxFxt.Searcher.SearchAsync(
                new EsSearchParams<TestDoc>(
                        d => d.Ids(idQDesc => idQDesc.Values(doc.Id))
                    )
                );

            //Assert
            Assert.NotNull(resp);
            Assert.Single(resp);
            Assert.Equal(doc, resp[0]);
        }

        [Fact]
        public async Task ShouldDeleteEntities()
        {
            //Arrange
            var opts = new IndexerOptions
            {
                Indexes = new[]
                {
                    new IndexOptions
                    {
                        Id = "foo",
                        EsIndex = _idxFxt.IndexName,
                        IdPropertyType = IdPropertyType.String
                    }
                }
            };

            var indexer = new IndexerService(_fxt.Indexer, opts);

            var doc = TestDoc.Generate();
            var postReq = new IndexingRequest
            {
                IndexId = "foo",
                PostList = new[]
                {
                    JObject.FromObject(doc)
                }
            };

            var delReq = new IndexingRequest
            {
                IndexId = "foo",
                DeleteList = new[]
                {
                    doc.Id
                }
            };

            //Act
            await indexer.IndexAsync(postReq);
            await Task.Delay(1000);
            await indexer.IndexAsync(delReq);
            await Task.Delay(1000);

            var resp = await _idxFxt.Searcher.SearchAsync(
                    new EsSearchParams<TestDoc>(
                            d => d.Ids(idQDesc => idQDesc.Values(doc.Id))
                        )
                );

            //Assert
            Assert.NotNull(resp);
            Assert.Empty(resp);
        }

        [Fact]
        public async Task ShouldPutNewEntities()
        {
            //Arrange
            var opts = new IndexerOptions
            {
                Indexes = new[]
                {
                    new IndexOptions
                    {
                        Id = "foo",
                        EsIndex = _idxFxt.IndexName,
                        IdPropertyType = IdPropertyType.String
                    }
                }
            };

            var indexer = new IndexerService(_fxt.Indexer, opts);

            var doc = TestDoc.Generate();
            var req = new IndexingRequest
            {
                IndexId = "foo",
                PutList = new[]
                {
                    JObject.FromObject(doc)
                }
            };

            //Act
            await indexer.IndexAsync(req);
            await Task.Delay(500);

            var resp = await _idxFxt.Searcher.SearchAsync(
                new EsSearchParams<TestDoc>(
                    d => d.Ids(idQDesc => idQDesc.Values(doc.Id))
                )
            );

            //Assert
            Assert.NotNull(resp);
            Assert.Single(resp);
            Assert.Equal(doc, resp[0]);
        }

        [Fact]
        public async Task ShouldPutEditEntities()
        {
            //Arrange
            var opts = new IndexerOptions
            {
                Indexes = new[]
                {
                    new IndexOptions
                    {
                        Id = "foo",
                        EsIndex = _idxFxt.IndexName,
                        IdPropertyType = IdPropertyType.String
                    }
                }
            };

            var indexer = new IndexerService(_fxt.Indexer, opts);

            var doc = TestDoc.Generate();
            var postReq = new IndexingRequest
            {
                IndexId = "foo",
                PostList = new[]
                {
                    JObject.FromObject(doc)
                }
            };

            var docPatcher = new TestDoc(doc.Id, "patched");
            var putReq = new IndexingRequest
            {
                IndexId = "foo",
                PutList = new[]
                {
                    JObject.FromObject(docPatcher)
                }
            };

            //Act
            await indexer.IndexAsync(postReq);
            await Task.Delay(1000);
            await indexer.IndexAsync(putReq);
            await Task.Delay(1000);

            var resp = await _idxFxt.Searcher.SearchAsync(
                new EsSearchParams<TestDoc>(
                    d => d.Ids(idQDesc => idQDesc.Values(doc.Id))
                )
            );

            //Assert
            Assert.NotNull(resp);
            Assert.Single(resp);
            Assert.Equal(docPatcher, resp[0]);
        }

        [Fact]
        public async Task ShouldPatchEntities()
        {
            //Arrange
            var opts = new IndexerOptions
            {
                Indexes = new[]
                {
                    new IndexOptions
                    {
                        Id = "foo",
                        EsIndex = _idxFxt.IndexName,
                        IdPropertyType = IdPropertyType.String
                    }
                }
            };

            var indexer = new IndexerService(_fxt.Indexer, opts);

            var doc = TestDoc.Generate();
            var postReq = new IndexingRequest
            {
                IndexId = "foo",
                PostList = new[]
                {
                    JObject.FromObject(doc)
                }
            };

            var dockPatcher = new TestDoc(doc.Id, "patched");
            var patchReq = new IndexingRequest
            {
                IndexId = "foo",
                PatchList = new[]
                {
                    JObject.FromObject(dockPatcher)
                }
            };

            //Act
            await indexer.IndexAsync(postReq);
            await Task.Delay(1000);
            await indexer.IndexAsync(patchReq);
            await Task.Delay(1000);

            var resp = await _idxFxt.Searcher.SearchAsync(
                new EsSearchParams<TestDoc>(
                    d => d.Ids(idQDesc => idQDesc.Values(doc.Id))
                )
            );

            //Assert
            Assert.NotNull(resp);
            Assert.Single(resp);
            Assert.Equal(dockPatcher, resp[0]);
        }

        [Fact]
        public async Task ShouldNotPatchEntitiesIfNotExists()
        {
            //Arrange
            var opts = new IndexerOptions
            {
                Indexes = new[]
                {
                    new IndexOptions
                    {
                        Id = "foo",
                        EsIndex = _idxFxt.IndexName,
                        IdPropertyType = IdPropertyType.String
                    }
                }
            };

            var indexer = new IndexerService(_fxt.Indexer, opts);
            
            var patchReq = new IndexingRequest
            {
                IndexId = "foo",
                PatchList = new[]
                {
                    JObject.FromObject(TestDoc.Generate())
                }
            };

            BulkResponse expectedFailedResp = null;
            EsException expectedEsException = null;

            //Act
            try
            {
                await indexer.IndexAsync(patchReq);
            }
            catch (EsException e)
            {
                expectedEsException = e;
                expectedFailedResp = e.Response as BulkResponse;
            }

            //Assert
            Assert.NotNull(expectedEsException);
            Assert.NotNull(expectedFailedResp);

            Assert.Contains(expectedFailedResp.ItemsWithErrors, itm => itm.Status == 404);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await _idxFxt.IndexTools.PruneIndexAsync();
        }
    }
}

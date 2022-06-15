using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.ApiClient.Test;
using MyLab.DbTest;
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
using IndexOptions = MyLab.Search.Indexer.Options.IndexOptions;

namespace FuncTests
{
    public class IndexerApiBehavior : 
        IClassFixture<EsFixture<TestEsFixtureStrategy>>, 
        IClassFixture<TestApi<Startup, IIndexerV2Api>>
    {
        private readonly EsFixture<TestEsFixtureStrategy> _esFxt;
        private IIndexerV2Api _api;
        private readonly EsIndexer<TestDoc> _indexer;
        private readonly string _esIndexName;

        public IndexerApiBehavior(
            EsFixture<TestEsFixtureStrategy> esFxt,
            TestApi<Startup, IIndexerV2Api> apiFxt,
            ITestOutputHelper output)
        {
            _esFxt = esFxt;
            _esFxt.Output = output;
            apiFxt.Output = output;

            _esIndexName = Guid.NewGuid().ToString("N");

            _indexer = new EsIndexer<TestDoc>(_esFxt.Indexer, new SingleIndexNameProvider(_esIndexName));

            _api = apiFxt.StartWithProxy(srv =>
            {
                srv.Configure<IndexerOptions>(opt =>
                    {
                        opt.ResourcePath = Path.Combine(Directory.GetCurrentDirectory(), "resources");
                        opt.Indexes = new[]
                        {
                            new IndexOptions
                            {
                                Id = "foo-index",
                                EsIndex = _esIndexName
                            }
                        };
                    })
                    .ConfigureEsTools(opt => opt.Url = TestTools.EsUrl)
                    .AddLogging(l => l
                        .ClearProviders()
                        .AddFilter(f => true)
                        .AddXUnit(output)
                    );
            });
        }

        [Fact]
        public async Task ShouldDelete()
        {
            //Arrange
            var docForDelete = TestDoc.Generate();

            await _indexer.CreateAsync(docForDelete);

            //Act
            await _api.DeleteAsync("foo-index", docForDelete.Id.ToString());
            await Task.Delay(500);

            var found = await _esFxt.Searcher.SearchAsync(_esIndexName, 
                new EsSearchParams<TestDoc>(q => 
                        q.Ids(idd => idd.Values(docForDelete.Id))
                    )
                );

            //Assert
            Assert.Empty(found);
        }

        [Fact]
        public async Task ShouldPost()
        {
            //Arrange
            var docForPost = TestDoc.Generate();

            //Act
            await _api.PostAsync("foo-index", JObject.FromObject(docForPost));
            await Task.Delay(500);

            var found = await _esFxt.Searcher.SearchAsync(_esIndexName,
                new EsSearchParams<TestDoc>(q =>
                    q.Ids(idd => idd.Values(docForPost.Id))
                )
            );

            //Assert
            Assert.Empty(found);
        }

        [Fact]
        public async Task ShouldPutCreate()
        {
            //Arrange
            var docForPut = TestDoc.Generate();

            //Act
            await _api.PutAsync("foo-index", JObject.FromObject(docForPut));
            await Task.Delay(500);

            var found = await _esFxt.Searcher.SearchAsync(_esIndexName,
                new EsSearchParams<TestDoc>(q =>
                    q.Ids(idd => idd.Values(docForPut.Id))
                )
            );

            //Assert
            Assert.Single(found);
            Assert.Equal(docForPut.Content, found[0].Content);
        }

        [Fact]
        public async Task ShouldPutUpdate()
        {
            //Arrange
            var docForPost = TestDoc.Generate();
            var docForPut = TestDoc.Generate(docForPost.Id);

            //Act
            await _api.PostAsync("foo-index", JObject.FromObject(docForPost));
            await Task.Delay(500);
            await _api.PutAsync("foo-index", JObject.FromObject(docForPut));
            await Task.Delay(500);

            var found = await _esFxt.Searcher.SearchAsync(_esIndexName,
                new EsSearchParams<TestDoc>(q =>
                    q.Ids(idd => idd.Values(docForPut.Id))
                )
            );

            //Assert
            Assert.Single(found);
            Assert.Equal(docForPut.Content, found[0].Content);
        }

        [Fact]
        public async Task ShouldPatch()
        {
            //Arrange
            var docForPost = TestDoc.Generate();
            var docForPatch = TestDoc.Generate(docForPost.Id);

            //Act
            await _api.PostAsync("foo-index", JObject.FromObject(docForPost));
            await Task.Delay(500);
            await _api.PutAsync("foo-index", JObject.FromObject(docForPatch));
            await Task.Delay(500);

            var found = await _esFxt.Searcher.SearchAsync(_esIndexName,
                new EsSearchParams<TestDoc>(q =>
                    q.Ids(idd => idd.Values(docForPatch.Id))
                )
            );

            //Assert
            Assert.Single(found);
            Assert.Equal(docForPatch.Content, found[0].Content);
        }
    }
}
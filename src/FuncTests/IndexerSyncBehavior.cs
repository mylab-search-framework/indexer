using System;
using System.IO;
using System.Threading.Tasks;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.ApiClient.Test;
using MyLab.DbTest;
using MyLab.Search.EsAdapter;
using MyLab.Search.EsAdapter.Search;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using MyLab.Search.IndexerClient;
using Xunit;
using Xunit.Abstractions;

namespace FuncTests
{
    public class IndexerSyncBehavior : 
        IClassFixture<TmpDbFixture<TestDbInitializer>>, 
        IClassFixture<EsFixture<TestEsFixtureStrategy>>, 
        IClassFixture<TestApi<Startup, IIndexerSyncTaskApi>>
    {
        private readonly TmpDbFixture<TestDbInitializer> _dbFxt;
        private readonly TestApi<Startup, IIndexerSyncTaskApi> _apiFxt;
        private readonly ITestOutputHelper _output;
        private readonly EsFixture<TestEsFixtureStrategy> _esFxt;

        public IndexerSyncBehavior(
            TmpDbFixture<TestDbInitializer> dbFxt,
            EsFixture<TestEsFixtureStrategy> esFxt,
            TestApi<Startup, IIndexerSyncTaskApi> apiFxtFxt,
            ITestOutputHelper output)
        {
            _esFxt = esFxt;
            _esFxt.Output = output;
            _dbFxt = dbFxt;
            _dbFxt.Output = output;
            _apiFxt = apiFxtFxt;
            _output = output;
            _apiFxt.Output = output;
        }

        [Fact]
        public async Task ShouldSync()
        {
            //Arrange
            var docId = 10;
            var testDt = DateTime.Now;

            var testDocOld = new TestDoc
            {
                Id = 50,
                Content = "foo",
                LastChanged = testDt.AddMinutes(-3)
            };

            var testDoc = new TestDoc
            {
                Id = docId,
                Content = "bar",
                LastChanged = testDt.AddMinutes(-1)
            };

            var dbMgr = await _dbFxt.CreateDbAsync();
            await dbMgr.DoOnce().InsertAsync(testDocOld);
            await dbMgr.DoOnce().InsertAsync(testDoc);

            var seedService = new TestSeedService(testDt.AddMinutes(-2));

            var api = _apiFxt.StartWithProxy(srv =>
            {
                srv.AddSingleton(dbMgr)
                    .Configure<IndexerOptions>(opt =>
                    {
                        opt.ResourcePath = Path.Combine(Directory.GetCurrentDirectory(), "resources");
                        opt.Indexes = new[]
                        {
                            new IndexOptions
                            {
                                Id = "baz",
                                EsIndex = "es-baz"
                            }
                        };
                    })
                    .ConfigureEsTools(opt => opt.Url = TestTools.EsUrl)
                    .AddLogging(l => l
                        .ClearProviders()
                        .AddFilter(f => true)
                        .AddXUnit(_output)
                    )
                    .AddSingleton<ISeedService>(seedService);
            });

            var searchP = new EsSearchParams<TestDoc>(q => q.Ids(idd => idd.Values(docId)));

            //Act
            await api.StartSynchronizationAsync();
            await Task.Delay(2000);

            var found = await _esFxt.Searcher.SearchAsync("es-baz", searchP);

            //Assert
            Assert.Single(found);
            Assert.Equal(docId, found[0].Id);
            Assert.Equal("bar", found[0].Content);
            Assert.True(seedService.DtSeed > testDt);
        }
    }
}
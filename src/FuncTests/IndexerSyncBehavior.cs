using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.ApiClient.Test;
using MyLab.Db;
using MyLab.DbTest;
using MyLab.Log.XUnit;
using MyLab.Search.EsAdapter;
using MyLab.Search.EsAdapter.Search;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using MyLab.Search.IndexerClient;
using MySql.Data.MySqlClient;
using Xunit;
using Xunit.Abstractions;

namespace FuncTests
{
    public class IndexerSyncBehavior : 
        IClassFixture<EsFixture<TestEsFixtureStrategy>>, 
        IClassFixture<TestApi<Startup, IIndexerSyncTaskApi>>,
        IAsyncLifetime
    {
        const string EsUrl = "http://localhost:9200";

        private readonly TestApi<Startup, IIndexerSyncTaskApi> _apiFxt;
        private readonly ITestOutputHelper _output;
        private readonly EsFixture<TestEsFixtureStrategy> _esFxt;
        private readonly IDbManager _dbMgr;
        private readonly IConfigurationRoot _dbConfig;

        public IndexerSyncBehavior(
            EsFixture<TestEsFixtureStrategy> esFxt,
            TestApi<Startup, IIndexerSyncTaskApi> apiFxtFxt,
            ITestOutputHelper output)
        {
            _esFxt = esFxt;
            _esFxt.Output = output;
            _apiFxt = apiFxtFxt;
            _output = output;
            _apiFxt.Output = output;

            var memConfig = new MemoryConfigurationSource
            {
                InitialData = new KeyValuePair<string, string>[]
                {
                    new("DB", "Server=localhost; Database=test; Uid=root; Pwd=root-pass;")
                }
            };

            _dbConfig = new ConfigurationBuilder()
                .Add(memConfig)
                .Build();
            var services = new ServiceCollection()
                .AddDbTools(_dbConfig, new MySqlDataProvider(ProviderName.MySql))
                .AddLogging(l => l
                    .ClearProviders()
                    .AddFilter(f => true)
                    .AddXUnit(_output)
                )
                .BuildServiceProvider();
            
            _dbMgr = services.GetRequiredService<IDbManager>();
            using var dc = _dbMgr.Use();

            try
            {
                dc.CreateTable<TestDoc>();
            }
            catch (MySqlException e) when (e.Message.EndsWith("already exists"))
            {
                //do nothing
            }

            dc.Tab<TestDoc>().Delete();

        }

        [Fact]
        public async Task ShouldSyncHeap()
        {
            //Arrange
            var testDt = DateTime.Now;

            var testDocOld = TestDoc.Generate();
            testDocOld.LastChanged = testDt.AddMinutes(-3);

            var testDoc = TestDoc.Generate();
            testDoc.LastChanged = testDt.AddMinutes(-1);
            
            await _dbMgr.DoOnce().InsertAsync(testDocOld);
            await _dbMgr.DoOnce().InsertAsync(testDoc);

            var seedService = new TestSeedService
            {
                Seed = new Seed(testDt.AddMinutes(-2))
            };

            var api = _apiFxt.StartWithProxy(srv =>
            {
                srv.AddSingleton(_dbMgr)
                    .Configure<IndexerOptions>(opt =>
                    {
                        opt.ResourcesPath = Path.Combine(Directory.GetCurrentDirectory(), "resources");
                        opt.Indexes = new[]
                        {
                            new IndexOptions
                            {
                                Id = "baz"
                            }
                        };
                        opt.EnableEsIndexAutoCreation = true;
                    })
                    .AddDbTools(_dbConfig, new MySqlDataProvider(ProviderName.MySql))
                    .ConfigureEsTools(opt => opt.Url = EsUrl)
                    .AddLogging(l => l
                        .ClearProviders()
                        .AddFilter(f => true)
                        .AddXUnit(_output)
                    )
                    .AddSingleton<ISeedService>(seedService);
            });

            var searchP = new EsSearchParams<TestDoc>(q => q.Ids(idd => idd.Values(testDoc.Id)));

            //Act
            await Task.Delay(500);
            await api.StartSynchronizationAsync();
            await Task.Delay(2000);

            var found = await _esFxt.Searcher.SearchAsync("baz", searchP);

            //Assert
            Assert.Single(found);
            Assert.Equal(testDoc.Id, found[0].Id);
            Assert.Equal(testDoc.Content, found[0].Content);
            Assert.True(seedService.Seed.IsDateTime);
            Assert.True(seedService.Seed.DateTime > testDt);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            var exists = await _esFxt.Tools.Index("baz").ExistsAsync();
            if(exists) await _esFxt.Tools.Index("baz").DeleteAsync();
        }

        public class TestDbInitializer : ITestDbInitializer
        {
            public async Task InitializeAsync(DataConnection dc)
            {
                await dc.CreateTableAsync<TestDoc>();
            }
        }

        class TestSeedService : ISeedService
        {
            public Seed Seed { get; set; }
            
            public Task SaveSeedAsync(string indexId, Seed seed)
            {
                Seed = seed;
                return Task.CompletedTask;
            }

            public Task<Seed> LoadSeedAsync(string indexId)
            {
                return Task.FromResult(Seed);
            }
        }
    }
}
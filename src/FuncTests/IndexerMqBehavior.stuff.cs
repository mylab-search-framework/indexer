using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
using Xunit;
using Xunit.Abstractions;

namespace FuncTests
{
    public partial class IndexerMqBehavior :
        IClassFixture<EsFixture<TestEsFixtureStrategy>>,
        IClassFixture<TmpDbFixture<TestDbInitializer>>,
        IClassFixture<TestApi<Startup, IApiKickerContract>>, 
        IAsyncLifetime
    {
        private RabbitQueue _queue;
        private EsIndexer<TestDoc> _indexer;
        private EsSearcher<TestDoc> _searcher;
        private readonly TmpDbFixture<TestDbInitializer> _dbFxt;
        private readonly ITestOutputHelper _output;
        private readonly EsFixture<TestEsFixtureStrategy> _esFxt;
        private readonly TestApi<Startup, IApiKickerContract> _testApi;
        private IApiKickerContract _kickApi;
        private IDbManager _dbMgr;
        private string _esIndexName;

        public IndexerMqBehavior(
            TestApi<Startup, IApiKickerContract> testApi,
            TmpDbFixture<TestDbInitializer> dbFxt,
            EsFixture<TestEsFixtureStrategy> esFxt,
            ITestOutputHelper output)
        {
            _testApi = testApi;
            _testApi.Output = output;

            _dbFxt = dbFxt;
            _output = output;
            _dbFxt.Output = output;

            _esFxt = esFxt;
            _esFxt.Output = output;
        }

        private Task<EsFound<TestDoc>> SearchByIdAsync(int id)
        {
            return _searcher.SearchAsync(new EsSearchParams<TestDoc>(q => q.Ids(idd => idd.Values(id))));
        }

        public async Task InitializeAsync()
        {
            _dbMgr = await _dbFxt.CreateDbAsync();

            _esIndexName = Guid.NewGuid().ToString("N");

            await _esFxt.IndexTools.CreateIndexAsync(_esIndexName);

            var indexNameProvider = new SingleIndexNameProvider(_esIndexName);

            _indexer = new EsIndexer<TestDoc>(_esFxt.Indexer, indexNameProvider);
            _searcher = new EsSearcher<TestDoc>(_esFxt.Searcher, indexNameProvider);

            var queueFactory = new RabbitQueueFactory(TestTools.RabbitChannelProvider);
            _queue = queueFactory.CreateWithRandomId();

            _kickApi = _testApi.StartWithProxy(srv => 
                srv.Configure<IndexerOptions>(opt =>
                {
                    opt.ResourcePath = Path.Combine(Directory.GetCurrentDirectory(), "resources");
                    opt.Indexes = new[]
                    {
                        new IndexOptions
                        {
                            Id = "baz",
                            EsIndex = _esIndexName,
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
                .AddSingleton(_dbMgr)
            );
        }

        public async Task DisposeAsync()
        {
            _queue.Remove();
            await _esFxt.IndexTools.DeleteIndexAsync(_esIndexName);
        }
    }
}
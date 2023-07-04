using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MyLab.ApiClient.Test;
using MyLab.Db;
using MyLab.DbTest;
using MyLab.Log.XUnit;
using MyLab.RabbitClient.Model;
using MyLab.Search.EsAdapter;
using MyLab.Search.EsAdapter.Indexing;
using MyLab.Search.EsAdapter.Search;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using Nest;
using Xunit;
using Xunit.Abstractions;
using IndexOptions = MyLab.Search.Indexer.Options.IndexOptions;

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
        private readonly string _testIndexName;

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

            _testIndexName = TestTools.CreateTestName<IndexerMqBehavior>();
        }

        private Task<EsFound<TestDoc>> SearchByIdAsync(string id)
        {
            return _searcher.SearchAsync(new EsSearchParams<TestDoc>(q => q.Ids(idd => idd.Values(id))));
        }

        public async Task InitializeAsync()
        {
            _dbMgr = await _dbFxt.CreateDbAsync();

            await TryDeleteIndex();
            await _esFxt.Tools.Index(_testIndexName).CreateAsync();

            var indexNameProvider = new SingleIndexNameProvider(_testIndexName);

            _indexer = new EsIndexer<TestDoc>(_esFxt.Indexer, indexNameProvider);
            _searcher = new EsSearcher<TestDoc>(_esFxt.Searcher, indexNameProvider);

            var queueFactory = new RabbitQueueFactory(TestTools.RabbitChannelProvider);
            _queue = queueFactory.CreateWithRandomId();

            _kickApi = _testApi.StartWithProxy(srv => 
                srv.Configure<IndexerOptions>(opt =>
                {
                    opt.ResourcesPath = Path.Combine(Directory.GetCurrentDirectory(), "resources");
                    opt.Indexes = new[]
                    {
                        new IndexOptions
                        {
                            Id = _testIndexName
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
                .AddSingleton(TestTools.CreateResourceProviderWrapper(_testIndexName, "baz"))
            );
        }

        public async Task DisposeAsync()
        {
            _queue.Remove();
            await TryDeleteIndex();
        }

        Task TryDeleteIndex()
        {
            return TestTools.RemoveTargetFromAliasAsync(_esFxt.Tools, _testIndexName);
        }
    }
}
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.ApiClient.Test;
using MyLab.Db;
using MyLab.DbTest;
using MyLab.Log.XUnit;
using MyLab.Search.EsAdapter;
using MyLab.Search.EsAdapter.Indexing;
using MyLab.Search.EsAdapter.Inter;
using MyLab.Search.EsAdapter.Search;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.Options;
using MyLab.Search.IndexerClient;
using Xunit;
using Xunit.Abstractions;

namespace FuncTests
{
    public partial class IndexerApiBehavior :
        IClassFixture<EsFixture<TestEsFixtureStrategy>>,
        IClassFixture<TmpDbFixture<TestDbInitializer>>,
        IClassFixture<TestApi<Startup, IIndexerV2Api>>,
        IAsyncLifetime
    {
        private IIndexerV2Api _api;
        private EsIndexer<TestDoc> _indexer;
        private EsSearcher<TestDoc> _searcher;
        private readonly TmpDbFixture<TestDbInitializer> _dbFxt;
        private readonly TestApi<Startup, IIndexerV2Api> _apiFxt;
        private readonly ITestOutputHelper _output;
        private readonly EsFixture<TestEsFixtureStrategy> _esFxt;
        private IDbManager _dbMgr;
        private string _esIndexName;

        public IndexerApiBehavior(
            TmpDbFixture<TestDbInitializer> dbFxt,
            EsFixture<TestEsFixtureStrategy> esFxt,
            TestApi<Startup, IIndexerV2Api> apiFxt,
            ITestOutputHelper output)
        {
            _output = output;

            _dbFxt = dbFxt;
            _dbFxt.Output = output;

            _esFxt = esFxt;
            esFxt.Output = output;

            _apiFxt = apiFxt;
            _apiFxt.Output = output;
        }

        Task<EsFound<TestDoc>> SearchByIdAsync(string id)
        {
            return _searcher.SearchAsync(new EsSearchParams<TestDoc>(q => q.Ids(idd => idd.Values(id))));
        }

        public async Task InitializeAsync()
        {
            _dbMgr = await _dbFxt.CreateDbAsync();

            _esIndexName = Guid.NewGuid().ToString("N");

            //await _esFxt.Tools.Index(_esIndexName).CreateAsync();

            var indexNameProvider = new SingleIndexNameProvider(_esIndexName);

            _indexer = new EsIndexer<TestDoc>(_esFxt.Indexer, indexNameProvider);
            _searcher = new EsSearcher<TestDoc>(_esFxt.Searcher, indexNameProvider);

            _api = _apiFxt.StartWithProxy(srv =>
            {
                srv.Configure<IndexerOptions>(opt =>
                    {
                        opt.ResourcesPath = Path.Combine(Directory.GetCurrentDirectory(), "resources");
                        opt.Indexes = new[]
                        {
                            new IndexOptions
                            {
                                Id = "baz",
                                EsIndex = _esIndexName
                            }
                        };
                        opt.EnableEsIndexAutoCreation = true;
                    })
                    .ConfigureEsTools(opt => { opt.Url = TestTools.EsUrl; })
                    .AddLogging(l => l
                        .ClearProviders()
                        .AddFilter(f => true)
                        .AddXUnit(_output)
                    )
                    .AddSingleton(_dbMgr);
            });
        }

        public async Task DisposeAsync()
        {
            try
            {
                await _esFxt.Tools.Index(_esIndexName).DeleteAsync();
            }
            catch (EsException e) when (e.Response.HasIndexNotFound)
            {
            }
        }
    }
}
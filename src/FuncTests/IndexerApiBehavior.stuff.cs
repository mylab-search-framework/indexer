using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.ApiClient.Test;
using MyLab.Search.EsAdapter;
using MyLab.Search.EsAdapter.Indexing;
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
        IClassFixture<TestApi<Startup, IIndexerV2Api>>
    {
        private readonly IIndexerV2Api _api;
        private readonly EsIndexer<TestDoc> _indexer;
        private readonly EsSearcher<TestDoc> _searcher;

        public IndexerApiBehavior(
            EsFixture<TestEsFixtureStrategy> esFxt,
            TestApi<Startup, IIndexerV2Api> apiFxt,
            ITestOutputHelper output)
        {
            esFxt.Output = output;
            apiFxt.Output = output;

            var esIndexName = Guid.NewGuid().ToString("N");

            var indexNameProvider = new SingleIndexNameProvider(esIndexName);

            _indexer = new EsIndexer<TestDoc>(esFxt.Indexer, indexNameProvider);
            _searcher = new EsSearcher<TestDoc>(esFxt.Searcher, indexNameProvider);

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
                                EsIndex = esIndexName
                            }
                        };
                    })
                    .ConfigureEsTools(opt => { opt.Url = TestTools.EsUrl; })
                    .AddLogging(l => l
                        .ClearProviders()
                        .AddFilter(f => true)
                        .AddXUnit(output)
                    );
            });
        }

        Task<EsFound<TestDoc>> SearchByIdAsync(int id)
        {
            return _searcher.SearchAsync(new EsSearchParams<TestDoc>(q => q.Ids(idd => idd.Values(id))));
        }
    }
}
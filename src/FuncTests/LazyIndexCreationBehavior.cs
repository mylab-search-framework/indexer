using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.ApiClient.Test;
using MyLab.Search.EsAdapter;
using MyLab.Search.EsAdapter.Indexing;
using MyLab.Search.EsAdapter.Inter;
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
    public class LazyIndexCreationBehavior :
        IClassFixture<EsFixture<TestEsFixtureStrategy>>,
        IClassFixture<TestApi<Startup, IIndexerV2Api>>,
        IAsyncLifetime
    {
        private readonly ITestOutputHelper _output;
        private readonly EsFixture<TestEsFixtureStrategy> _esFxt;
        private readonly TestApi<Startup, IIndexerV2Api> _apiFxt;
        private readonly string _esIndexName;

        public LazyIndexCreationBehavior(
            EsFixture<TestEsFixtureStrategy> esFxt,
            TestApi<Startup, IIndexerV2Api> apiFxt,
            ITestOutputHelper output)
        {
            _output = output;
            
            _esFxt = esFxt;
            esFxt.Output = output;

            _apiFxt = apiFxt;
            _apiFxt.Output = output;

            _esIndexName = Guid.NewGuid().ToString("N");

            _apiFxt.ServiceOverrider = srv =>
            {
                srv.Configure<IndexerOptions>(opt =>
                    {
                        opt.ResourcePath = Path.Combine(Directory.GetCurrentDirectory(), "resources");
                    })
                    .ConfigureEsTools(opt => { opt.Url = TestTools.EsUrl; })
                    .AddLogging(l => l
                        .ClearProviders()
                        .AddFilter(f => true)
                        .AddXUnit(_output)
                    );
            };
        }

        [Fact]
        public async Task ShouldCreateIndexWhenNotFound()
        {
            //Arrange
            var indexesOptions = Array.Empty<IndexOptions>();

            var api = _apiFxt.StartWithProxy(srv => srv.Configure<IndexerOptions>(
                opt =>
                {
                    opt.Indexes = indexesOptions;
                })
            );

            var docForPost = TestDoc.Generate();

            //Act
            await api.PostAsync(_esIndexName, JObject.FromObject(docForPost));
            await Task.Delay(1000);

            var isIndexExists = await _esFxt.IndexTools.IsIndexExistsAsync(_esIndexName);

            //Assert
            Assert.True(isIndexExists);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            try
            {
                await _esFxt.IndexTools.DeleteIndexAsync(_esIndexName);
            }
            catch (EsException e) when (e.HasIndexNotFound())
            {
            }
        }
    }
}

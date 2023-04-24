using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.ApiClient.Test;
using MyLab.Search.EsAdapter;
using MyLab.Search.EsAdapter.Inter;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.Options;
using Xunit;
using Xunit.Abstractions;

namespace FuncTests
{
    public class StartupIndexCreationBehavior : IAsyncLifetime, 
        IClassFixture<TestApi<Startup, IApiKickerContract>>,
        IClassFixture<EsFixture<TestEsFixtureStrategy>>
    {
        private readonly TestApi<Startup, IApiKickerContract> _kickerApi;
        private readonly EsFixture<TestEsFixtureStrategy> _esFxt;
        private readonly string _esIndexName;

        public StartupIndexCreationBehavior(
            TestApi<Startup, IApiKickerContract> kickerApi,
            EsFixture<TestEsFixtureStrategy> esFxt,
            ITestOutputHelper output)
        {
            _kickerApi = kickerApi;
            _kickerApi.Output = output;

            _esFxt = esFxt;
            _esFxt.Output = output;

            _kickerApi.ServiceOverrider = srv =>
            {
                srv.Configure<IndexerOptions>(opt =>
                    {
                        opt.ResourcePath = Path.Combine(Directory.GetCurrentDirectory(), "resources");
                    })
                    .ConfigureEsTools(opt => { opt.Url = TestTools.EsUrl; })
                    .AddLogging(l => l
                        .ClearProviders()
                        .AddFilter(f => true)
                        .AddXUnit(output)
                    );
            };

            _esIndexName = Guid.NewGuid().ToString("N");
        }

        [Fact]
        public async Task ShouldCreateIndexAtStartup()
        {
            //Arrange
            var indexesOptions = new []
            {
                new IndexOptions
                {
                    Id = "baz",
                    EsIndex = _esIndexName,
                    IdPropertyType = IdPropertyType.Int
                }
            };

            var api = _kickerApi.StartWithProxy(srv => srv.Configure<IndexerOptions>(
                opt =>
                {
                    opt.Indexes = indexesOptions;
                })
            );

            //Act
            await api.KickAsync();

            await Task.Delay(1000);

            var isIndexExists = await _esFxt.IndexTools.IsIndexExistentAsync(_esIndexName);

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
            catch (EsException e) when (e.Response.HasIndexNotFound)
            {
            }
        }
    }
}

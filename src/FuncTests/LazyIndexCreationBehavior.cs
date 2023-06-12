using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.ApiClient.Test;
using MyLab.Log.XUnit;
using MyLab.Search.EsAdapter;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services.ComponentUploading;
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
                        opt.AppId = "test";
                        opt.ResourcesPath = Path.Combine(Directory.GetCurrentDirectory(), "resources");
                        opt.EnableEsIndexAutoCreation = true;
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

            var newDoc = TestDoc.Generate();

            //Act
            await api.PutAsync(_esIndexName, JObject.FromObject(newDoc));
            await Task.Delay(1000);

            var isIndexExists = await _esFxt.Tools.Index(_esIndexName).ExistsAsync();

            //Assert
            Assert.True(isIndexExists);
        }

        [Fact]
        public async Task ShouldSetupMappingMetaWhenCreateIndex()
        {
            //Arrange
            var api = _apiFxt.StartWithProxy();

            var newDoc = TestDoc.Generate();

            //Act
            await api.PutAsync(_esIndexName, JObject.FromObject(newDoc));
            await Task.Delay(1000);

            var indexInfo= await _esFxt.Tools.Index(_esIndexName).TryGetAsync(CancellationToken.None);

            var metaDict = indexInfo?.Mappings?.Meta;

            MappingMetadata.TryGet(metaDict, out var metaObj);

            //Assert
            Assert.NotNull(indexInfo);
            Assert.NotNull(indexInfo.Mappings);
            Assert.NotNull(metaObj);
            Assert.Null(metaObj.Template);
            Assert.Equal("test", metaObj.Creator.Owner);
            Assert.Equal("ab32e71ac91c75ecdca810cfa0bb8196", metaObj.Creator.SourceHash);
            
        }

        [Fact]
        public async Task ShouldCreateStreamWhenNotFound()
        {
            //Arrange
            var indexesOptions = Array.Empty<IndexOptions>();

            var api = _apiFxt.StartWithProxy(srv => srv.Configure<IndexerOptions>(
                opt =>
                {
                    opt.DefaultIndexOptions.IndexType = IndexType.Stream;
                    opt.Indexes = indexesOptions;
                })
            );

            var newDoc = TestDoc.Generate();

            //Act
            await api.PutAsync(_esIndexName, JObject.FromObject(newDoc));
            await Task.Delay(1000);

            var isStreamExists = await _esFxt.Tools.Stream(_esIndexName).ExistsAsync();

            //Assert
            Assert.True(isStreamExists);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            var indexExists = await _esFxt.Tools.Index(_esIndexName).ExistsAsync();
            if(indexExists) await _esFxt.Tools.Index(_esIndexName).DeleteAsync();

            var streamExists = await _esFxt.Tools.Stream(_esIndexName).ExistsAsync();
            if(streamExists) await _esFxt.Tools.Stream(_esIndexName).DeleteAsync();
        }
    }
}

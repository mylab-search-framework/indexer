using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MyLab.ApiClient.Test;
using MyLab.Log.XUnit;
using MyLab.Search.EsAdapter;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Services.ComponentUploading;
using MyLab.Search.IndexerClient;
using Nest;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;
using IndexOptions = MyLab.Search.Indexer.Options.IndexOptions;

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
                        opt.EnableAutoCreation = true;
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

            var indexCreatorMock = new Mock<IIndexCreator>();
            CreatedIndexDescription justCreatedIndexDesc = null;

            var api = _apiFxt.StartWithProxy(srv => srv
                .Configure<IndexerOptions>(
                    opt =>
                    {
                        opt.Indexes = indexesOptions;
                    })
                .AddSingleton(sp =>
                {
                    var originCreator = ActivatorUtilities.CreateInstance<IndexCreator>(sp);
                    indexCreatorMock
                        .Setup(ic => ic.CreateIndexAsync(It.Is<string>(name => name == _esIndexName), It.IsAny<CancellationToken>()))
                        .Returns<string, CancellationToken>(async (idxId, ct) =>
                        {
                            return justCreatedIndexDesc = await originCreator.CreateIndexAsync(idxId, ct);
                        });
                    return indexCreatorMock.Object;
                })
            );

            var newDoc = TestDoc.Generate();
            bool isIndexExists = false;

            //Act
            await api.PutAsync(_esIndexName, JObject.FromObject(newDoc));
            await Task.Delay(1000);

            if (justCreatedIndexDesc != null)
            {
                isIndexExists = await _esFxt.Tools.Index(justCreatedIndexDesc.Name).ExistsAsync();
            }
            
            //Assert
            Assert.NotNull(justCreatedIndexDesc);
            Assert.True(isIndexExists);
        }

        [Fact]
        public async Task ShouldSetupMappingMetaWhenCreateIndex()
        {
            //Arrange
            var indexCreatorMock = new Mock<IIndexCreator>();
            CreatedIndexDescription justCreatedIndexDesc = null;

            var api = _apiFxt.StartWithProxy(srv => srv
                .AddSingleton(sp =>
                {
                    var originCreator = ActivatorUtilities.CreateInstance<IndexCreator>(sp);
                    indexCreatorMock
                        .Setup(ic => ic.CreateIndexAsync(It.Is<string>(name => name == _esIndexName), It.IsAny<CancellationToken>()))
                        .Returns<string, CancellationToken>(async (idxId, ct) =>
                        {
                            return justCreatedIndexDesc = await originCreator.CreateIndexAsync(idxId, ct);
                        });
                    return indexCreatorMock.Object;
                }));

            var newDoc = TestDoc.Generate();

            IndexState indexInfo = null;

            //Act
            await api.PutAsync(_esIndexName, JObject.FromObject(newDoc));
            await Task.Delay(1000);

            if (justCreatedIndexDesc != null)
            {
                indexInfo = await _esFxt.Tools.Index(justCreatedIndexDesc.Name).TryGetAsync(CancellationToken.None);
            }

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

            var indexCreatorMock = new Mock<IIndexCreator>();
            CreatedIndexDescription justCreatedIndexDesc = null;

            var api = _apiFxt.StartWithProxy(srv => srv.Configure<IndexerOptions>(
                opt =>
                {
                    opt.DefaultIndex.IsStream = true;
                    opt.Indexes = indexesOptions;
                })
                .AddSingleton(sp =>
                {
                    var originCreator = ActivatorUtilities.CreateInstance<IndexCreator>(sp);
                    indexCreatorMock
                        .Setup(ic => ic.CreateIndexAsync(It.Is<string>(name => name == _esIndexName), It.IsAny<CancellationToken>()))
                        .Returns<string, CancellationToken>(async (idxId, ct) =>
                        {
                            return justCreatedIndexDesc = await originCreator.CreateIndexAsync(idxId, ct);
                        });
                    return indexCreatorMock.Object;
                })
            );

            var newDoc = TestDoc.Generate();
            bool isStreamExists = false;

            //Act
            await api.PutAsync(_esIndexName, JObject.FromObject(newDoc));
            await Task.Delay(1000);

            if (justCreatedIndexDesc != null)
            {
                isStreamExists = await _esFxt.Tools.Stream(justCreatedIndexDesc.Name).ExistsAsync();
            }

            //Assert
            Assert.True(isStreamExists);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            return TestTools.RemoveTargetFromAliasAsync(_esFxt.Tools, _esIndexName);
        }
    }
}

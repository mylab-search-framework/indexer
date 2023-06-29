using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MyLab.Log.XUnit;
using MyLab.Search.EsAdapter.Tools;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services.ComponentUploading;
using Xunit;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace IntegrationTests
{
    public partial class IndexTemplateUploaderBehavior : IClassFixture<EsFixture<TestEsFixtureStrategy>>, IAsyncLifetime
    {
        [Fact]
        public async Task ShouldSaveMappingMetadataWhenUpload()
        {
            //Arrange
            var newTemplate = CreateTemplate("foo", "1", "hash");
            var resourceProvider = CreateResourceProvider("index-template-test", newTemplate);

            var services = new ServiceCollection()
                .AddLogging(l => l
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddXUnit(_output)
                    )
                .AddSingleton(_fxt.Tools)
                .AddSingleton(resourceProvider)
                .Configure<IndexerOptions>(o => o.AppId = "foo")
                .BuildServiceProvider();

            var uploader = ActivatorUtilities.CreateInstance<IndexTemplateUploader>(services);
            
            MappingMetadata mappingMetadata = null;

            //Act
            await uploader.UploadAsync(CancellationToken.None);

            var templateInfo = await _fxt.Tools.IndexTemplate("index-template-test").TryGetAsync();

            if (templateInfo != null)
            {
                MappingMetadata.TryGet(templateInfo.Template?.Mappings?.Meta, out mappingMetadata);
            }

            //Assert

            Assert.NotNull(mappingMetadata);
            Assert.Equal("foo", mappingMetadata.Template.Owner);
            Assert.Equal("index-template-test", mappingMetadata.Template.SourceName);
        }

        [Fact]
        public async Task ShouldUploadIfDoesNotExists()
        {
            //Arrange
            var newTemplate = CreateTemplate("foo", "1", "hash");
            var resourceProvider = CreateResourceProvider("index-template-test", newTemplate);

            var services = new ServiceCollection()
                .AddLogging(l => l
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddXUnit(_output)
                    )            
                .AddSingleton(_fxt.Tools)
                .AddSingleton(resourceProvider)
                .Configure<IndexerOptions>(o => o.AppId = "foo")
                .BuildServiceProvider();

            var uploader = ActivatorUtilities.CreateInstance<IndexTemplateUploader>(services);
            
            ComponentMetadata componentMetadata = null;
            string ver = null;

            //Act
            await uploader.UploadAsync(CancellationToken.None);
            
            var templateInfo = await _fxt.Tools.IndexTemplate("index-template-test").TryGetAsync();

            if (templateInfo != null)
            {
                ComponentMetadata.TryGet(templateInfo.Meta, out componentMetadata);
                ver = TestTools.GetComponentVer(templateInfo.Meta);
            }

            //Assert
            Assert.NotNull(componentMetadata);
            Assert.Equal("hash", componentMetadata.SourceHash);
            Assert.Equal("foo", componentMetadata.Owner);
            Assert.Equal("1", ver);
        }

        [Fact]
        public async Task ShouldUpdateIfExists()
        {
            //Arrange
            var originTemplate = CreateTemplatePutRequest("index-template-test", "foo", "1", "origin-hash");
            var newTemplate = CreateTemplate("foo", "2", "hash");
            var resourceProvider = CreateResourceProvider("index-template-test", newTemplate);

            var services = new ServiceCollection()
                .AddLogging(l => l
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddXUnit(_output)
                    )
                .AddSingleton(_fxt.Tools)
                .AddSingleton(resourceProvider)
                .Configure<IndexerOptions>(o => o.AppId = "foo")
                .BuildServiceProvider();

            var uploader = ActivatorUtilities.CreateInstance<IndexTemplateUploader>(services);

            string ver = null;
            ComponentMetadata componentMetadata = null;
            
            await _fxt.Tools.IndexTemplate("index-template-test").PutAsync(originTemplate);

            //Act
            await uploader.UploadAsync(CancellationToken.None);

            var templateInfo = await _fxt.Tools.IndexTemplate("index-template-test").TryGetAsync();

            if (templateInfo != null)
            {
                ComponentMetadata.TryGet(templateInfo.Meta, out componentMetadata);
                ver = TestTools.GetComponentVer(templateInfo.Meta);
            }

            //Assert
            Assert.NotNull(componentMetadata);
            Assert.Equal("hash", componentMetadata.SourceHash);
            Assert.Equal("foo", componentMetadata.Owner);
            Assert.Equal("2", ver);
        }

        [Fact]
        public async Task ShouldNotUpdateWithSameVersion()
        {
            //Arrange
            var originTemplate = CreateTemplatePutRequest("index-template-test", "foo", "1", "same-hash");
            var newTemplate = CreateTemplate("foo", "2", "same-hash");
            var resourceProvider = CreateResourceProvider("index-template-test", newTemplate);

            var indexTemplateToolMock = new Mock<IEsIndexTemplateTool>();
            indexTemplateToolMock.Setup(t => t.TryGetAsync(It.IsAny<CancellationToken>()))
                .Returns<CancellationToken>(ct =>
                    _fxt.Tools.IndexTemplate("index-template-test").TryGetAsync(ct));

            var toolsMock = new Mock<IEsTools>();
            toolsMock.Setup(m => m.IndexTemplate(It.IsAny<string>()))
                .Returns<string>(s => indexTemplateToolMock.Object);
            toolsMock.SetupGet(m => m.Serializer)
                .Returns(() => _fxt.Tools.Serializer);

            var services = new ServiceCollection()
                .AddLogging(l => l
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddXUnit(_output)
                    )
                .AddSingleton(toolsMock.Object)
                .AddSingleton(resourceProvider)
                .Configure<IndexerOptions>(o => o.AppId = "foo")
                .BuildServiceProvider();

            var uploader = ActivatorUtilities.CreateInstance<IndexTemplateUploader>(services);

            ComponentMetadata srvMeta = null;
            string ver = null;
            
            await _fxt.Tools.IndexTemplate("index-template-test").PutAsync(originTemplate);

            //Act
            await uploader.UploadAsync(CancellationToken.None);

            var templateInfo = await _fxt.Tools.IndexTemplate("index-template-test").TryGetAsync();

            if (templateInfo != null)
            {
                ComponentMetadata.TryGet(templateInfo.Meta, out srvMeta);
                ver = TestTools.GetComponentVer(templateInfo.Meta);
            }

            //Assert
            Assert.NotNull(srvMeta);
            Assert.Equal("same-hash", srvMeta.SourceHash);
            Assert.Equal("foo", srvMeta.Owner);
            Assert.Equal("1", ver);
            indexTemplateToolMock.Verify(t => t.PutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MyLab.Log.XUnit;
using MyLab.Search.EsAdapter.Tools;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Services.ResourceUploading;
using Nest;
using Xunit;
using Xunit.Abstractions;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace IntegrationTests
{
    public partial class ComponentTemplateUploaderBehavior : IClassFixture<EsFixture<TestEsFixtureStrategy>>, IAsyncLifetime
    {
        [Fact]
        public async Task ShouldUploadIfDoesNotExists()
        {
            //Arrange
            var newTemplate = CreateTemplate("foo", "1", "hash");
            var resourceProvider = CreateResourceProvider("component-template-test", newTemplate);

            var services = new ServiceCollection()
                .AddLogging(l => l
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddXUnit(_output)
                    )            
                .AddSingleton(_fxt.Tools)
                .AddSingleton(resourceProvider)
                .Configure<IndexerOptions>(o => o.AppId = "foo")
                .BuildServiceProvider();

            var uploader = ActivatorUtilities.CreateInstance<ComponentTemplateUploader>(services);

            ComponentMetadata componentMetadata = null;
            string ver = null;

            //Act
            await uploader.UploadAsync(CancellationToken.None);

            var templateInfo = await _fxt.Tools.ComponentTemplate("component-template-test").TryGetAsync();

            if (templateInfo != null)
            {
                ComponentMetadata.TryGet(templateInfo.Meta, out componentMetadata);
                ver = TestTools.GetComponentVer(templateInfo.Meta);
            }

            //Assert
            Assert.NotNull(componentMetadata);
            Assert.Equal("foo", componentMetadata.Owner);
            Assert.Equal("hash", componentMetadata.SourceHash);
            Assert.Equal("1", ver);
        }

        [Fact]
        public async Task ShouldUpdateIfExists()
        {
            //Arrange
            var originTemplate = CreateTemplatePutRequest("component-template-test","foo", "1", "origin-hash");
            var newTemplate = CreateTemplate("foo", "2", "hash");
            var resourceProvider = CreateResourceProvider("component-template-test", newTemplate);
            
            var services = new ServiceCollection()
                .AddLogging(l => l
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddXUnit(_output)
                    )
                .AddSingleton(_fxt.Tools)
                .AddSingleton(resourceProvider)
                .Configure<IndexerOptions>(o => o.AppId = "foo")
                .BuildServiceProvider();

            var uploader = ActivatorUtilities.CreateInstance<ComponentTemplateUploader>(services);

            string ver = null;
            ComponentMetadata componentMetadata = null;
            
            await _fxt.Tools.ComponentTemplate("component-template-test").PutAsync(originTemplate);

            //Act
            await uploader.UploadAsync(CancellationToken.None);

            var templateInfo = await _fxt.Tools.ComponentTemplate("component-template-test").TryGetAsync();

            if (templateInfo != null)
            {
                ComponentMetadata.TryGet(templateInfo.Meta, out componentMetadata);
                ver = TestTools.GetComponentVer(templateInfo.Meta);
            }

            //Assert
            Assert.NotNull(componentMetadata);
            Assert.Equal("foo", componentMetadata.Owner);
            Assert.Equal("hash", componentMetadata.SourceHash);
            Assert.Equal("2", ver);
        }

        [Fact]
        public async Task ShouldNotUpdateWithSameVersion()
        {
            //Arrange
            var originTemplate = CreateTemplatePutRequest("component-template-test", "foo", "1", "same-hash");
            var newTemplate = CreateTemplate("foo", "2", "same-hash");
            var resourceProvider = CreateResourceProvider("component-template-test", newTemplate);

            var componentTemplateToolMock = new Mock<IEsComponentTemplateTool>();
            componentTemplateToolMock.Setup(t => t.TryGetAsync(It.IsAny<CancellationToken>()))
                .Returns<CancellationToken>(ct =>
                    _fxt.Tools.ComponentTemplate("component-template-test").TryGetAsync(ct));

            var toolsMock = new Mock<IEsTools>();
            toolsMock.Setup(m => m.ComponentTemplate(It.IsAny<string>()))
                .Returns<string>(s => componentTemplateToolMock.Object);
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

            var uploader = ActivatorUtilities.CreateInstance<ComponentTemplateUploader>(services);

            ComponentMetadata componentMetadata = null;
            string ver = null;
            
            await _fxt.Tools.ComponentTemplate("component-template-test").PutAsync(originTemplate);

            //Act
            await uploader.UploadAsync(CancellationToken.None);

            var templateInfo = await _fxt.Tools.ComponentTemplate("component-template-test").TryGetAsync();

            if (templateInfo != null)
            {
                ComponentMetadata.TryGet(templateInfo.Meta, out componentMetadata);
                ver = TestTools.GetComponentVer(templateInfo.Meta);
            }

            //Assert
            Assert.NotNull(componentMetadata);
            Assert.Equal("same-hash", componentMetadata.SourceHash);
            Assert.Equal("foo", componentMetadata.Owner);
            Assert.Equal("1", ver);

            componentTemplateToolMock.Verify(t => t.PutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}

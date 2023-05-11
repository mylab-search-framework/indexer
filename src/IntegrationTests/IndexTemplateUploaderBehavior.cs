using System;
using System.Collections.Generic;
using System.IO;
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
using MyLab.Search.Indexer.Tools;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class IndexTemplateUploaderBehavior : IClassFixture<EsFixture<TestEsFixtureStrategy>>, IAsyncLifetime
    {
        private readonly EsFixture<TestEsFixtureStrategy> _fxt;
        private readonly ITestOutputHelper _output;
        private readonly string _indexerVer;

        public IndexTemplateUploaderBehavior(EsFixture<TestEsFixtureStrategy> fxt, ITestOutputHelper output)
        {
            _fxt = fxt;
            _output = output;
            fxt.Output = output;

            _indexerVer = typeof(IResourceUploader).Assembly.GetName().Version?.ToString();
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await _fxt.Tools.IndexTemplate("index-template-test").DeleteAsync();
        }

        [Fact]
        public async Task ShouldUploadIfDoesNotExists()
        {
            //Arrange
            var idxResProviderMock = new Mock<IResourceProvider>();
            
            var templateResource = new TestResource("index-template-test", "resources\\index-template-example.json");

            var resourceHash = await TestTools.GetResourceHashAsync(templateResource);

            idxResProviderMock.Setup(p => p.ProvideIndexTemplates())
                .Returns(() => new IResource[] { templateResource });
            
            var services = new ServiceCollection()
                .AddLogging(l => l
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddXUnit(_output)
                    )            
                .AddSingleton(_fxt.Tools)
                .AddSingleton(idxResProviderMock.Object)
                .BuildServiceProvider();

            var uploader = ActivatorUtilities.CreateInstance<IndexTemplateUploader>(services);
            
            string hash = null;
            string ver = null;

            //Act
            await uploader.UploadAsync(CancellationToken.None);

            var templateInfo = await _fxt.Tools.IndexTemplate("index-template-test").TryGetAsync();

            if (templateInfo != null)
            {
                ServiceMetadata.TryGetComponentHash(templateInfo.Meta, out hash);
                ver = TestTools.GetComponentVer(templateInfo.Meta);
            }

            //Assert
            Assert.Equal(resourceHash, hash);
            Assert.Equal("1", ver);
        }

        [Fact]
        public async Task ShouldUpdateIfExists()
        {
            //Arrange
            var idxResProviderMock = new Mock<IResourceProvider>();
            
            var templateResource = new TestResource("index-template-test", "resources\\index-template-example-2.json");

            var resourceHash = await TestTools.GetResourceHashAsync(templateResource);

            idxResProviderMock.Setup(p => p.ProvideIndexTemplates())
                .Returns(() => new IResource[] { templateResource });
            
            var services = new ServiceCollection()
                .AddLogging(l => l
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddXUnit(_output)
                    )
                .AddSingleton(_fxt.Tools)
                .AddSingleton(idxResProviderMock.Object)
                .BuildServiceProvider();

            var uploader = ActivatorUtilities.CreateInstance<IndexTemplateUploader>(services);

            string ver = null;
            string hash = null;

            var templateJson = await File.ReadAllTextAsync("resources\\existent-index-template.json");
            await _fxt.Tools.IndexTemplate("index-template-test").PutAsync(templateJson);

            //Act
            await uploader.UploadAsync(CancellationToken.None);

            var templateInfo = await _fxt.Tools.IndexTemplate("index-template-test").TryGetAsync();

            if (templateInfo != null)
            {
                ServiceMetadata.TryGetComponentHash(templateInfo.Meta, out hash);
                ver = TestTools.GetComponentVer(templateInfo.Meta);
            }

            //Assert
            Assert.Equal(resourceHash, hash);
            Assert.Equal("2", ver);
        }

        [Fact]
        public async Task ShouldNotUpdateWithSameVersion()
        {
            //Arrange
            var idxResProviderMock = new Mock<IResourceProvider>();

            var templateResource = new TestResource("index-template-test", "resources\\index-template-example.json");

            var resourceHash = await TestTools.GetResourceHashAsync(templateResource);

            idxResProviderMock.Setup(p => p.ProvideIndexTemplates())
                .Returns(() => new IResource[] { templateResource });

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
                .AddSingleton(idxResProviderMock.Object)
                .BuildServiceProvider();

            var uploader = ActivatorUtilities.CreateInstance<IndexTemplateUploader>(services);

            string hash = null;
            string ver = null;

            var templateJson = await File.ReadAllTextAsync("resources\\existent-index-template.json");
            await _fxt.Tools.IndexTemplate("index-template-test").PutAsync(templateJson);

            //Act
            await uploader.UploadAsync(CancellationToken.None);

            var templateInfo = await _fxt.Tools.IndexTemplate("index-template-test").TryGetAsync();

            if (templateInfo != null)
            {
                ServiceMetadata.TryGetComponentHash(templateInfo.Meta, out hash);
                ver = TestTools.GetComponentVer(templateInfo.Meta);
            }

            //Assert
            Assert.Equal(resourceHash, hash);
            Assert.Equal("1", ver);
            indexTemplateToolMock.Verify(t => t.PutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        class TestResource : IResource
        {
            private readonly string _filename;
            public string Name { get; }
            public Stream OpenRead()
            {
                return File.OpenRead(_filename);
            }

            public TestResource(string name, string filename)
            {
                Name = name;
                _filename = filename;
            }
        }
    }
}

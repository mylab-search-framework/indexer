using System;
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
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class IndicesUploaderBehavior : IClassFixture<EsFixture<TestEsFixtureStrategy>>, IAsyncLifetime
    {
        private static readonly string NamePrefix = nameof(IndicesUploaderBehavior).ToLower() + "-";

        private readonly EsFixture<TestEsFixtureStrategy> _fxt;
        private readonly ITestOutputHelper _output;

        public IndicesUploaderBehavior(EsFixture<TestEsFixtureStrategy> fxt, ITestOutputHelper output)
        {
            _fxt = fxt;
            _output = output;
            fxt.Output = output;
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            return _fxt.Tools.Index(NamePrefix + "*").DeleteAsync();
        }

        [Fact]
        public async Task ShouldUploadIndexIfNotExists()
        {
            //Arrange
            var idxResProviderMock = new Mock<IIndexResourceProvider>();

            idxResProviderMock.Setup(p => p.ProvideIndexSettingsAsync(It.Is<string>(s => s == "foo")))
                .Returns<string>(s => File.ReadAllTextAsync("resources\\foo\\index.json"));

            string indexName = NamePrefix + Guid.NewGuid().ToString("N");

            var services = new ServiceCollection()
                .AddLogging(l => l
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddXUnit(_output)
                    )            
                .AddSingleton(_fxt.Tools)
                .AddSingleton(idxResProviderMock.Object)
                .Configure<IndexerOptions>(o =>
                {
                    o.Indexes = new IndexOptions[]
                    {
                        new()
                        {
                            Id = "foo",
                            EsIndex = indexName
                        }
                    };
                })
                .BuildServiceProvider();

            var indexUploader = ActivatorUtilities.CreateInstance<IndexUploader>(services);

            //Act
            await indexUploader.UploadAsync(CancellationToken.None);

            var exists = await _fxt.Tools.Index(indexName).ExistsAsync();

            //Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ShouldNotUploadIndexIfAlreadyExists()
        {
            //Arrange
            string indexName = NamePrefix + Guid.NewGuid().ToString("N");

            var index1Request = await File.ReadAllTextAsync("resources\\foo\\index.json");

            await _fxt.Tools.Index(indexName).CreateAsync(index1Request);

            var idxResProviderMock = new Mock<IIndexResourceProvider>();

            idxResProviderMock.Setup(p => p.ProvideIndexSettingsAsync(It.Is<string>(s => s == "foo")))
                .Returns<string>(s => File.ReadAllTextAsync("resources\\foo\\index2.json"));

            var services = new ServiceCollection()
                .AddLogging(l => l
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddXUnit(_output)
                )
                .AddSingleton(_fxt.Tools)
                .AddSingleton(idxResProviderMock.Object)
                .Configure<IndexerOptions>(o =>
                {
                    o.Indexes = new IndexOptions[]
                    {
                        new()
                        {
                            Id = "foo",
                            EsIndex = indexName
                        }
                    };
                })
                .BuildServiceProvider();

            var indexUploader = ActivatorUtilities.CreateInstance<IndexUploader>(services);

            //Act
            await indexUploader.UploadAsync(CancellationToken.None);

            var indexInfo = await _fxt.Tools.Index(indexName).TryGet();

            //Assert
            Assert.NotNull(indexInfo);
            Assert.Contains(indexInfo.Mappings.Meta, p => p.Key == "ver" && (string)p.Value == "1");
        }
    }
}

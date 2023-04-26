using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MyLab.Log.XUnit;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Services.ResourceUploading;
using MyLab.Search.Indexer.Tools;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class LifecyclePolicyUploaderBehavior : IClassFixture<EsFixture<TestEsFixtureStrategy>>, IAsyncLifetime
    {
        private static readonly string NamePrefix = nameof(LifecyclePolicyUploaderBehavior).ToLower() + "-";

        private readonly EsFixture<TestEsFixtureStrategy> _fxt;
        private readonly ITestOutputHelper _output;
        private readonly string _indexerVer;

        public LifecyclePolicyUploaderBehavior(EsFixture<TestEsFixtureStrategy> fxt, ITestOutputHelper output)
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
            await _fxt.Tools.LifecyclePolicy("lifecycle-example").DeleteAsync();
        }

        [Fact]
        public async Task ShouldUploadPolicyIfDoesNotExists()
        {
            //Arrange
            var idxResProviderMock = new Mock<IResourceProvider>();

            idxResProviderMock.Setup(p => p.ProvideLifecyclePoliciesAsync())
                .Returns(() => new IResource[]{ new FileResource(new FileInfo("resources\\lifecycle-example.json")) });

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

            var uploader = ActivatorUtilities.CreateInstance<LifecyclePolicyUploader>(services);

            ServiceMetadata metadata = null;

            //Act
            await uploader.UploadAsync(CancellationToken.None);

            var policyInfo = await _fxt.Tools.LifecyclePolicy("lifecycle-example").TryGetAsync();

            if (policyInfo != null)
            {
                metadata = ServiceMetadata.Read(policyInfo.Policy.Meta);
            }

            //Assert
            Assert.NotNull(metadata);
            Assert.Equal(ServiceMetadata.MyCreator, metadata.Creator);
            Assert.Equal(_indexerVer, metadata.CreatorVer);
            Assert.Equal("1", metadata.Ver);
            Assert.Equal(DateTime.Now.Date, metadata.PutDt.Date);
            
        }
    }
}

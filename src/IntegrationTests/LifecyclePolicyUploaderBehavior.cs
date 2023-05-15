using System;
using System.IO;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollector.InProcDataCollector;
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
    public class LifecyclePolicyUploaderBehavior : IClassFixture<EsFixture<TestEsFixtureStrategy>>, IAsyncLifetime
    {
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
            await _fxt.Tools.LifecyclePolicy("lifecycle-test").DeleteAsync();
        }

        [Fact]
        public async Task ShouldUploadIfDoesNotExists()
        {
            //Arrange
            var idxResProviderMock = new Mock<IResourceProvider>();
            
            var policyResource = new TestResource("lifecycle-test", "resources\\lifecycle-example.json");

            var resourceHash = await TestTools.GetResourceHashAsync(policyResource);

            idxResProviderMock.Setup(p => p.ProvideLifecyclePolicies())
                .Returns(() => new IResource[] { policyResource });
            
            var services = new ServiceCollection()
                .AddLogging(l => l
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddXUnit(_output)
                    )            
                .AddSingleton(_fxt.Tools)
                .AddSingleton(idxResProviderMock.Object)
                .Configure<IndexerOptions>(o => o.AppId = "foo")
                .BuildServiceProvider();

            var uploader = ActivatorUtilities.CreateInstance<LifecyclePolicyUploader>(services);

            ServiceMetadata srvMeta = null;
            string ver = null;

            //Act
            await uploader.UploadAsync(CancellationToken.None);

            var policyInfo = await _fxt.Tools.LifecyclePolicy("lifecycle-test").TryGetAsync();

            if (policyInfo != null)
            {
                ServiceMetadata.TryGet(policyInfo.Policy.Meta, out srvMeta);
                ver = TestTools.GetComponentVer(policyInfo.Policy.Meta);
            }

            //Assert
            Assert.NotNull(srvMeta);
            Assert.Equal("foo", srvMeta.Owner);
            Assert.Equal(resourceHash, srvMeta.SourceHash);
            Assert.Equal("1", ver);
        }

        [Fact]
        public async Task ShouldUpdateIfExists()
        {
            //Arrange
            var idxResProviderMock = new Mock<IResourceProvider>();
            
            var policyResource = new TestResource("lifecycle-test", "resources\\lifecycle-example-2.json");

            var resourceHash = await TestTools.GetResourceHashAsync(policyResource);

            idxResProviderMock.Setup(p => p.ProvideLifecyclePolicies())
                .Returns(() => new IResource[] { policyResource });
            
            var services = new ServiceCollection()
                .AddLogging(l => l
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddXUnit(_output)
                    )
                .AddSingleton(_fxt.Tools)
                .Configure<IndexerOptions>(o => o.AppId = "foo")
                .AddSingleton(idxResProviderMock.Object)
                .BuildServiceProvider();

            var uploader = ActivatorUtilities.CreateInstance<LifecyclePolicyUploader>(services);

            ServiceMetadata srvMeta = null;
            string ver = null;

            var existentPolicyJson = await File.ReadAllTextAsync("resources\\existent-lifecycle.json");
            await _fxt.Tools.LifecyclePolicy("lifecycle-test").PutAsync(existentPolicyJson);

            //Act
            await uploader.UploadAsync(CancellationToken.None);

            var policyInfo = await _fxt.Tools.LifecyclePolicy("lifecycle-test").TryGetAsync();

            if (policyInfo != null)
            {
                ServiceMetadata.TryGet(policyInfo.Policy.Meta, out srvMeta);
                ver = TestTools.GetComponentVer(policyInfo.Policy.Meta);
            }

            //Assert
            Assert.NotNull(srvMeta);
            Assert.Equal(resourceHash, srvMeta.SourceHash);
            Assert.Equal("foo", srvMeta.Owner);
            Assert.Equal("2", ver);
        }

        [Fact]
        public async Task ShouldNotUpdateWithSameVersion()
        {
            //Arrange
            var idxResProviderMock = new Mock<IResourceProvider>();

            var policyResource = new TestResource("lifecycle-test", "resources\\lifecycle-example.json");

            var resourceHash = await TestTools.GetResourceHashAsync(policyResource);

            idxResProviderMock.Setup(p => p.ProvideLifecyclePolicies())
                .Returns(() => new IResource[] { policyResource });

            var lifecyclePolicyToolMock = new Mock<IEsLifecyclePolicyTool>();
            lifecyclePolicyToolMock.Setup(t => t.TryGetAsync(It.IsAny<CancellationToken>()))
                .Returns<CancellationToken>(ct =>
                    _fxt.Tools.LifecyclePolicy("lifecycle-test").TryGetAsync(ct));

            var toolsMock = new Mock<IEsTools>();
            toolsMock.Setup(m => m.LifecyclePolicy(It.IsAny<string>()))
                .Returns<string>(s => lifecyclePolicyToolMock.Object);
            toolsMock.SetupGet(m => m.Serializer)
                .Returns(() => _fxt.Tools.Serializer);

            var services = new ServiceCollection()
                .AddLogging(l => l
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddXUnit(_output)
                    )
                .AddSingleton(toolsMock.Object)
                .AddSingleton(idxResProviderMock.Object)
                .Configure<IndexerOptions>(o => o.AppId = "foo")
                .BuildServiceProvider();

            var uploader = ActivatorUtilities.CreateInstance<LifecyclePolicyUploader>(services);

            ServiceMetadata srvMeta = null;
            string ver = null;

            var existentPolicyJson = await File.ReadAllTextAsync("resources\\existent-lifecycle.json");
            await _fxt.Tools.LifecyclePolicy("lifecycle-test").PutAsync(existentPolicyJson);

            //Act
            await uploader.UploadAsync(CancellationToken.None);

            var policyInfo = await _fxt.Tools.LifecyclePolicy("lifecycle-test").TryGetAsync();

            if (policyInfo != null)
            {
                ServiceMetadata.TryGet(policyInfo.Policy.Meta, out srvMeta);
                ver = TestTools.GetComponentVer(policyInfo.Policy.Meta);
            }

            //Assert
            Assert.NotNull(srvMeta);
            Assert.Equal("foo", srvMeta.Owner);
            Assert.Equal(resourceHash, srvMeta.SourceHash);
            Assert.Equal("1", ver);
            lifecyclePolicyToolMock.Verify(t => t.PutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

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

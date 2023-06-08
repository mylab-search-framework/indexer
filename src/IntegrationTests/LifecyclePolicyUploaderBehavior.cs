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
    public partial class LifecyclePolicyUploaderBehavior : IClassFixture<EsFixture<TestEsFixtureStrategy>>, IAsyncLifetime
    {
        [Fact]
        public async Task  ShouldUploadIfDoesNotExists()
        {
            //Arrange
            var newPolicy = CreatePolicy("foo", "1", "hash");
            var resourceProvider = CreateResourceProvider("lifecycle-test", newPolicy);

            var services = new ServiceCollection()
                .AddLogging(l => l
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddXUnit(_output)
                    )            
                .AddSingleton(_fxt.Tools)
                .AddSingleton(resourceProvider)
                .Configure<IndexerOptions>(o => o.AppId = "foo")
                .BuildServiceProvider();

            var uploader = ActivatorUtilities.CreateInstance<LifecyclePolicyUploader>(services);

            ComponentMetadata componentMetadata = null;
            string ver = null;

            //Act
            await uploader.UploadAsync(CancellationToken.None);

            var policyInfo = await _fxt.Tools.LifecyclePolicy("lifecycle-test").TryGetAsync();

            if (policyInfo != null)
            {
                ComponentMetadata.TryGet(policyInfo.Policy.Meta, out componentMetadata);
                ver = TestTools.GetComponentVer(policyInfo.Policy.Meta);
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
            var originPolicy = CreateLifecyclePutRequest("lifecycle-test-policy", "foo", "1", "origin-hash");
            var newPolicy = CreatePolicy("foo", "2", "hash");
            var resourceProvider = CreateResourceProvider("lifecycle-test-policy", newPolicy);

            var services = new ServiceCollection()
                .AddLogging(l => l
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddXUnit(_output)
                    )
                .AddSingleton(_fxt.Tools)
                .Configure<IndexerOptions>(o => o.AppId = "foo")
                .AddSingleton(resourceProvider)
                .BuildServiceProvider();

            var uploader = ActivatorUtilities.CreateInstance<LifecyclePolicyUploader>(services);

            ComponentMetadata componentMetadata = null;
            string ver = null;
            
            await _fxt.Tools.LifecyclePolicy("lifecycle-test-policy").PutAsync(originPolicy);

            //Act
            await uploader.UploadAsync(CancellationToken.None);

            var policyInfo = await _fxt.Tools.LifecyclePolicy("lifecycle-test-policy").TryGetAsync();

            if (policyInfo != null)
            {
                ComponentMetadata.TryGet(policyInfo.Policy.Meta, out componentMetadata);
                ver = TestTools.GetComponentVer(policyInfo.Policy.Meta);
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
            var originPolicy = CreateLifecyclePutRequest("lifecycle-test-policy", "foo", "1", "same-hash");
            var newPolicy = CreatePolicy("foo", "2", "same-hash");
            var resourceProvider = CreateResourceProvider("lifecycle-test", newPolicy);

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
                .AddSingleton(resourceProvider)
                .Configure<IndexerOptions>(o => o.AppId = "foo")
                .BuildServiceProvider();

            var uploader = ActivatorUtilities.CreateInstance<LifecyclePolicyUploader>(services);

            ComponentMetadata componentMetadata = null;
            string ver = null;
            
            await _fxt.Tools.LifecyclePolicy("lifecycle-test-policy").PutAsync(originPolicy);

            //Act
            await uploader.UploadAsync(CancellationToken.None);

            var policyInfo = await _fxt.Tools.LifecyclePolicy("lifecycle-test-policy").TryGetAsync();

            if (policyInfo != null)
            {
                ComponentMetadata.TryGet(policyInfo.Policy.Meta, out componentMetadata);
                ver = TestTools.GetComponentVer(policyInfo.Policy.Meta);
            }

            //Assert
            Assert.NotNull(componentMetadata);
            Assert.Equal("foo", componentMetadata.Owner);
            Assert.Equal("same-hash", componentMetadata.SourceHash);
            Assert.Equal("1", ver);
            lifecyclePolicyToolMock.Verify(t => t.PutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        }
    }
}

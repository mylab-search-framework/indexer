using MyLab.Search.EsTest;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class FileResourceProviderBehavior : IAsyncLifetime, IClassFixture<EsFixture<TestEsFixtureStrategy>>
    {
        private readonly FileResourceProvider _resourceProvider;

        public FileResourceProviderBehavior(EsFixture<TestEsFixtureStrategy> fxt, ITestOutputHelper output)
        {
            fxt.Output = output;

            var opts = new IndexerOptions
            {
                ResourcesPath = Path.Combine(Directory.GetCurrentDirectory(), "for-loadres-test")
            };

            _resourceProvider = new FileResourceProvider(fxt.Tools, opts);
        }

        [Fact]
        public void ShouldLoadIndexes()
        {
            //Arrange
            var iDir = _resourceProvider.IndexDirectory;
            var fooIdxDir = iDir?.Named["foo-index"];

            //Assert
            Assert.NotNull(iDir);
            Assert.NotNull(fooIdxDir);
            Assert.Equal("foo-index", fooIdxDir.IndexId);

            Assert.NotNull(fooIdxDir.KickQuery);
            Assert.Equal("-- kick", fooIdxDir.KickQuery.Content);
            Assert.Equal("kick", fooIdxDir.KickQuery.Name);
            Assert.Equal("269663b14189f2996eec1efc90a86789", fooIdxDir.KickQuery.Hash);

            Assert.NotNull(fooIdxDir.SyncQuery);
            Assert.Equal("-- sync", fooIdxDir.SyncQuery.Content);
            Assert.Equal("sync", fooIdxDir.SyncQuery.Name);
            Assert.Equal("ae62d283e8997c3804e5a35e0e7d2bd0", fooIdxDir.SyncQuery.Hash);

            Assert.NotNull(fooIdxDir.Mapping);
            Assert.Equal("mapping", fooIdxDir.Mapping.Name);
            Assert.Equal("96ad7d8a0875d561a6e1b02a85a092bc", fooIdxDir.Mapping.Hash);
            Assert.NotNull(fooIdxDir.Mapping.Content);
            Assert.NotNull(fooIdxDir.Mapping.Content.Properties);

            var idPropFound = fooIdxDir.Mapping.Content.Properties.TryGetValue("Id", out var idProp);
            Assert.True(idPropFound);
            Assert.Equal("Id", idProp.Name);
            Assert.Equal("long", idProp.Type);

            var contentPropFound = fooIdxDir.Mapping.Content.Properties.TryGetValue("Content", out var contentProp);
            Assert.True(contentPropFound);
            Assert.Equal("Content", contentProp.Name);
            Assert.Equal("text", contentProp.Type);

            Assert.NotNull(iDir.CommonMapping);
            Assert.Equal("mapping", iDir.CommonMapping.Name);
            Assert.Equal("d6e9687d10fd3e74a0c4f1e116814101", iDir.CommonMapping.Hash);

            Assert.NotNull(iDir.CommonMapping.Content);
            Assert.NotNull(iDir.CommonMapping.Content.Properties);
            Assert.Single(iDir.CommonMapping.Content.Properties);
            var commonIdPropFound = iDir.CommonMapping.Content.Properties.TryGetValue("Id", out var commonIdProp);
            Assert.True(commonIdPropFound);
            Assert.Equal("Id", commonIdProp.Name);
            Assert.Equal("long", commonIdProp.Type);

        }

        [Fact]
        public void ShouldLoadComponentTemplates()
        {
            //Assert
            Assert.NotNull(_resourceProvider.ComponentTemplates);
            Assert.Single(_resourceProvider.ComponentTemplates);

            var ctFound = _resourceProvider.ComponentTemplates.TryGetValue("foo-c-template", out var foundT);
            Assert.True(ctFound);
            Assert.NotNull(foundT);
            Assert.Equal("foo-c-template", foundT.Name);
            Assert.Equal("40f024e3334964575dd797cc4ed0d78c", foundT.Hash);
            Assert.NotNull(foundT.Content);
            Assert.NotNull(foundT.Content.Template);
            Assert.NotNull(foundT.Content.Template.Mappings);
            Assert.NotNull(foundT.Content.Template.Mappings.Properties);
            Assert.Single(foundT.Content.Template.Mappings.Properties);
            
            var propFound = foundT.Content.Template.Mappings.Properties.TryGetValue("Id", out var foundProp);
            Assert.True(propFound);
            Assert.Equal("Id", foundProp.Name);
            Assert.Equal("long", foundProp.Type);
        }

        [Fact]
        public void ShouldLoadIndexTemplates()
        {
            //Assert
            Assert.NotNull(_resourceProvider.IndexTemplates);
            Assert.Single(_resourceProvider.IndexTemplates);

            var ctFound = _resourceProvider.IndexTemplates.TryGetValue("foo-i-template", out var foundT);
            Assert.True(ctFound);
            Assert.NotNull(foundT);
            Assert.Equal("foo-i-template", foundT.Name);
            Assert.Equal("50c11430623337f549487927b891ff03", foundT.Hash);
            Assert.NotNull(foundT.Content);
            Assert.NotNull(foundT.Content.Template);
            Assert.NotNull(foundT.Content.Template.Mappings);
            Assert.NotNull(foundT.Content.Template.Mappings.Properties);
            Assert.Equal(2, foundT.Content.Template.Mappings.Properties.Count);

            var createdAtPropFound = foundT.Content.Template.Mappings.Properties.TryGetValue("created_at", out var createdAtProp);
            Assert.True(createdAtPropFound);
            Assert.Equal("created_at", createdAtProp.Name);
            Assert.Equal("date", createdAtProp.Type);

            var hnFound = foundT.Content.Template.Mappings.Properties.TryGetValue("host_name", out var foundHn);
            Assert.True(hnFound);
            Assert.Equal("host_name", foundHn.Name);
            Assert.Equal("keyword", foundHn.Type);
        }

        [Fact]
        public void ShouldLoadLifecyclePolicies()
        {
            //Assert
            Assert.NotNull(_resourceProvider.LifecyclePolicies);
            Assert.Single(_resourceProvider.LifecyclePolicies); 
            
            var policyFound = _resourceProvider.LifecyclePolicies.TryGetValue("foo-lifecycle", out var foundPolicy);
            Assert.True(policyFound);
            Assert.NotNull(foundPolicy);
            Assert.Equal("foo-lifecycle", foundPolicy.Name);
            Assert.Equal("015e53e7d5355327ed7fee25d68f232c", foundPolicy.Hash);
            Assert.NotNull(foundPolicy.Content);
            Assert.NotNull(foundPolicy.Content.Policy);
            Assert.NotNull(foundPolicy.Content.Policy.Phases);
            Assert.NotNull(foundPolicy.Content.Policy.Phases.Warm);
            Assert.Equal("30d", foundPolicy.Content.Policy.Phases.Warm.MinimumAge);
            
            var actFound = foundPolicy.Content.Policy.Phases.Warm.Actions.TryGetValue("shrink", out var shrinkAction);
            Assert.True(actFound);
            Assert.NotNull(shrinkAction);

        }

        public Task InitializeAsync()
        {
            return _resourceProvider.StartAsync(CancellationToken.None);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}

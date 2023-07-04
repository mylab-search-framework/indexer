using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Services.ComponentUploading;
using Nest;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public partial class IndexTemplateUploaderBehavior
    {
        private readonly EsFixture<TestEsFixtureStrategy> _fxt;
        private readonly ITestOutputHelper _output;
        private readonly string _indexerVer;

        public IndexTemplateUploaderBehavior(EsFixture<TestEsFixtureStrategy> fxt, ITestOutputHelper output)
        {
            _fxt = fxt;
            _output = output;
            fxt.Output = output;

            _indexerVer = typeof(IComponentUploader).Assembly.GetName().Version?.ToString();
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await _fxt.Tools.IndexTemplate("index-template-test").DeleteAsync();
        }

        private IResourceProvider CreateResourceProvider(string idxId, IndexTemplate template)
        {
            var resProviderMock = new Mock<IResourceProvider>();

            if (!ComponentMetadata.TryGet(template.Meta, out var metadata))
            {
                throw new InvalidOperationException("Component metadata not found");
            }

            resProviderMock.SetupGet(p => p.IndexTemplates)
                .Returns(() => new NamedResources<IndexTemplate>(idxId, template, metadata.SourceHash));

            return resProviderMock.Object;
        }

        private Func<PutIndexTemplateV2Descriptor, IPutIndexTemplateV2Request> CreateTemplatePutDescriptor(string owner, string ver, string hash)
        {
            var newTemplateMetadata = new ComponentMetadata
            {
                Owner = owner,
                SourceHash = hash
            };

            return d => d
                .IndexPatterns(Guid.NewGuid().ToString("N") + "*")
                .Priority(100)
                .Template(t => t)
                .Meta(fd =>
                {
                    newTemplateMetadata.Save(fd);
                    return fd.Add("ver", ver);
                });
        }

        private IndexTemplate CreateTemplate(string owner, string ver, string hash)
        {
            var newTemplateMetaDict = new Dictionary<string, object> { { "ver", ver } };
            var newTemplate = new IndexTemplate
            {
                IndexPatterns = new[]{ Guid.NewGuid().ToString("N") + "*" },
                Priority = 1000,
                Template = new Template
                {
                    Mappings = new TypeMapping()
                },
                Meta = newTemplateMetaDict
            };

            var newTemplateMetadata = new ComponentMetadata
            {
                Owner = owner,
                SourceHash = hash
            };
            newTemplateMetadata.Save(newTemplateMetaDict);

            return newTemplate;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Services.ResourceUploading;
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

        private IPutIndexTemplateV2Request CreateTemplatePutRequest(string id, string owner, string ver, string hash)
        {
            var newTemplateMetaDict = new Dictionary<string, object> { { "ver", ver } };
            IPutIndexTemplateV2Request newTemplateReq = new PutIndexTemplateV2Request(id)
            {
                Template = new Template(),
                Meta = newTemplateMetaDict
            };

            var newTemplateMetadata = new ComponentMetadata
            {
                Owner = owner,
                SourceHash = hash
            };
            newTemplateMetadata.Save(newTemplateMetaDict);

            return newTemplateReq;
        }

        private IndexTemplate CreateTemplate(string owner, string ver, string hash)
        {
            var newTemplateMetaDict = new Dictionary<string, object> { { "ver", ver } };
            var newTemplate = new IndexTemplate
            {
                Template = new Template(),
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
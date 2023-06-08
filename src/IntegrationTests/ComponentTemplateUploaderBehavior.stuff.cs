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
    public partial class ComponentTemplateUploaderBehavior
    {
        private readonly EsFixture<TestEsFixtureStrategy> _fxt;
        private readonly ITestOutputHelper _output;
        private readonly string _indexerVer;

        public ComponentTemplateUploaderBehavior(EsFixture<TestEsFixtureStrategy> fxt, ITestOutputHelper output)
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
            await _fxt.Tools.ComponentTemplate("component-template-test").DeleteAsync();
        }

        private IResourceProvider CreateResourceProvider(string idxId, ComponentTemplate template)
        {
            var resProviderMock = new Mock<IResourceProvider>();

            if (!ComponentMetadata.TryGet(template.Meta, out var metadata))
            {
                throw new InvalidOperationException("Component metadata not found");
            }

            resProviderMock.SetupGet(p => p.ComponentTemplates)
                .Returns(() => new NamedResources<ComponentTemplate>(idxId, template, metadata.SourceHash));

            return resProviderMock.Object;
        }

        private IPutComponentTemplateRequest CreateTemplatePutRequest(string id, string owner, string ver, string hash)
        {
            var newTemplateMetaDict = new Dictionary<string, object> { { "ver", ver } };
            IPutComponentTemplateRequest newTemplateReq = new PutComponentTemplateRequest(id)
            {
                Meta = newTemplateMetaDict,
                Template = new Template()
            };

            var newTemplateMetadata = new ComponentMetadata
            {
                Owner = owner,
                SourceHash = hash
            };
            newTemplateMetadata.Save(newTemplateMetaDict);

            return newTemplateReq;
        }

        private ComponentTemplate CreateTemplate(string owner, string ver, string hash)
        {
            var newTemplateMetaDict = new Dictionary<string, object> { { "ver", ver } };
            var newTemplate = new ComponentTemplate
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
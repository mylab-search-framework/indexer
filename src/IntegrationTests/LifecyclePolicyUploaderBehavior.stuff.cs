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
    public partial class LifecyclePolicyUploaderBehavior
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
            var exists = await _fxt.Tools.LifecyclePolicy("lifecycle-test").ExistsAsync();
            if(exists)
                await _fxt.Tools.LifecyclePolicy("lifecycle-test").DeleteAsync();
        }

        private IResourceProvider CreateResourceProvider(string idxId, LifecyclePolicy policy)
        {
            var resProviderMock = new Mock<IResourceProvider>();

            if (!ComponentMetadata.TryGet(policy.Policy.Meta, out var metadata))
            {
                throw new InvalidOperationException("Component metadata not found");
            }

            resProviderMock.SetupGet(p => p.LifecyclePolicies)
                .Returns(() => new NamedResources<LifecyclePolicy>(idxId, policy, metadata.SourceHash));

            return resProviderMock.Object;
        }

        private IPutLifecycleRequest CreateLifecyclePutRequest(string id, string owner, string ver, string hash)
        {
            var newPolicyMetaDict = new Dictionary<string, object> { { "ver", ver } };
            
            IPutLifecycleRequest newPolicyDesc = new PutLifecycleDescriptor(id)
                .Policy(pd => pd
                    .Meta(newPolicyMetaDict)
                    .Phases(phd => phd
                        .Cold(cd => cd
                            .MinimumAge("1d")
                            .Actions(la => la.Delete(_ => new DeleteLifecycleAction() )))));

            var newPolicyMetadata = new ComponentMetadata
            {
                Owner = owner,
                SourceHash = hash
            };
            newPolicyMetadata.Save(newPolicyMetaDict);

            return newPolicyDesc;
        }

        private LifecyclePolicy CreatePolicy(string owner, string ver, string hash)
        {
            var newPolicyMetaDict = new Dictionary<string, object> { { "ver", ver } };
            var newPolicy = new LifecyclePolicy();

            var nestedPolicy = new Policy
            {
                Meta = newPolicyMetaDict
            };

            typeof(LifecyclePolicy).GetProperty(nameof(LifecyclePolicy.Policy))
                .SetValue(newPolicy, nestedPolicy, null);

            var newPolicyMetadata = new ComponentMetadata
            {
                Owner = owner,
                SourceHash = hash
            };
            newPolicyMetadata.Save(newPolicyMetaDict);

            return newPolicy;
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.Search.EsAdapter.Tools;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.Services.ResourceUploading
{
    class LifecyclePolicyUploader : IResourceUploader
    {
        private readonly IEsTools _esTools;
        private readonly IResourceProvider _resourceProvider;
        private readonly IDslLogger _log;
        private readonly IndexerOptions _opts;

        public LifecyclePolicyUploader(
            IEsTools esTools, 
            IResourceProvider resourceProvider,
            IOptions<IndexerOptions> options,
            ILogger<LifecyclePolicyUploader> logger = null)
        {
            _esTools = esTools;
            _resourceProvider = resourceProvider;
            _log = logger?.Dsl();
            _opts = options.Value;
        }
        public async Task UploadAsync(CancellationToken cancellationToken)
        {
            var policies = _resourceProvider.ProvideLifecyclePoliciesAsync();

            foreach (var policy in policies)
            {
                await TouchPolicyAsync(policy, cancellationToken);
            }
        }

        private async Task TouchPolicyAsync(IResource policy, CancellationToken cancellationToken)
        {
            var resPolicyName = _opts.GetEsName(policy.Name);

            try
            {
                var esPolicy = await _esTools.LifecyclePolicy(resPolicyName).TryGetAsync(cancellationToken);

                var srvMetadataFromEs = ServiceMetadata.Read(esPolicy?.Policy?.Meta);

                if (srvMetadataFromEs != null && srvMetadataFromEs.IsMyCreator())
                {
                    await using var rStream = policy.OpenRead();
                    var policyObj = _esTools.Deserializer.DeserializeLifecyclePolicy(rStream);

                    var srvMetadataFromRes = ServiceMetadata.Read(policyObj?.Policy?.Meta);

                    if (srvMetadataFromRes != null && srvMetadataFromEs.Ver != srvMetadataFromRes.Ver)
                    {
                        var policyJson = await policy.ReadAllTextAsync();
                        await _esTools.LifecyclePolicy(resPolicyName).PutAsync(policyJson, cancellationToken);
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error("Unable to upload lifecycle policy", e)
                    .AndFactIs("policy-name", resPolicyName)
                    .Write();
            }
        }
    }
}

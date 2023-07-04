using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Search.EsAdapter.Tools;
using MyLab.Search.Indexer.Options;
using Nest;

namespace MyLab.Search.Indexer.Services.ComponentUploading
{
    class LifecyclePolicyUploader : ComponentUploader<LifecyclePolicy>
    {
        public LifecyclePolicyUploader(
            IEsTools esTools,
            IResourceProvider resourceProvider,
            IOptions<IndexerOptions> options,
            ILogger<LifecyclePolicyUploader> logger = null) 
            : base(
                new LifecyclePolicyUploaderStrategy(),
                esTools,
                resourceProvider,
                options,
                logger
            )
        {
            
        }

        class LifecyclePolicyUploaderStrategy : IComponentUploaderStrategy<LifecyclePolicy>
    {
        public string ResourceSetName => "Lifecycle policies";

        public IResource<LifecyclePolicy>[] GetResources(IResourceProvider resourceProvider)
        {
            return resourceProvider.LifecyclePolicies.Values.ToArray();
        }

        public Task<LifecyclePolicy> TryGetComponentFromEsAsync(string componentId, IEsTools esTools, CancellationToken cancellationToken)
        {
            return esTools.LifecyclePolicy(componentId).TryGetAsync(cancellationToken);
        }

        public bool HasAbsentNode(LifecyclePolicy component, out string absentNodeName)
        {
            if (component.Policy == null)
            {
                absentNodeName = "policy";
                return true;
            }

            absentNodeName = null;
            return false;
        }

        public void SetMeta(string componentId, string appId, LifecyclePolicy component, IDictionary<string, object> newMeta)
        {
            component.Policy.Meta = newMeta;
        }

        public IReadOnlyDictionary<string, object> ProvideMeta(LifecyclePolicy component)
        {
            return component.Policy.Meta != null 
                ? new Dictionary<string, object>(component.Policy.Meta)
                : new Dictionary<string, object>();
        }

        public Task UploadComponentAsync(string componentId, LifecyclePolicy component, IEsTools esTools,
            CancellationToken cancellationToken)
        {
            var req = new PutLifecycleRequest(componentId)
            {
                Policy = component.Policy
            };

            return esTools.LifecyclePolicy(componentId).PutAsync(req,cancellationToken);
        }
    }
    }
}

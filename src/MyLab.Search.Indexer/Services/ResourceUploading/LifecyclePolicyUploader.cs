using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Search.EsAdapter.Inter;
using MyLab.Search.EsAdapter.Tools;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Tools;
using Nest;

namespace MyLab.Search.Indexer.Services.ResourceUploading
{
    class LifecyclePolicyUploader : ResourceUploader<LifecyclePolicy>
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

        class LifecyclePolicyUploaderStrategy : IResourceUploaderStrategy<LifecyclePolicy>
    {
        public string ResourceSetName => "Lifecycle policies";
        public string OneResourceName => "Lifecycle policy";

        public IResource[] GetResources(IResourceProvider resourceProvider)
        {
            return resourceProvider.ProvideLifecyclePolicies();
        }

        public Task<LifecyclePolicy> TryGetComponentFromEsAsync(string componentId, IEsTools esTools, CancellationToken cancellationToken)
        {
            return esTools.LifecyclePolicy(componentId).TryGetAsync(cancellationToken);
        }

        public LifecyclePolicy DeserializeComponent(IEsSerializer serializer, Stream inStream)
        {
            return serializer.DeserializeLifecyclePolicy(inStream);
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

        public void SetMeta(LifecyclePolicy component, IDictionary<string, object> newMeta)
        {
            component.Policy.Meta = newMeta;
        }

        public IReadOnlyDictionary<string, object> ProvideMeta(LifecyclePolicy component)
        {
            return new Dictionary<string, object>(component.Policy.Meta);
        }

        public void ApplyMetadata(LifecyclePolicy component, ServiceMetadata newMetadata)
        {
            var metaDict = component.Policy.Meta ??= new Dictionary<string, object>();

            newMetadata.Save(metaDict);
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

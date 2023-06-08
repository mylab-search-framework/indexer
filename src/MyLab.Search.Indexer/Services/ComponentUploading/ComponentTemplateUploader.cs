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
    class ComponentTemplateUploader : ComponentUploader<ComponentTemplate>
    {
        public ComponentTemplateUploader(
            IEsTools esTools,
            IResourceProvider resourceProvider,
            IOptions<IndexerOptions> options,
            ILogger<LifecyclePolicyUploader> logger = null) 
            : base(
                new ComponentTemplateUploaderStrategy(),
                esTools,
                resourceProvider,
                options,
                logger
            )
        {
            
        }

        class ComponentTemplateUploaderStrategy : IComponentUploaderStrategy<ComponentTemplate>
        {
            public string ResourceSetName => "Component templates";

            public IResource<ComponentTemplate>[] GetResources(IResourceProvider resourceProvider)
            {
                return resourceProvider.ComponentTemplates.Values
                    .Cast<IResource<ComponentTemplate>>()
                    .ToArray();
            }

            public Task<ComponentTemplate> TryGetComponentFromEsAsync(string componentId, IEsTools esTools, CancellationToken cancellationToken)
            {
                return esTools.ComponentTemplate(componentId).TryGetAsync(cancellationToken);
            }

            public bool HasAbsentNode(ComponentTemplate component, out string absentNodeName)
            {
                if (component.Template == null)
                {
                    absentNodeName = "template";
                    return true;
                }

                absentNodeName = null;
                return false;
            }

            public void SetMeta(string componentId, string appId, ComponentTemplate component, IDictionary<string, object> newMeta)
            {
                component.Meta = new Dictionary<string, object>(newMeta);
            }

            public IReadOnlyDictionary<string, object> ProvideMeta(ComponentTemplate component)
            {
                return component.Meta;
            }
            
            public Task UploadComponentAsync(string componentId, ComponentTemplate component, IEsTools esTools,
                CancellationToken cancellationToken)
            {
                var req = new PutComponentTemplateRequest(componentId)
                {
                    Template = component.Template,
                    Meta = new Dictionary<string, object>(component.Meta),
                    Version = component.Version
                };

                return esTools.ComponentTemplate(componentId).PutAsync(req, cancellationToken);
            }
        }
    }
}

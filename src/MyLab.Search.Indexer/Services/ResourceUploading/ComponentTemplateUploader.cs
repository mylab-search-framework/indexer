using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Search.EsAdapter.Inter;
using MyLab.Search.EsAdapter.Tools;
using MyLab.Search.Indexer.Options;
using Nest;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.Services.ResourceUploading
{
    class ComponentTemplateUploader : ResourceUploader<ComponentTemplate>
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

        class ComponentTemplateUploaderStrategy : IResourceUploaderStrategy<ComponentTemplate>
        {
            public string ResourceSetName => "Component templates";
            public string OneResourceName => "Component template";

            public IResource[] GetResources(IResourceProvider resourceProvider)
            {
                return resourceProvider.ProvideComponentTemplates();
            }

            public Task<ComponentTemplate> TryGetComponentFromEsAsync(string componentId, IEsTools esTools, CancellationToken cancellationToken)
            {
                return esTools.ComponentTemplate(componentId).TryGetAsync(cancellationToken);
            }

            public ComponentTemplate DeserializeComponent(IEsSerializer serializer, Stream inStream)
            {
                return serializer.Deserialize<ComponentTemplate>(inStream);
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

            public void SetMeta(ComponentTemplate component, IDictionary<string, object> newMeta)
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
                    Meta = new Dictionary<string, object>(component.Meta)
                };

                return esTools.ComponentTemplate(componentId).PutAsync(req, cancellationToken);
            }
        }
    }
}

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
    class IndexTemplateUploader : ComponentUploader<IndexTemplate>
    {
        public IndexTemplateUploader(
            IEsTools esTools,
            IResourceProvider resourceProvider,
            IOptions<IndexerOptions> options,
            ILogger<LifecyclePolicyUploader> logger = null) 
            : base(
                new IndexTemplateUploaderStrategy(),
                esTools,
                resourceProvider,
                options,
                logger
            )
        {
        }

        class IndexTemplateUploaderStrategy : IComponentUploaderStrategy<IndexTemplate>
        {
            public string ResourceSetName => "Index templates";

            public IResource<IndexTemplate>[] GetResources(IResourceProvider resourceProvider)
            {
                return resourceProvider.IndexTemplates.Values
                    .Cast<IResource<IndexTemplate>>()
                    .ToArray();
            }

            public Task<IndexTemplate> TryGetComponentFromEsAsync(string componentId, IEsTools esTools, CancellationToken cancellationToken)
            {
                return esTools.IndexTemplate(componentId).TryGetAsync(cancellationToken);
            }

            public bool HasAbsentNode(IndexTemplate component, out string absentNodeName)
            {
                if (component.Template == null)
                {
                    absentNodeName = "template";
                    return true;
                }

                absentNodeName = null;
                return false;
            }

            public void SetMeta(string componentId, string appId, IndexTemplate component, IDictionary<string, object> newMeta)
            {
                component.Meta = new Dictionary<string, object>(newMeta);

                var originMapping = component.Template?.Mappings;
                if (originMapping != null)
                {
                    originMapping.Meta ??= new Dictionary<string, object>();

                    var mappingMetadataObj = new MappingMetadata
                    {   
                        Template = new MappingMetadata.TemplateDesc
                        {
                            SourceName = componentId,
                            Owner = appId
                        }
                    };

                    mappingMetadataObj.Save(originMapping.Meta);
                }
            }

            public IReadOnlyDictionary<string, object> ProvideMeta(IndexTemplate component)
            {
                return component.Meta != null 
                    ? new Dictionary<string, object>(component.Meta)
                    : new Dictionary<string, object>();
            }
            
            public Task UploadComponentAsync(string componentId, IndexTemplate component, IEsTools esTools,
                CancellationToken cancellationToken)
            {
                var req = new PutIndexTemplateV2Request(componentId)
                {
                    Template = component.Template,
                    IndexPatterns = component.IndexPatterns,
                    Meta = component.Meta,
                    Priority = component.Priority,
                    ComposedOf = component.ComposedOf,
                    DataStream = component.DataStream,
                    Version = component.Version
                };

                return esTools.IndexTemplate(componentId).PutAsync(req, cancellationToken);
            }
        }
    }
}

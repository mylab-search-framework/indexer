using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Search.EsAdapter.Tools;
using MyLab.Search.Indexer.Options;
using Nest;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace MyLab.Search.Indexer.Services.ResourceUploading
{
    class IndexTemplateUploader : ResourceUploader<IndexTemplate>
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

        class IndexTemplateUploaderStrategy : IResourceUploaderStrategy<IndexTemplate>
        {
            public string ResourceSetName => "Index templates";
            public string OneResourceName => "Index template";

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

            public IndexTemplate DeserializeComponent(IEsSerializer serializer, Stream inStream)
            {
                return serializer.Deserialize<IndexTemplate>(inStream);
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

                var originMappingMetadata = component.Template?.Mappings;
                if (originMappingMetadata != null)
                {
                    var mappingMetadataObj = new MappingMetadata
                    {   
                        Template = new MappingMetadata.TemplateDesc
                        {
                            SourceName = componentId,
                            Owner = appId
                        }
                    };

                    mappingMetadataObj.Save(originMappingMetadata.Meta ??= new Dictionary<string, object>());
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
                    Meta = component.Meta
                };

                return esTools.IndexTemplate(componentId).PutAsync(req, cancellationToken);
            }
        }
    }
}

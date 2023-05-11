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

            public IResource[] GetResources(IResourceProvider resourceProvider)
            {
                return resourceProvider.ProvideIndexTemplates();
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

            public void SetMeta(IndexTemplate component, IDictionary<string, object> newMeta)
            {
                component.Meta = new Dictionary<string, object>(newMeta);
            }

            public IReadOnlyDictionary<string, object> ProvideMeta(IndexTemplate component)
            {
                return new Dictionary<string, object>(component.Meta);
            }

            public void ApplyMetadata(IndexTemplate component, ServiceMetadata newMetadata)
            {
                var metaDict = component.Meta ??= new Dictionary<string, object>();

                newMetadata.Save(metaDict);
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

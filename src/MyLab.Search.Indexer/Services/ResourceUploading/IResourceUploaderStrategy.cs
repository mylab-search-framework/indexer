using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MyLab.Search.EsAdapter.Tools;
using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.Services.ResourceUploading
{
    interface IResourceUploaderStrategy<TEsComponent>
    {
        string ResourceSetName { get; }
        string OneResourceName { get; }
        IResource<TEsComponent>[] GetResources(IResourceProvider resourceProvider);
        Task<TEsComponent> TryGetComponentFromEsAsync(string componentId, IEsTools esTools, CancellationToken cancellationToken);
        TEsComponent DeserializeComponent(IEsSerializer serializer, Stream inStream);
        bool HasAbsentNode(TEsComponent component, out string absentNodeName);
        void SetMeta(string componentId, string appId, TEsComponent component, IDictionary<string, object> newMeta);
        IReadOnlyDictionary<string, object> ProvideMeta(TEsComponent component);

        Task UploadComponentAsync(string componentId, TEsComponent component, IEsTools esTools, CancellationToken cancellationToken);
    }
}
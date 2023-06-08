using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MyLab.Search.EsAdapter.Tools;

namespace MyLab.Search.Indexer.Services.ComponentUploading
{
    interface IComponentUploaderStrategy<TEsComponent>
    {
        string ResourceSetName { get; }
        IResource<TEsComponent>[] GetResources(IResourceProvider resourceProvider);
        Task<TEsComponent> TryGetComponentFromEsAsync(string componentId, IEsTools esTools, CancellationToken cancellationToken);
        bool HasAbsentNode(TEsComponent component, out string absentNodeName);
        void SetMeta(string componentId, string appId, TEsComponent component, IDictionary<string, object> newMeta);
        IReadOnlyDictionary<string, object> ProvideMeta(TEsComponent component);

        Task UploadComponentAsync(string componentId, TEsComponent component, IEsTools esTools, CancellationToken cancellationToken);
    }
}
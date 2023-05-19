using System.IO;
using System.Text;
using System.Threading.Tasks;
using MyLab.Search.EsAdapter.Tools;
using MyLab.Search.Indexer.Tools;
using Nest;

namespace MyLab.Search.Indexer.Services
{
    public interface IIndexMappingProvider
    {
        Task<IndexMappingDesc> ProvideAsync(string idxId);
    }

    public class IndexMappingDesc
    {
        public TypeMapping Mapping { get; init; }
        public string SourceHash { get; init; }
    }

    class IndexMappingProvider : IIndexMappingProvider
    {
        private readonly IResourceProvider _resourceProvider;
        private readonly IEsTools _esTools;

        public IndexMappingProvider(
            IResourceProvider resourceProvider,
            IEsTools esTools)
        {
            _resourceProvider = resourceProvider;
            _esTools = esTools;
        }
        public async Task<IndexMappingDesc> ProvideAsync(string idxId)
        {
            var mappingStr = await _resourceProvider.ProvideIndexMappingAsync(idxId);

            var mappingBin = Encoding.UTF8.GetBytes(mappingStr);

            using var stream = new MemoryStream(mappingBin);
            var mapping = _esTools.Serializer.Deserialize<TypeMapping>(stream);

            return new IndexMappingDesc
            {
                Mapping = mapping,
                SourceHash = HashCalculator.Calculate(mappingBin)
            };
        }
    }
}

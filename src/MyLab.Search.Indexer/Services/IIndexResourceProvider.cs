using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Services
{
    public interface IIndexResourceProvider
    {
        Task<string> ProvideKickQueryAsync(string indexId);
        Task<string> ProvideSyncQueryAsync(string indexId);
    }

}

using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Services
{
    class FileIndexResourceProvider : IIndexResourceProvider
    {
        public Task<string> ProvideKickQueryAsync(string indexId)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> ProvideSyncQueryAsync(string indexId)
        {
            throw new System.NotImplementedException();
        }
    }
}

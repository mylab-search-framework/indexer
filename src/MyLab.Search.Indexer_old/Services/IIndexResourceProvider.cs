using System.Threading.Tasks;
using MyLab.Search.EsAdapter;

namespace MyLab.Search.Indexer.Services
{
    public interface IIndexResourceProvider
    {
        Task<string> ReadFileAsync(string idxId, string filename);
        Task<string> ReadDefaultFileAsync(string filename);
    }
}

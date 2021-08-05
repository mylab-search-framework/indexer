using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Services
{
    public interface ISeedService
    {
        Task WriteAsync(string seed);

        Task<string> ReadAsync();
    }
}

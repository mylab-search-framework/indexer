using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Services
{
    public interface ISeedService
    {
        Task WriteAsync(string jobId, string seed);

        Task<string> ReadAsync(string jobId);
    }
}

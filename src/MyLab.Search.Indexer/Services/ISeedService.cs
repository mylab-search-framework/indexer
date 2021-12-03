using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Services
{
    public interface ISeedService
    {
        Task WriteAsync(string nsId, string seed);

        Task<string> ReadAsync(string nsId);
    }
}

using System.IO;
using System.Threading.Tasks;

namespace MyLab.Search.Indexer
{
    public interface ISeedService
    {
        Task WriteAsync(long seed);

        Task<long> ReadAsync();
    }

    class FileSeedService : ISeedService
    {
        private const string Filename = "data-seed";
        public Task WriteAsync(long seed)
        {
            return File.WriteAllTextAsync(Filename, seed.ToString());
        }

        public async Task<long> ReadAsync()
        {
            return File.Exists(Filename)
                ? long.Parse(await File.ReadAllTextAsync(Filename))
                : 0;
        }
    }
}

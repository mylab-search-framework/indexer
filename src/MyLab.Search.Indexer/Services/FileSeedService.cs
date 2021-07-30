using System.IO;
using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Services
{
    class FileSeedService : ISeedService
    {
        private const string Filename = "data-seed";
        public Task WriteAsync(string seed)
        {
            return File.WriteAllTextAsync(Filename, seed);
        }

        public async Task<string> ReadAsync()
        {
            return File.Exists(Filename)
                ? await File.ReadAllTextAsync(Filename)
                : null;
        }
    }
}
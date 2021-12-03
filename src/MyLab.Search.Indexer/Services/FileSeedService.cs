using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace MyLab.Search.Indexer.Services
{
    class FileSeedService : ISeedService
    {
        private readonly IndexerOptions _options;

        public FileSeedService(IOptions<IndexerOptions> options)
            : this(options.Value)
        {

        }
        public FileSeedService(IndexerOptions options)
        {
            _options = options;
        }

        public Task WriteAsync(string nsId, string seed)
        {
            var fn = nsIdToFilename(nsId);
            var dir = Path.GetDirectoryName(fn);

            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return File.WriteAllTextAsync(fn, seed);
        }

        public async Task<string> ReadAsync(string nsId)
        {
            var fn = nsIdToFilename(nsId);
            return File.Exists(fn)
                ? await File.ReadAllTextAsync(fn)
                : null;
        }

        string nsIdToFilename(string nsId)
        {
            return Path.Combine(_options.SeedPath, nsId);
        }
    }
}
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

        public Task WriteAsync(string jobId, string seed)
        {
            var fn = JobIdToFilename(jobId);
            var dir = Path.GetDirectoryName(fn);

            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return File.WriteAllTextAsync(fn, seed);
        }

        public async Task<string> ReadAsync(string jobId)
        {
            var fn = JobIdToFilename(jobId);
            return File.Exists(fn)
                ? await File.ReadAllTextAsync(fn)
                : null;
        }

        string JobIdToFilename(string jobId)
        {
            return Path.Combine(_options.SeedPath, jobId);
        }
    }
}
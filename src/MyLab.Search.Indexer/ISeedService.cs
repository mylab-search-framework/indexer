using System;
using System.IO;
using System.Threading.Tasks;

namespace MyLab.Search.Indexer
{
    public interface ISeedService
    {
        Task WriteAsync(DateTime seed);

        Task<DateTime> ReadAsync();
    }

    class FileSeedService : ISeedService
    {
        private const string Filename = "data-seed";
        public Task WriteAsync(DateTime seed)
        {
            return File.WriteAllTextAsync(Filename, seed.ToString("s"));
        }

        public async Task<DateTime> ReadAsync()
        {
            return File.Exists(Filename)
                ? DateTime.Parse(await File.ReadAllTextAsync(Filename))
                : DateTime.MinValue;
        }
    }
}

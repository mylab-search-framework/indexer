using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.Options;

namespace MyLab.Search.Indexer.Services
{
    class FileSeedService : ISeedService
    {
        private readonly string _basePath;

        public FileSeedService(IOptions<IndexerOptions> opts)
            :this(opts.Value.SeedPath)
        {

        }

        public FileSeedService(string basePath)
        {
            if(string.IsNullOrWhiteSpace(basePath))
                throw new InvalidOperationException("Base seed path is not defined");

            _basePath = basePath;
        }

        public async Task SaveSeedAsync(string indexId, Seed seed)
        {
            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);

            var fullPath = GetIndexSeedFilePath(indexId);
            
            await File.WriteAllTextAsync(fullPath, seed.ToString());
        }

        public async Task<Seed> LoadSeedAsync(string indexId)
        {
            var fullPath = GetIndexSeedFilePath(indexId);

            var file = new FileInfo(fullPath);

            if (!file.Exists || file.Length == 0)
                return -1;

            var line = await ReadLineAsync(file);

            return Seed.Parse(line);
        }
        
        string GetIndexSeedFilePath(string indexId)
        {
            return Path.Combine(_basePath, indexId);
        }

        async Task<string> ReadLineAsync(FileInfo file)
        {
            using var rdr = file.OpenText();

            return await rdr.ReadLineAsync();
        }
    }
}

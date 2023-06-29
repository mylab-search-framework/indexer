using System.IO;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
using Org.BouncyCastle.Asn1.Cms;

namespace MyLab.Search.Indexer.Services
{
    public interface ISeedService
    {
        Task SaveSeedAsync(string indexId, Seed seed);
        Task<Seed> LoadSeedAsync(string indexId);
    }

    class FileSeedService : ISeedService
    {
        private readonly string _basePath;

        public FileSeedService(IOptions<IndexerOptions> opts)
            : this(opts.Value.SeedPath)
        {

        }

        public FileSeedService(string basePath)
        {
            if (string.IsNullOrWhiteSpace(basePath))
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
                return Seed.Empty;

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

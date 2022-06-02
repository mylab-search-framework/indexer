using System;
using System.IO;
using System.Threading.Tasks;
using MyLab.Log;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.Tools
{
    class FileSeedService : ISeedService
    {
        private readonly string _basePath;
        
        public FileSeedService(string basePath)
        {
            if(string.IsNullOrWhiteSpace(basePath))
                throw new InvalidOperationException("Base seed path is not defined");

            _basePath = basePath;
        }

        public async Task SaveSeedAsync(string indexId, long idSeed)
        {
            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);

            var fullPath = GetIndexSeedFilePath(indexId);

            await File.WriteAllTextAsync(fullPath, idSeed.ToString("D"));
        }

        public async Task SaveSeedAsync(string indexId, DateTime dtSeed)
        {
            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);

            var fullPath = GetIndexSeedFilePath(indexId);

            await File.WriteAllTextAsync(fullPath, dtSeed.ToString("O"));
        }

        public async Task<long> LoadIdSeedAsync(string indexId)
        {
            var fullPath = GetIndexSeedFilePath(indexId);

            var file = new FileInfo(fullPath);

            if (!file.Exists || file.Length == 0)
                return -1;

            var line = await ReadLineAsync(file);

            if (!long.TryParse(line, out var seed))
                throw new InvalidOperationException("Stored seed has wrong 'long' format")
                    .AndFactIs("value", line);

            return seed;
        }

        public async Task<DateTime> LoadDtSeedAsync(string indexId)
        {
            var fullPath = GetIndexSeedFilePath(indexId);

            var file = new FileInfo(fullPath);

            if (!file.Exists || file.Length == 0)
                return DateTime.MinValue;

            var line = await ReadLineAsync(file);

            if (!DateTime.TryParse(line, out var seed))
                throw new InvalidOperationException("Stored seed has wrong 'DateTime' format")
                    .AndFactIs("value", line);

            return seed;
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

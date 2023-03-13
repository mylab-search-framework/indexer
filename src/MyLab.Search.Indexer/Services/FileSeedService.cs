using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLab.Log;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.Options;
using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Services
{
    class FileSeedService : ISeedService
    {
        private readonly string _basePath;
        private readonly IDslLogger _log;

        public FileSeedService(IOptions<IndexerOptions> opts, ILogger<FileSeedService> logger = null)
            :this(opts.Value.SeedPath, logger)
        {

        }

        public FileSeedService(string basePath, ILogger<FileSeedService> logger = null)
        {
            if(string.IsNullOrWhiteSpace(basePath))
                throw new InvalidOperationException("Base seed path is not defined");

            _basePath = basePath;
            _log = logger?.Dsl();
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
            {
                _log?.Warning("Stored seed has wrong 'long' format. Initial value will be used '0'.")
                    .AndFactIs("seed-string", line)
                    .Write();

                return 0;
            }
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
            {
                _log?.Warning($"Stored seed has wrong 'DateTime' format. Initial value will be used '{DateTime.MinValue:s}'.")
                    .AndFactIs("seed-string", line)
                    .Write();

                return DateTime.MinValue;
            }

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

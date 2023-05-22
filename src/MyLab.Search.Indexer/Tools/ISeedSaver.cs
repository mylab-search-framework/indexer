using System;
using System.Threading.Tasks;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.Tools
{
    public interface ISeedSaver
    {
        Task SaveAsync();
    }

    class SeedSaver : ISeedSaver
    {
        private readonly string _indexId;
        private readonly Seed _seed;
        private readonly ISeedService _seedService;

        public SeedSaver(string indexId, Seed seed, ISeedService seedService)
        {
            _indexId = indexId;
            _seed = seed;
            _seedService = seedService;
        }
        public Task SaveAsync()
        {
            return _seedService.SaveSeedAsync(_indexId, _seed);
        }
    }
}

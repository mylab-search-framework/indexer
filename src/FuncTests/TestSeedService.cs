using System;
using System.Threading.Tasks;
using MyLab.Search.Indexer.Services;

namespace FuncTests
{
    class TestSeedService : ISeedService
    {
        public Seed Seed { get; set; }
        
        public Task SaveSeedAsync(string indexId, Seed seed)
        {
            Seed = seed;
            return Task.CompletedTask;
        }
        
        public Task<Seed> LoadSeedAsync(string indexId)
        {
            return Task.FromResult(Seed);
        }
    }
}
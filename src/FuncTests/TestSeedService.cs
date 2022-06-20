using System;
using System.Threading.Tasks;
using MyLab.Search.Indexer.Services;

namespace FuncTests
{
    class TestSeedService : ISeedService
    {
        public DateTime DtSeed { get; private set; }
        public long IdSeed { get; set; }

        public TestSeedService(DateTime initialDtSeed)
        {
            DtSeed = initialDtSeed;
        }
        public TestSeedService(long initialIdSeed)
        {
            IdSeed = initialIdSeed;
        }
        public Task SaveSeedAsync(string indexId, long idSeed)
        {
            IdSeed = idSeed;
            return Task.CompletedTask;
        }

        public Task SaveSeedAsync(string indexId, DateTime dtSeed)
        {
            DtSeed = dtSeed;
            return Task.CompletedTask;
        }

        public Task<long> LoadIdSeedAsync(string indexId)
        {
            return Task.FromResult(IdSeed);
        }

        public Task<DateTime> LoadDtSeedAsync(string indexId)
        {
            return Task.FromResult(DtSeed);
        }
    }
}
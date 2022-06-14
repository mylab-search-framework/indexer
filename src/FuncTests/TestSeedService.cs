using System;
using System.Threading.Tasks;
using MyLab.Search.Indexer.Services;

namespace FuncTests
{
    class TestSeedService : ISeedService
    {
        public DateTime DtSeed { get; private set; }
        
        public TestSeedService(DateTime initialDtSeed)
        {
            DtSeed = initialDtSeed;
        }

        public Task SaveSeedAsync(string indexId, long idSeed)
        {
            throw new NotImplementedException();
        }

        public Task SaveSeedAsync(string indexId, DateTime dtSeed)
        {
            DtSeed = dtSeed;
            return Task.CompletedTask;
        }

        public Task<long> LoadIdSeedAsync(string indexId)
        {
            throw new NotImplementedException();
        }

        public Task<DateTime> LoadDtSeedAsync(string indexId)
        {
            return Task.FromResult(DtSeed);
        }
    }
}
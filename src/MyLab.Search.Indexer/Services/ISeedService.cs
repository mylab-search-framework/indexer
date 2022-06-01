using System;
using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Services
{
    public interface ISeedService
    {
        Task SaveSeedAsync(string indexId, ulong idSeed);
        Task SaveSeedAsync(string indexId, DateTime dtSeed);
        Task<ulong> LoadIdSeedAsync(string indexId);
        Task<DateTime> LoadDtSeedAsync(string indexId);
    }
}

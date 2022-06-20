using System;
using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Services
{
    public interface ISeedService
    {
        Task SaveSeedAsync(string indexId, long idSeed);
        Task SaveSeedAsync(string indexId, DateTime dtSeed);
        Task<long> LoadIdSeedAsync(string indexId);
        Task<DateTime> LoadDtSeedAsync(string indexId);
    }
}

using System;
using System.Threading.Tasks;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.Tools
{
    public interface ISeedSaver
    {
        Task SaveAsync();
    }

    class IdSeedSaver : ISeedSaver
    {
        private readonly string _indexId;
        private readonly ulong _idSeed;
        private readonly ISeedService _seedService;

        public IdSeedSaver(string indexId, ulong idSeed, ISeedService seedService)
        {
            _indexId = indexId;
            _idSeed = idSeed;
            _seedService = seedService;
        }
        public Task SaveAsync()
        {
            return _seedService.SaveSeedAsync(_indexId, _idSeed);
        }
    }

    class DtSeedSaver : ISeedSaver
    {
        private readonly string _indexId;
        private readonly DateTime _dtSeed;
        private readonly ISeedService _seedService;

        public DtSeedSaver(string indexId, DateTime dtSeed, ISeedService seedService)
        {
            _indexId = indexId;
            _dtSeed = dtSeed;
            _seedService = seedService;
        }
        public Task SaveAsync()
        {
            return _seedService.SaveSeedAsync(_indexId, _dtSeed);
        }
    }
}

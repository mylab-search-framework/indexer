using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.LogicStrategy
{
    class AddModeIndexerLogicStrategy : IIndexerLogicStrategy
    {
        private readonly string _nsId;
        private readonly string _idFieldName;
        private readonly ISeedService _seedService;
        public IDslLogger Log { get; set; }

        public AddModeIndexerLogicStrategy(string nsId, string idFieldName, ISeedService seedService)
        {
            _nsId = nsId;
            _idFieldName = idFieldName;
            _seedService = seedService;
        }

        public ISeedCalc CreateSeedCalc()
        {
            return new IdSeedCalc(_nsId, _idFieldName, _seedService)
            {
                Log = Log
            };
        }

        public async Task<DataParameter> CreateSeedDataParameterAsync()
        {
            var seed = await _seedService.ReadIdAsync(_nsId);
            return new DataParameter(QueryParameterNames.Seed, seed, DataType.Long);
        }
    }
}
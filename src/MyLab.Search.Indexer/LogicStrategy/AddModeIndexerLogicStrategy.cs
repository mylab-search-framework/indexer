using MyLab.Log.Dsl;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.LogicStrategy
{
    class AddModeIndexerLogicStrategy : IIndexerLogicStrategy
    {
        private readonly string _idFieldName;
        private readonly ISeedService _seedService;
        public IDslLogger Log { get; set; }

        public AddModeIndexerLogicStrategy(string idFieldName, ISeedService seedService)
        {
            _idFieldName = idFieldName;
            _seedService = seedService;
        }

        public ISeedCalc CreateSeedCalc()
        {
            return new IdSeedCalc(_idFieldName, _seedService);
        }
    }
}
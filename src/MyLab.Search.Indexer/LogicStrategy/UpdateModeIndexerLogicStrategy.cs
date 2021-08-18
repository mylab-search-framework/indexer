using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.LogicStrategy
{
    class UpdateModeIndexerLogicStrategy : IIndexerLogicStrategy
    {
        private readonly string _jobId;
        private readonly string _lastModifiedFiledName;
        private readonly ISeedService _seedService;

        public IDslLogger Log { get; set; }

        public UpdateModeIndexerLogicStrategy(string jobId, string lastModifiedFiledName, ISeedService seedService)
        {
            _jobId = jobId;
            _lastModifiedFiledName = lastModifiedFiledName;
            _seedService = seedService;
        }

        public ISeedCalc CreateSeedCalc()
        {
            return new LastModifiedSeedCalc(_jobId, _lastModifiedFiledName, _seedService)
            {
                Log = Log
            };
        }

        public async Task<DataParameter> CreateSeedDataParameterAsync()
        {
            var seed = await _seedService.ReadDateTimeAsync(_jobId);
            return new DataParameter(QueryParameterNames.Seed, seed, DataType.DateTime);
        }
    }
}
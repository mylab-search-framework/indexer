using System;
using System.Linq;
using System.Threading.Tasks;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.LogicStrategy
{
    class IdSeedCalc : ISeedCalc
    {
        private readonly string _idFieldName;
        private readonly ISeedService _seedService;
        private long _lastId;

        public IDslLogger Log { get; set; }

        public IdSeedCalc(string idFieldName, ISeedService seedService)
        {
            _idFieldName = idFieldName;
            _seedService = seedService;
        }

        public async Task StartAsync()
        {
            _lastId = await _seedService.ReadIdAsync();
        }

        public void Update(DataSourceEntity[] entities)
        {
            var maxIdFromBatch = entities
                .Select(ExtractId)
                .Max();

            _lastId = Math.Max(_lastId, maxIdFromBatch);
        }

        public Task SaveAsync()
        {
            return _seedService.WriteIdAsync(_lastId);
        }

        public string GetLogValue()
        {
            return _lastId.ToString();
        }

        long ExtractId(DataSourceEntity e)
        {
            if (_idFieldName == null)
                return 0;

            if (e.Properties.TryGetValue(_idFieldName, out var idFieldValue))
            {
                if (long.TryParse(idFieldValue.Value, out var id))
                {
                    return id;
                }
                else
                {
                    Log.Error("Can't parse Id value")
                        .AndFactIs("actual", id)
                        .Write();

                    return 0;
                }
            }
            else
            {
                Log.Error("Id field not found")
                    .AndFactIs("Expected field name", _idFieldName)
                    .Write();

                return 0;
            }
        }
    }
}
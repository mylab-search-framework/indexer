using System;
using System.Linq;
using System.Threading.Tasks;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.LogicStrategy
{
    class LastModifiedSeedCalc : ISeedCalc
    {
        private readonly string _lastModifiedFieldName;
        private readonly ISeedService _seedService;
        private DateTime _lastModified;

        public IDslLogger Log { get; set; }

        public LastModifiedSeedCalc(string lastModifiedFieldName, ISeedService seedService)
        {
            _lastModifiedFieldName = lastModifiedFieldName;
            _seedService = seedService;
        }

        public async Task StartAsync()
        {
            _lastModified = await _seedService.ReadDateTimeAsync();
        }

        public void Update(DataSourceEntity[] entities)
        {
            var maxLastModifiedFromBatch = entities
                .Select(ExtractLastModified)
                .Max();

            _lastModified = maxLastModifiedFromBatch > _lastModified
                ? maxLastModifiedFromBatch
                : _lastModified;
        }

        public Task SaveAsync()
        {
            return _seedService.WriteDateTimeAsync(_lastModified);
        }

        public string GetLogValue()
        {
            return _lastModified.ToString("O");
        }

        DateTime ExtractLastModified(DataSourceEntity e)
        {
            if (_lastModifiedFieldName == null)
                return DateTime.MinValue;

            if (e.Properties.TryGetValue(_lastModifiedFieldName, out var lastModifiedFieldValue))
            {
                if (DateTime.TryParse(lastModifiedFieldValue.Value, out var lastModified))
                {
                    return lastModified;
                }
                else
                {
                    Log.Error("Can't parse lastModified date time value")
                        .AndFactIs("actual", lastModifiedFieldValue)
                        .Write();

                    return DateTime.MinValue;
                }
            }
            else
            {
                Log.Error("LastModified field not found")
                    .AndFactIs("Expected field name", _lastModifiedFieldName)
                    .Write();

                return DateTime.MinValue;
            }
        }
    }
}
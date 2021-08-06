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
        private readonly string _lastChangeProperty;
        private readonly ISeedService _seedService;
        private DateTime _lastModified;

        public IDslLogger Log { get; set; }

        public LastModifiedSeedCalc(string lastChangeProperty, ISeedService seedService)
        {
            _lastChangeProperty = lastChangeProperty;
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
            if (_lastChangeProperty == null)
                return DateTime.MinValue;

            if (e.Properties.TryGetValue(_lastChangeProperty, out var lastModifiedFieldValue))
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
                Log.Error("Last change property not found")
                    .AndFactIs("Expected field name", _lastChangeProperty)
                    .AndFactIs("Actual fields", string.Join(", ", e.Properties.Keys))
                    .Write();

                return DateTime.MinValue;
            }
        }
    }
}
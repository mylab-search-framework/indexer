using System;
using System.Threading;
using System.Threading.Tasks;
using MyLab.Log;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.Tools
{
    class CreateIndexStrategyFactory
    {
        private readonly JobOptions _options;
        private readonly IJobResourceProvider _jobResourceProvider;
        private readonly DataSourceEntity _exampleEntity;

        public IDslLogger Log { get; set; }

        public CreateIndexStrategyFactory(JobOptions options, IJobResourceProvider jobResourceProvider, DataSourceEntity exampleEntity)
        {
            _options = options;
            _jobResourceProvider = jobResourceProvider;
            _exampleEntity = exampleEntity;
        }

        public async Task<ICreateIndexStrategy> CreateAsync(CancellationToken cancellationToken)
        {
            switch (_options.NewIndexStrategy)
            {
                case NewIndexStrategy.Undefined:
                    throw new InvalidOperationException("IndexAsync creation mode not defined");
                case NewIndexStrategy.Auto:
                {
                    var autoStrategy = new AutoSettingsBasedCreateIndexStrategy(_exampleEntity) { Log = Log };
                    return autoStrategy;
                }
                case NewIndexStrategy.File:
                {
                    var request = await _jobResourceProvider.ReadFileAsync(_options.JobId, "new-index.json");

                    var jsonStrategy = new JsonSettingsBasedCreateIndexStrategy(request)
                    {
                        Log = Log
                    };

                    return jsonStrategy;
                }
                default:
                    throw new ArgumentOutOfRangeException("Unexpected creation mode", (Exception)null)
                        .AndFactIs("actual", _options.NewIndexStrategy);
            }

            
        }
    }
}
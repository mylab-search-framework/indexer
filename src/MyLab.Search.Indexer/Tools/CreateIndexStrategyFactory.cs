using System;
using System.Threading;
using System.Threading.Tasks;
using MyLab.Log;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.DataContract;

namespace MyLab.Search.Indexer.Tools
{
    class CreateIndexStrategyFactory
    {
        private readonly IndexerOptions _options;
        private readonly DataSourceEntity _exampleEntity;

        public IDslLogger Log { get; set; }

        public CreateIndexStrategyFactory(IndexerOptions options, DataSourceEntity exampleEntity)
        {
            _options = options;
            _exampleEntity = exampleEntity;
        }

        public async Task<ICreateIndexStrategy> CreateAsync(CancellationToken cancellationToken)
        {
            switch (_options.NewIndexStrategy)
            {
                case NewIndexStrategy.Undefined:
                    throw new InvalidOperationException("Index creation mode not defined");
                case NewIndexStrategy.Auto:
                {
                    var autoStrategy = new AutoSettingsBasedCreateIndexStrategy(_exampleEntity) { Log = Log };
                    return autoStrategy;
                }
                case NewIndexStrategy.File:
                {
                    var jsonStrategy = await JsonSettingsBasedCreateIndexStrategy.LoadFormFileAsync(_options.NewIndexRequestFile, cancellationToken);
                    jsonStrategy.Log = Log;

                    return jsonStrategy;
                }
                default:
                    throw new ArgumentOutOfRangeException("Unexpected creation mode", (Exception)null)
                        .AndFactIs("actual", _options.NewIndexStrategy);
            }

            
        }
    }
}
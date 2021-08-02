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
            switch (_options.EntityMappingMode)
            {
                case EntityMappingMode.Undefined:
                    throw new InvalidOperationException("Index creation mode not defined");
                case EntityMappingMode.Auto:
                {
                    var autoStrategy = new AutoSettingsBasedCreateIndexStrategy(_exampleEntity) { Log = Log };
                    return autoStrategy;
                }
                case EntityMappingMode.SettingsFile:
                {
                    var jsonStrategy = await JsonSettingsBasedCreateIndexStrategy.LoadFormFileAsync(_options.IndexSettingsPath, cancellationToken);
                    jsonStrategy.Log = Log;

                    return jsonStrategy;
                }
                default:
                    throw new ArgumentOutOfRangeException("Unexpected creation mode", (Exception)null)
                        .AndFactIs("actual", _options.EntityMappingMode);
            }

            
        }
    }
}
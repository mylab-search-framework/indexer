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
        private readonly NsOptions _options;
        private readonly INamespaceResourceProvider _namespaceResourceProvider;
        private readonly DataSourceEntity _exampleEntity;

        public IDslLogger Log { get; set; }

        public CreateIndexStrategyFactory(NsOptions options, INamespaceResourceProvider namespaceResourceProvider, DataSourceEntity exampleEntity)
        {
            _options = options;
            _namespaceResourceProvider = namespaceResourceProvider;
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
                    var request = await _namespaceResourceProvider.ReadFileAsync(_options.NsId, "new-index.json");

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
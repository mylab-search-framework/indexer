using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MyLab.Log;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.Tools
{
    class CreateIndexStrategyFactory
    {
        private readonly IdxOptions _options;
        private readonly IIndexResourceProvider _indexResourceProvider;
        private readonly DataSourceEntity _exampleEntity;

        public IDslLogger Log { get; set; }

        public CreateIndexStrategyFactory(IdxOptions options, IIndexResourceProvider indexResourceProvider, DataSourceEntity exampleEntity)
        {
            _options = options;
            _indexResourceProvider = indexResourceProvider;
            _exampleEntity = exampleEntity;
        }

        public async Task<ICreateIndexStrategy> CreateAsync(CancellationToken cancellationToken)
        {
            switch (_options.NewIndexStrategy)
            {
                case NewIndexStrategy.Undefined:
                    throw new InvalidOperationException("ES index creation mode not defined");
                case NewIndexStrategy.Auto:
                {
                    var autoStrategy = new AutoSettingsBasedCreateIndexStrategy(_exampleEntity) { Log = Log };
                    return autoStrategy;
                }
                case NewIndexStrategy.File:
                {
                    string request;

                    try
                    {
                        request = await _indexResourceProvider.ReadFileAsync(_options.Id, "new-index.json");
                    }
                    catch (FileNotFoundException)
                    {
                        try
                        {
                            request = await _indexResourceProvider.ReadDefaultFileAsync("new-index.json");
                        }
                        catch (FileNotFoundException)
                        {
                            throw new InvalidOperationException("New index settings file not found");
                        }
                    }

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
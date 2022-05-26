using System.IO;
using System.Threading.Tasks;
using MyLab.ApiClient.Test;
using MyLab.Db;
using MyLab.Search.EsAdapter;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.Services;
using MyLab.Search.IndexerClient;
using MyLab.TaskApp;
using Nest;
using Xunit.Abstractions;

namespace FunctionTests
{
    public partial class IndexerBehavior
    {
        private readonly ITestOutputHelper _output;
        private readonly MqFixture _mqFxt;
        private readonly TestApi<Startup, ITaskAppContract> _taskApi;
        private readonly TestApi<Startup, IIndexerApiV1> _indexApi;
        private readonly IDbManager _db;
        private readonly IEsSearcher<SearchTestEntity> _es;
        private readonly ElasticClient _esClient;

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            _taskApi.Dispose();
            _indexApi.Dispose();
            return Task.CompletedTask;
        }

        public class SearchTestEntity
        {
            [Number(Name = "Id")] public long Id { get; set; }
            [Text(Name = "Value")] public string Value { get; set; }

            [Boolean(Name = "Bool")] public bool? Bool { get; set; }
        }

        private class TestIndexResourceProvider : IIndexResourceProvider
        {
            private readonly string _explicitFilepath;
            private readonly string _defaultFilepath;

            public TestIndexResourceProvider(string explicitFilepath, string defaultFilepath)
            {
                _explicitFilepath = explicitFilepath;
                _defaultFilepath = defaultFilepath;
            }

            public Task<string> ReadFileAsync(string idxId, string filename)
            {
                return File.ReadAllTextAsync(Path.Combine("files", _explicitFilepath));
            }

            public Task<string> ReadDefaultFileAsync(string filename)
            {
                return File.ReadAllTextAsync(Path.Combine("files", _defaultFilepath));
            }
        }

        private class TestSeedService : ISeedService
        {
            private string _seed;

            public Task WriteAsync(string nsId, string seed)
            {
                _seed = seed;

                return Task.CompletedTask;
            }

            public Task<string> ReadAsync(string nsId)
            {
                return Task.FromResult(_seed);
            }
        }

        public IndexerBehavior(
            ITestOutputHelper output, 
            IndexerTestDbFixture dbFxt, 
            EsFixture<TestEsConnection> esFxt,
            MqFixture mqFxt)
        {
            _output = output;
            _mqFxt = mqFxt;

            dbFxt.Output = output;
            _db = dbFxt.Manager;

            esFxt.Output = output;
            _es = esFxt.CreateSearcher<SearchTestEntity>();

            _taskApi = new TestApi<Startup, ITaskAppContract>
            {
                Output = output
            };

            _indexApi = new TestApi<Startup, IIndexerApiV1>
            {
                Output = output
            };

            _esClient = esFxt.EsClient;
        }
    }
}
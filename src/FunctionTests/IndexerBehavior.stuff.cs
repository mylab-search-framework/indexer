using System.IO;
using System.Threading.Tasks;
using MyLab.ApiClient.Test;
using MyLab.Db;
using MyLab.Search.EsAdapter;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.Services;
using MyLab.TaskApp;
using Nest;
using Xunit.Abstractions;

namespace FunctionTests
{
    public partial class IndexerBehavior
    {
        private readonly ITestOutputHelper _output;
        private readonly MqFixture _mqFxt;
        private readonly TestApi<Startup, ITaskAppContract> _api;
        private readonly IDbManager _db;
        private readonly IEsSearcher<SearchTestEntity> _es;

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            _api.Dispose();
            return Task.CompletedTask;
        }

        public class SearchTestEntity
        {
            [Number(Name = "Id")] public long Id { get; set; }
            [Text(Name = "Value")] public string Value { get; set; }

            [Boolean(Name = "Bool")] public bool? Bool { get; set; }
        }

        private class TestJobResourceProvider : IJobResourceProvider
        {
            private readonly string _filepath;

            public TestJobResourceProvider(string filepath)
            {
                _filepath = filepath;
            }

            public Task<string> ReadFileAsync(string jobId, string filename)
            {
                return File.ReadAllTextAsync(Path.Combine("files", _filepath));
            }
        }

        private class TestSeedService : ISeedService
        {
            private string _seed;

            public Task WriteAsync(string jobId, string seed)
            {
                _seed = seed;

                return Task.CompletedTask;
            }

            public Task<string> ReadAsync(string jobId)
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

            _api = new TestApi<Startup, ITaskAppContract>
            {
                Output = output
            };
        }
    }
}
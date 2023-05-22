using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnitTests
{
    public partial class InputRequestProcessorBehavior
    {
        private readonly JObject _putEnt = JObject.FromObject(new TestDoc("put-id", "put-data"));
        private readonly JObject _patchEnt = JObject.FromObject(new TestDoc("patch-id", "patch-data"));
        private readonly JObject _kickEnt = JObject.FromObject(new TestDoc("kick-id", "kick-data"));

        private string _deleteId = "delete-id";

        private class TestDataSourceService : IDataSourceService
        {
            private readonly DataSourceLoad _load;

            public TestDataSourceService(DataSourceLoad load)
            {
                _load = load;
            }

            public Task<DataSourceLoad> LoadKickAsync(string indexId, string[] idList)
            {
                return Task.FromResult(_load);
            }

            public Task<IAsyncEnumerable<DataSourceLoad>> LoadSyncAsync(string indexId)
            {
                throw new NotImplementedException();
            }
        }

        private class TestIndexerService : IIndexerService
        {
            public IndexingRequest LastRequest { get; set; }

            public Task IndexAsync(IndexingRequest req, CancellationToken cToken)
            {
                LastRequest = req;

                return Task.CompletedTask;
            }
        }

        private class TestDoc
        {
            [JsonProperty("id")] public string Id { get; set; }
            public string Content { get; set; }

            public TestDoc()
            {
            }

            public TestDoc(string id, string content)
            {
                Id = id;
                Content = content;
            }

            public static TestDoc Generate()
            {
                return new TestDoc
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Content = Guid.NewGuid().ToString("N")
                };
            }
        }
    }
}
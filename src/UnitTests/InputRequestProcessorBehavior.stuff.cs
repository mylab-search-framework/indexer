using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnitTests
{
    public partial class InputRequestProcessorBehavior
    {
        private readonly IndexingEntity _postEnt = new IndexingEntity
        {
            Id = "post-id",
            Entity = JObject.FromObject(new TestEntity("post-id", "post-data"))
        };

        private readonly IndexingEntity _putEnt = new IndexingEntity
        {
            Id = "put-id",
            Entity = JObject.FromObject(new TestEntity("put-id", "put-data"))
        };

        private readonly IndexingEntity _patchEnt = new IndexingEntity
        {
            Id = "patch-id",
            Entity = JObject.FromObject(new TestEntity("patch-id", "patch-data"))
        };

        private readonly IndexingEntity _kickEnt = new IndexingEntity
        {
            Id = "kick-id",
            Entity = JObject.FromObject(new TestEntity("kick-id", "kick-data"))
        };

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

            public Task IndexAsync(IndexingRequest req)
            {
                LastRequest = req;

                return Task.CompletedTask;
            }
        }

        private class TestEntity
        {
            [JsonProperty("id")] public string Id { get; set; }
            public string Content { get; set; }

            public TestEntity()
            {
            }

            public TestEntity(string id, string content)
            {
                Id = id;
                Content = content;
            }

            public static TestEntity Generate()
            {
                return new TestEntity
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Content = Guid.NewGuid().ToString("N")
                };
            }
        }
    }
}
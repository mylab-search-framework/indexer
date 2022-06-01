using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using MyLab.DbTest;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests
{
    public partial class DataSourceServiceBehavior
    {
        private readonly TmpDbFixture<DbInitializer> _dbFxt;

        public DataSourceServiceBehavior(TmpDbFixture<DbInitializer> dbFxt, ITestOutputHelper output)
        {
            _dbFxt = dbFxt;
            _dbFxt.Output = output;
        }

        void AssertEntity(TestEntity entity, JObject jObject)
        {
            Assert.NotNull(jObject);
            Assert.Equal(entity.Id, jObject.Property("id").Value.Value<int>());
            Assert.Equal(entity.Content, jObject.Property("content").Value.Value<string>());
        }

        private class TestIndexResourceProvider : IIndexResourceProvider
        {
            private readonly string _kickQuery;
            private readonly string _syncQuery;

            public TestIndexResourceProvider(IndexOptions options)
            {
                _kickQuery = options.KickDbQuery;
                _syncQuery = options.SyncDbQuery;
            }

            public Task<string> ProvideKickQueryAsync(string indexId)
            {
                return Task.FromResult(_kickQuery);
            }

            public Task<string> ProvideSyncQueryAsync(string indexId)
            {
                return Task.FromResult(_syncQuery);
            }
        }

        private class TestSeedService : ISeedService
        {
            public Stack<long> IdSeeds { get; } = new();

            public Stack<DateTime> DtSeeds { get; } = new();

            public Task SaveSeedAsync(string indexId, long idSeed)
            {
                IdSeeds.Push(idSeed);

                return Task.CompletedTask;
            }

            public Task SaveSeedAsync(string indexId, DateTime dtSeed)
            {
                DtSeeds.Push(dtSeed);

                return Task.CompletedTask;
            }

            public Task<long> LoadIdSeedAsync(string indexId)
            {
                return Task.FromResult(IdSeeds.Count != 0 ? IdSeeds.Peek() : -1);
            }

            public Task<DateTime> LoadDtSeedAsync(string indexId)
            {
                return Task.FromResult(DtSeeds.Count != 0 ? DtSeeds.Peek() : DateTime.MinValue);
            }
        }

        public class DbInitializer : ITestDbInitializer
        {
            public async Task InitializeAsync(DataConnection dc)
            {
                await dc.CreateTableAsync<TestEntity>();
            }
        }

        class TableFiller : ITestDbInitializer
        {
            private readonly TestEntity[] _initialEntities;

            public TableFiller(TestEntity[] initialEntities)
            {
                _initialEntities = initialEntities;
            }
            public async Task InitializeAsync(DataConnection dc)
            {
                await dc.BulkCopyAsync(_initialEntities);
            }
        }

        [Table("entities")]
        private class TestEntity
        {
            [PrimaryKey, Column("id")] public int Id { get; set; }
            [Column("content")] public string Content { get; set; }
            [Column("last_change_dt")] public DateTime LastChangeDt { get; set; }
        }
    }
}
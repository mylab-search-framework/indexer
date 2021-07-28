using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.Db;
using MyLab.DbTest;
using MyLab.Search.Indexer;
using MyLab.TaskApp;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class IndexerTaskLogicBehavior : IClassFixture<TmpDbFixture>
    {
        private readonly TmpDbFixture _dvFxt;
        private readonly ITestOutputHelper _output;

        public IndexerTaskLogicBehavior(TmpDbFixture dvFxt, ITestOutputHelper output)
        {
            dvFxt.Output = output;
            _dvFxt = dvFxt;
            _output = output;
        }

        [Fact]
        public async Task ShouldIndexFullWhenNoSeed()
        {
            //Arrange
            var testDb = await _dvFxt.CreateDbAsync(new FiveInserter());
            var indexer = new TestIndexer();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IDbManager>(testDb)
                .AddSingleton<IDataSourceService, DbDataSourceService>()
                .AddSingleton<ISeedService, TestSeedService>()
                .AddSingleton<IDataIndexer>(indexer)
                .AddSingleton<ITaskLogic, IndexerTaskLogic>()
                .Configure<IndexerOptions>(o => o.PageSize = 2)
                .Configure<IndexerOptions>(o =>
                    o.Query = "select * from foo_table limit {limit} offset {offset}")
                .AddLogging(l => l.AddXUnit(_output).SetMinimumLevel(LogLevel.Debug))
                .BuildServiceProvider();
            
            var logic = (ITaskLogic)serviceProvider.GetService(typeof(ITaskLogic));

            //Act
            await logic.Perform(CancellationToken.None);

            //Assert
            Assert.Equal(5, indexer.IndexedEntities.Count);
            CheckIndexedEntity("0", "0");
            CheckIndexedEntity("1", "1");
            CheckIndexedEntity("2", "2");
            CheckIndexedEntity("3", "3");
            CheckIndexedEntity("4", "4");

            void CheckIndexedEntity(string id, string expectedValue)
            {
                Assert.Equal(expectedValue, indexer.IndexedEntities[id].Properties[nameof(TestEntity.Value)]);
            }
        }

        class TestEntity
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        class FiveInserter : ITestDbInitializer
        {
            public async Task InitializeAsync(DataConnection dataConnection)
            {
                var t = await dataConnection.CreateTableAsync<TestEntity>("foo_table");
                await t.BulkCopyAsync(Enumerable.Repeat<TestEntity>(null, 5).Select((entity, i) => new TestEntity { Id = i, Value = i.ToString() }));
            }
        }

        class TestSeedService : ISeedService
        {
            private long _seed;
            public Task WriteAsync(long seed)
            {
                _seed = seed;

                return Task.CompletedTask;
            }

            public Task<long> ReadAsync()
            {
                return Task.FromResult(_seed);
            }
        }

        class TestIndexer : IDataIndexer
        {
            public Dictionary<string, DataSourceEntity> IndexedEntities { get; } = new Dictionary<string, DataSourceEntity>();

            public Task IndexAsync(DataSourceEntity[] dataSourceEntities)
            {
                foreach (var entity in dataSourceEntities)
                {
                    IndexedEntities.Add(entity.Properties[nameof(TestEntity.Id)], entity);
                }
                return Task.CompletedTask;
            }
        }
    }
}

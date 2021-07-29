using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
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

        async Task<IServiceProvider> InitServices(Action<IndexerOptions> configureOptions)
        {
            var testDb = await _dvFxt.CreateDbAsync(new FiveInserter());

            return new ServiceCollection()
                .AddSingleton<IDbManager>(testDb)
                .AddSingleton<IDataSourceService, DbDataSourceService>()
                .AddSingleton<ISeedService, TestSeedService>()
                .AddSingleton<IDataIndexer, TestIndexer>()
                .AddSingleton<ITaskLogic, IndexerTaskLogic>()
                .Configure(configureOptions)
                .AddLogging(l => l.AddXUnit(_output).SetMinimumLevel(LogLevel.Debug))
                .BuildServiceProvider();
        }

        [Fact]
        public async Task ShouldIndexFullWhenNoSeed()
        {
            //Arrange
            var sp = await InitServices(o =>
                {
                    o.PageSize = 2;
                    o.EnablePaging = true;
                    o.Query = "select * from foo_table limit @limit offset @offset";
                });
            
            var logic = sp.GetService<ITaskLogic>();
            var indexer = (TestIndexer)sp.GetService<IDataIndexer>();

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

        [Fact]
        public async Task ShouldUpdateSeed()
        {
            //Arrange
            var lastModified = DateTime.Now;

            var sp = await InitServices(o =>
                {
                    o.PageSize = 2;
                    o.Query = "select * from foo_table limit @limit offset @offset";
                    o.EnablePaging = true;
                    o.LastModifiedFieldName = nameof(TestEntity.LastModified);
                });

            var logic = sp.GetService<ITaskLogic>();
            var dbManager = sp.GetService<IDbManager>();
            var seedService= sp.GetService<ISeedService>();

            var updatedCount = await dbManager.DoOnce()
                .GetTable<TestEntity>()
                .Where(e => e.Id == 2)
                .Set(e => e.LastModified, lastModified)
                .UpdateAsync();

            _output.WriteLine("Updated count: {0}", updatedCount);

            //Act
            await logic.Perform(CancellationToken.None);

            var actualSeed = await seedService.ReadAsync();

            //Assert
            Assert.Equal(lastModified, actualSeed);
        }

        [Fact]
        public async Task ShouldIndexLastModified()
        {
            //Arrange
            var lastModified = DateTime.Now;

            var sp = await InitServices(o =>
            {
                o.Query = "select * from foo_table where LastModified > @seed";
            });

            var logic = sp.GetService<ITaskLogic>();
            var dbManager = sp.GetService<IDbManager>();
            var seedService = sp.GetService<ISeedService>();
            var indexer = (TestIndexer)sp.GetService<IDataIndexer>();

            var updatedCount = await dbManager.DoOnce()
                .GetTable<TestEntity>()
                .Where(e => e.Id == 2)
                .Set(e => e.LastModified, lastModified)
                .UpdateAsync();

            _output.WriteLine("Updated count: {0}", updatedCount);


            await seedService.WriteAsync(lastModified.AddSeconds(-1));

            //Act
            await logic.Perform(CancellationToken.None);
            
            //Assert
            Assert.Single(indexer.IndexedEntities);
            Assert.True(indexer.IndexedEntities.ContainsKey("2"));
        }

        [Table("foo_table")]
        class TestEntity
        {
            [Column]

            public int Id { get; set; }
            [Column]

            public string Value { get; set; }
            [Column]
            public DateTime LastModified { get; set; } = DateTime.MinValue;
        }

        class FiveInserter : ITestDbInitializer
        {
            public async Task InitializeAsync(DataConnection dataConnection)
            {
                var t = await dataConnection.CreateTableAsync<TestEntity>();
                await t.BulkCopyAsync(Enumerable.Repeat<TestEntity>(null, 5).Select((entity, i) => new TestEntity { Id = i, Value = i.ToString() }));
            }
        }

        class TestSeedService : ISeedService
        {
            private DateTime _seed;

            public TestSeedService()
                : this(DateTime.MinValue)
            {
                
            }

            public TestSeedService(DateTime seed)
            {
                _seed = seed;
            }

            public Task WriteAsync(DateTime seed)
            {
                _seed = seed;

                return Task.CompletedTask;
            }

            public Task<DateTime> ReadAsync()
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

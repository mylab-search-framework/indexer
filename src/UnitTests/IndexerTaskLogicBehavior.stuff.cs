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
using MyLab.Search.EsAdapter;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Tools;
using MyLab.TaskApp;
using Xunit.Abstractions;

namespace UnitTests
{
    public partial class IndexerTaskLogicBehavior
    {
        private readonly TmpDbFixture _dvFxt;
        private readonly ITestOutputHelper _output;

        public IndexerTaskLogicBehavior(TmpDbFixture dvFxt, ITestOutputHelper output)
        {
            dvFxt.Output = output;
            _dvFxt = dvFxt;
            _output = output;
        }

        private async Task<IServiceProvider> InitServices(Action<IndexerDbOptions> configureDbOptions, Action<IndexerOptions> configureIndexerOptions = null)
        {
            var testDb = await _dvFxt.CreateDbAsync(new FiveInserter());

            var srvs = new ServiceCollection()
                .AddSingleton<IDbManager>(testDb)
                .AddSingleton<IDataSourceService, DbDataSourceService>()
                .AddSingleton<ISeedService, TestSeedService>()
                .AddSingleton<IDataIndexer, TestIndexer>()
                .AddSingleton<ITaskLogic, IndexerTaskLogic>()
                .Configure(configureDbOptions)
                .Configure<ElasticsearchOptions>(o =>
                {
                    o.DefaultIndex = "[for validation]";
                    o.Url = "[for validation]";
                })
                .Configure<IndexerDbOptions>(o =>
                {
                    o.Provider = "[for validation]";
                })
                .AddLogging(l => l.AddXUnit(_output).SetMinimumLevel(LogLevel.Debug));

            if(configureIndexerOptions != null)
                srvs = srvs.Configure(configureIndexerOptions);
            else
            {
                srvs = srvs.Configure<IndexerOptions>(o =>
                {
                    o.IdProperty = "[for validation]";
                });
            }
            
            return srvs.BuildServiceProvider();
        }

        [Table("foo_table")]
        private class TestEntity
        {
            [Column] public int Id { get; set; }
            [Column] public string Value { get; set; }
            [Column] public DateTime LastModified { get; set; } = DateTime.MinValue;
        }

        private class FiveInserter : ITestDbInitializer
        {
            public async Task InitializeAsync(DataConnection dataConnection)
            {
                var t = await dataConnection.CreateTableAsync<TestEntity>();
                await t.BulkCopyAsync(Enumerable.Repeat<TestEntity>(null, 5)
                    .Select((entity, i) => new TestEntity {Id = i, Value = i.ToString()}));
            }
        }

        private class TestSeedService : ISeedService
        {
            private string _seed;

            public TestSeedService()
            {
            }

            public Task WriteAsync(string seed)
            {
                _seed = seed;

                return Task.CompletedTask;
            }

            public Task<string> ReadAsync()
            {
                return Task.FromResult(_seed);
            }
        }

        private class TestIndexer : IDataIndexer
        {
            public Dictionary<string, DataSourceEntity> IndexedEntities { get; } =
                new Dictionary<string, DataSourceEntity>();

            public Task IndexAsync(DataSourceEntity[] dataSourceEntities, CancellationToken cancellationToken)
            {
                foreach (var entity in dataSourceEntities)
                {
                    IndexedEntities.Add(entity.Properties[nameof(TestEntity.Id)].Value, entity);
                }

                return Task.CompletedTask;
            }
        }

        private Task<int> UpdateLastModified(IDbManager db, long id, DateTime newLastModified)
        {
            return db.DoOnce()
                .GetTable<TestEntity>()
                .Where(e => e.Id == id)
                .Set(e => e.LastModified, newLastModified)
                .UpdateAsync();
        }

        private Task SaveSeed(IServiceProvider serviceProvider, Func<ISeedService, Task> act)
        {
            return act(serviceProvider.GetService<ISeedService>());
        }
    }
}
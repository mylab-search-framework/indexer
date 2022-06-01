using System;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;
using LinqToDB.Mapping;
using Microsoft.Extensions.Options;
using MyLab.DbTest;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests
{
    public partial class DataSourceServiceBehavior : IClassFixture<TmpDbFixture<DataSourceServiceBehavior.DbInitializer>>
    {
        [Fact]
        public async Task ShouldNotLoadFromStreamIfEmpty()
        {
            //Arrange
            var dbMgr = await _dbFxt.CreateDbAsync();

            var seedSrv = new TestSeedService();

            var indexOpts = new IndexOptions
            {
                Id = "foo-index",
                IsStream = true,
                SyncDbQuery = "select id, content from entities where id > @seed"
            };

            var options = new IndexerOptions
                {
                    Indexes = new []
                    {
                        indexOpts
                    }
                };

            var indexResProvider = new TestIndexResourceProvider(indexOpts);

            IDataSourceService srv = new DataSourceService(dbMgr, seedSrv, indexResProvider, options);

            var enumerable = await srv.LoadSyncAsync("foo-index");

            //Act
            var loads = await enumerable.ToArrayAsync();

            //Assert
            Assert.Empty(loads);
        }

        [Fact]
        public async Task ShouldLoadDataFromStream()
        {
            //Arrange

            var ent0 = new TestEntity { Id = 0, Content = "0-content", LastChangeDt = DateTime.Now };
            var ent1 = new TestEntity { Id = 1, Content = "1-content", LastChangeDt = DateTime.Now };

            var tableFiller = new TableFiller(new [] { ent0, ent1 });

            var dbMgr = await _dbFxt.CreateDbAsync(tableFiller);
            
            var seedSrv = new TestSeedService();

            var indexOpts = new IndexOptions
            {
                Id = "foo-index",
                IsStream = true,
                SyncDbQuery = "select id, content from entities where id > @seed limit @offset, @limit",
                SyncPageSize = 1
            };

            var options = new IndexerOptions
            {
                Indexes = new[]
                {
                    indexOpts
                }
            };

            var indexResProvider = new TestIndexResourceProvider(indexOpts);

            IDataSourceService srv = new DataSourceService(dbMgr, seedSrv, indexResProvider, options);

            var enumerable = await srv.LoadSyncAsync("foo-index");

            //Act
            var loads = await enumerable.ToArrayAsync();

            //Assert
            Assert.Equal(2, loads.Length);
            Assert.Single(loads[0].Batch.Entities);
            Assert.Equal("0", loads[0].Batch.Entities[0].Id);

            AssertEntity(ent0, loads[0].Batch.Entities[0].Entity);
            AssertEntity(ent1, loads[1].Batch.Entities[0].Entity);
        }

        [Fact]
        public async Task ShouldNotLoadFromNonStreamIfEmpty()
        {
            //Arrange
            var dbMgr = await _dbFxt.CreateDbAsync();
            var seedSrv = new TestSeedService();
            var indexOpts = new IndexOptions
            {
                Id = "foo-index",
                IsStream = false,
                SyncDbQuery = "select id, content from entities where last_change_dt > @seed"
            };
            var options = new IndexerOptions
            {
                Indexes = new[]
                {
                    indexOpts
                }
            };

            var indexResProvider = new TestIndexResourceProvider(indexOpts);

            IDataSourceService srv = new DataSourceService(dbMgr, seedSrv, indexResProvider, options);

            var enumerable = await srv.LoadSyncAsync("foo-index");

            //Act
            var loads = await enumerable.ToArrayAsync();

            //Assert
            Assert.Empty(loads);
        }
    }

    
}

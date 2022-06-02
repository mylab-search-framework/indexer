using System.Threading.Tasks;
using LinqToDB.Async;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using Xunit;

namespace UnitTests
{
    public partial class DbDataSourceServiceBehavior
    {
        [Fact]
        public async Task ShouldProvideSingleKickedEntityByIntField()
        {
            //Arrange

            var ent0 = new TestEntity { Id = 0, Content = "0-content" };
            var ent1 = new TestEntity { Id = 1, Content = "1-content" };

            var tableFiller = new TableFiller(new[] { ent0, ent1 });

            var dbMgr = await _dbFxt.CreateDbAsync(tableFiller);

            var seedSrv = new TestSeedService();

            var indexOpts = new IndexOptions 
            {
                Id = "foo-index",
                KickDbQuery = "select id, content from entities where id in (@id)",
                IdPropertyType = IdPropertyType.Int
            };

            var options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var indexResProvider = new TestIndexResourceProvider(indexOpts);

            IDataSourceService srv = new DbDataSourceService(dbMgr, seedSrv, indexResProvider, options);

            //Act
            var load = await srv.LoadKickAsync("foo-index", new[] { "1" });

            //Assert
            Assert.Single(load.Batch.Entities);
            Assert.Equal("1", load.Batch.Entities[0].Id);
            AssertEntity(ent1, load.Batch.Entities[0].Entity);
        }

        [Fact]
        public async Task ShouldProvideMultipleKickedEntitiesByIntField()
        {
            //Arrange

            var ent0 = new TestEntity { Id = 0, Content = "0-content" };
            var ent1 = new TestEntity { Id = 1, Content = "1-content" };
            var ent2 = new TestEntity { Id = 2, Content = "2-content" };

            var tableFiller = new TableFiller(new[] { ent0, ent1, ent2 });

            var dbMgr = await _dbFxt.CreateDbAsync(tableFiller);

            var seedSrv = new TestSeedService();

            var indexOpts = new IndexOptions
            {
                Id = "foo-index",
                KickDbQuery = "select id, content from entities where id in (@id)",
                IdPropertyType = IdPropertyType.Int
            };

            var options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var indexResProvider = new TestIndexResourceProvider(indexOpts);

            IDataSourceService srv = new DbDataSourceService(dbMgr, seedSrv, indexResProvider, options);

            //Act
            var load = await srv.LoadKickAsync("foo-index", new[] { "0", "2" });

            //Assert
            Assert.Equal(2, load.Batch.Entities.Length);
            Assert.Equal("0", load.Batch.Entities[0].Id);
            AssertEntity(ent0, load.Batch.Entities[0].Entity);
            Assert.Equal("2", load.Batch.Entities[1].Id);
            AssertEntity(ent2, load.Batch.Entities[1].Entity);
        }

        [Fact]
        public async Task ShouldProvideSingleKickedEntityByStringField()
        {
            //Arrange

            var ent0 = new TestEntity { Id = 0, Content = "0-content" };
            var ent1 = new TestEntity { Id = 1, Content = "1-content" };

            var tableFiller = new TableFiller(new[] { ent0, ent1 });

            var dbMgr = await _dbFxt.CreateDbAsync(tableFiller);

            var seedSrv = new TestSeedService();

            var indexOpts = new IndexOptions
            {
                Id = "foo-index",
                KickDbQuery = "select id, content from entities where content in (@id)",
                IdPropertyType = IdPropertyType.Int
            };

            var options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var indexResProvider = new TestIndexResourceProvider(indexOpts);

            IDataSourceService srv = new DbDataSourceService(dbMgr, seedSrv, indexResProvider, options);

            //Act
            var load = await srv.LoadKickAsync("foo-index", new[] { "1-content" });

            //Assert
            Assert.Single(load.Batch.Entities);
            Assert.Equal("1", load.Batch.Entities[0].Id);
            AssertEntity(ent1, load.Batch.Entities[0].Entity);
        }

        [Fact]
        public async Task ShouldProvideMultipleKickedEntitiesByStringField()
        {
            //Arrange

            var ent0 = new TestEntity { Id = 0, Content = "0-content" };
            var ent1 = new TestEntity { Id = 1, Content = "1-content" };
            var ent2 = new TestEntity { Id = 2, Content = "2-content" };

            var tableFiller = new TableFiller(new[] { ent0, ent1, ent2 });

            var dbMgr = await _dbFxt.CreateDbAsync(tableFiller);

            var seedSrv = new TestSeedService();

            var indexOpts = new IndexOptions
            {
                Id = "foo-index",
                KickDbQuery = "select id, content from entities where content in (@id)",
                IdPropertyType = IdPropertyType.Int
            };

            var options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var indexResProvider = new TestIndexResourceProvider(indexOpts);

            IDataSourceService srv = new DbDataSourceService(dbMgr, seedSrv, indexResProvider, options);

            //Act
            var load = await srv.LoadKickAsync("foo-index", new[] { "0-content", "2-content" });

            //Assert
            Assert.Equal(2, load.Batch.Entities.Length);
            Assert.Equal("0", load.Batch.Entities[0].Id);
            AssertEntity(ent0, load.Batch.Entities[0].Entity);
            Assert.Equal("2", load.Batch.Entities[1].Id);
            AssertEntity(ent2, load.Batch.Entities[1].Entity);
        }
    }
}
using System.Threading.Tasks;
using LinqToDB.Async;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Tools;
using Xunit;

namespace UnitTests
{
    public partial class DbDataSourceServiceBehavior
    {
        [Fact]
        public async Task ShouldProvideSingleKickedDocByIntField()
        {
            //Arrange

            var ent0 = new TestDoc { Id = 0, Content = "0-content" };
            var ent1 = new TestDoc { Id = 1, Content = "1-content" };

            var tableFiller = new TableFiller(new[] { ent0, ent1 });

            var dbMgr = await _dbFxt.CreateDbAsync(tableFiller);

            var seedSrv = new TestSeedService();

            var indexOpts = new IndexOptions 
            {
                Id = "foo-index",
                KickDbQuery = "select id, content from docs where id in (@id)",
                IdPropertyType = IdPropertyType.Int
            };

            var options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var indexResProvider = new TestResourceProvider(indexOpts);

            IDataSourceService srv = new DbDataSourceService(dbMgr, seedSrv, indexResProvider, options);

            //Act
            var load = await srv.LoadKickAsync("foo-index", new[] { "1" });

            //Assert
            Assert.Single(load.Batch.Docs);
            AssertDoc(ent1, load.Batch.Docs[0]);
        }

        [Fact]
        public async Task ShouldProvideMultipleKickedDocsByIntField()
        {
            //Arrange

            var ent0 = new TestDoc { Id = 0, Content = "0-content" };
            var ent1 = new TestDoc { Id = 1, Content = "1-content" };
            var ent2 = new TestDoc { Id = 2, Content = "2-content" };

            var tableFiller = new TableFiller(new[] { ent0, ent1, ent2 });

            var dbMgr = await _dbFxt.CreateDbAsync(tableFiller);

            var seedSrv = new TestSeedService();

            var indexOpts = new IndexOptions
            {
                Id = "foo-index",
                KickDbQuery = "select id, content from docs where id in (@id)",
                IdPropertyType = IdPropertyType.Int
            };

            var options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var indexResProvider = new TestResourceProvider(indexOpts);

            IDataSourceService srv = new DbDataSourceService(dbMgr, seedSrv, indexResProvider, options);

            //Act
            var load = await srv.LoadKickAsync("foo-index", new[] { "0", "2" });

            //Assert
            Assert.Equal(2, load.Batch.Docs.Length);
            AssertDoc(ent0, load.Batch.Docs[0]);
            AssertDoc(ent2, load.Batch.Docs[1]);
        }

        [Fact]
        public async Task ShouldProvideSingleKickedDocByStringField()
        {
            //Arrange

            var ent0 = new TestDoc { Id = 0, Content = "0-content" };
            var ent1 = new TestDoc { Id = 1, Content = "1-content" };

            var tableFiller = new TableFiller(new[] { ent0, ent1 });

            var dbMgr = await _dbFxt.CreateDbAsync(tableFiller);

            var seedSrv = new TestSeedService();

            var indexOpts = new IndexOptions
            {
                Id = "foo-index",
                KickDbQuery = "select id, content from docs where content in (@id)",
                IdPropertyType = IdPropertyType.Int
            };

            var options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var indexResProvider = new TestResourceProvider(indexOpts);

            IDataSourceService srv = new DbDataSourceService(dbMgr, seedSrv, indexResProvider, options);

            //Act
            var load = await srv.LoadKickAsync("foo-index", new[] { "1-content" });

            //Assert
            Assert.Single(load.Batch.Docs);
            AssertDoc(ent1, load.Batch.Docs[0]);
        }

        [Fact]
        public async Task ShouldProvideMultipleKickedDocsByStringField()
        {
            //Arrange

            var ent0 = new TestDoc { Id = 0, Content = "0-content" };
            var ent1 = new TestDoc { Id = 1, Content = "1-content" };
            var ent2 = new TestDoc { Id = 2, Content = "2-content" };

            var tableFiller = new TableFiller(new[] { ent0, ent1, ent2 });

            var dbMgr = await _dbFxt.CreateDbAsync(tableFiller);

            var seedSrv = new TestSeedService();

            var indexOpts = new IndexOptions
            {
                Id = "foo-index",
                KickDbQuery = "select id, content from docs where content in (@id)",
                IdPropertyType = IdPropertyType.Int
            };

            var options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var indexResProvider = new TestResourceProvider(indexOpts);

            IDataSourceService srv = new DbDataSourceService(dbMgr, seedSrv, indexResProvider, options);

            //Act
            var load = await srv.LoadKickAsync("foo-index", new[] { "0-content", "2-content" });

            //Assert
            Assert.Equal(2, load.Batch.Docs.Length);
            AssertDoc(ent0, load.Batch.Docs[0]);
            AssertDoc(ent2, load.Batch.Docs[1]);
        }
    }
}
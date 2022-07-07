using System;
using System.Threading.Tasks;
using LinqToDB.Async;
using MyLab.DbTest;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Tools;
using Xunit;

namespace UnitTests
{
    public partial class DbDataSourceServiceBehavior : IClassFixture<TmpDbFixture<DbDataSourceServiceBehavior.DbInitializer>>
    {
        [Fact]
        public async Task ShouldNotLoadSyncFromStreamIfEmpty()
        {
            //Arrange
            var dbMgr = await _dbFxt.CreateDbAsync();

            var seedSrv = new TestSeedService();

            var indexOpts = new IndexOptions
            {
                Id = "foo-index",
                IndexType = IndexType.Stream,
                SyncDbQuery = "select id, content from docs where id > @seed"
            };

            var options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var indexResProvider = new TestIndexResourceProvider(indexOpts);

            IDataSourceService srv = new DbDataSourceService(dbMgr, seedSrv, indexResProvider, options);

            var enumerable = await srv.LoadSyncAsync("foo-index");

            //Act
            var loads = await enumerable.ToArrayAsync();

            //Assert
            Assert.Empty(loads);
        }

        [Fact]
        public async Task ShouldNotLoadSyncFromHeapIfEmpty()
        {
            //Arrange
            var dbMgr = await _dbFxt.CreateDbAsync();
            var seedSrv = new TestSeedService();
            var indexOpts = new IndexOptions
            {
                Id = "foo-index",
                IndexType = IndexType.Heap,
                SyncDbQuery = "select id, content from docs where last_change_dt > @seed"
            };
            var options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var indexResProvider = new TestIndexResourceProvider(indexOpts);

            IDataSourceService srv = new DbDataSourceService(dbMgr, seedSrv, indexResProvider, options);

            var enumerable = await srv.LoadSyncAsync("foo-index");

            //Act
            var loads = await enumerable.ToArrayAsync();

            //Assert
            Assert.Empty(loads);
        }

        [Fact]
        public async Task ShouldLoadAllSyncDataFromStream()
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
                IndexType = IndexType.Stream,
                SyncDbQuery = "select id, content from docs where id > @seed limit @offset, @limit",
                SyncPageSize = 1
            };

            var options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var indexResProvider = new TestIndexResourceProvider(indexOpts);

            IDataSourceService srv = new DbDataSourceService(dbMgr, seedSrv, indexResProvider, options);

            var enumerable = await srv.LoadSyncAsync("foo-index");

            //Act
            var loads = await enumerable.ToArrayAsync();

            //Assert
            Assert.Equal(2, loads.Length);
            Assert.Single(loads[0].Batch.Docs);
            Assert.Equal("0", loads[0].Batch.Docs[0].GetIdProperty());

            AssertDoc(ent0, loads[0].Batch.Docs[0]);
            AssertDoc(ent1, loads[1].Batch.Docs[0]);
        }

        [Fact]
        public async Task ShouldLoadAllSyncDataFromHeap()
        {
            //Arrange

            var ent0 = new TestDoc { Id = 0, Content = "0-content", LastChangeDt = DateTime.Now };
            var ent1 = new TestDoc { Id = 1, Content = "1-content", LastChangeDt = DateTime.Now };

            var tableFiller = new TableFiller(new[] { ent0, ent1 });

            var dbMgr = await _dbFxt.CreateDbAsync(tableFiller);

            var seedSrv = new TestSeedService();

            var indexOpts = new IndexOptions
            {
                Id = "foo-index",
                IndexType = IndexType.Heap,
                SyncDbQuery = "select id, content from docs where last_change_dt > @seed limit @offset, @limit",
                SyncPageSize = 1
            };

            var options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var indexResProvider = new TestIndexResourceProvider(indexOpts);

            IDataSourceService srv = new DbDataSourceService(dbMgr, seedSrv, indexResProvider, options);

            var enumerable = await srv.LoadSyncAsync("foo-index");

            //Act
            var loads = await enumerable.ToArrayAsync();

            //Assert
            Assert.Equal(2, loads.Length);
            Assert.Single(loads[0].Batch.Docs);
            Assert.Equal("0", loads[0].Batch.Docs[0].GetIdProperty());

            AssertDoc(ent0, loads[0].Batch.Docs[0]);
            AssertDoc(ent1, loads[1].Batch.Docs[0]);
        }

        [Fact]
        public async Task ShouldLoadDeltaSyncFromStream()
        {
            //Arrange
            const long lastProcessedDocId = 0;

            var ent0 = new TestDoc { Id = lastProcessedDocId, Content = "0-content" };
            var ent1 = new TestDoc { Id = 1, Content = "1-content" };

            var tableFiller = new TableFiller(new[] { ent0, ent1 });

            var dbMgr = await _dbFxt.CreateDbAsync(tableFiller);

            var seedSrv = new TestSeedService();

            await seedSrv.SaveSeedAsync("foo-index", lastProcessedDocId);

            var indexOpts = new IndexOptions
            {
                Id = "foo-index",
                IndexType = IndexType.Stream,
                SyncDbQuery = "select id, content from docs where id > @seed limit @offset, @limit",
                SyncPageSize = 1
            };

            var options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var indexResProvider = new TestIndexResourceProvider(indexOpts);

            IDataSourceService srv = new DbDataSourceService(dbMgr, seedSrv, indexResProvider, options);

            var enumerable = await srv.LoadSyncAsync("foo-index");

            //Act
            var loads = await enumerable.ToArrayAsync();

            //Assert
            Assert.Single(loads);
            Assert.Single(loads[0].Batch.Docs);
            Assert.Equal("1", loads[0].Batch.Docs[0].GetIdProperty());
            AssertDoc(ent1, loads[0].Batch.Docs[0]);
        }

        [Fact]
        public async Task ShouldLoadDeltaSyncFromHeap()
        {
            //Arrange
            var lastIndexedDt = DateTime.Now;

            var ent0 = new TestDoc { Id = 0, Content = "0-content", LastChangeDt = lastIndexedDt.AddMinutes(-1) };
            var ent1 = new TestDoc { Id = 1, Content = "1-content", LastChangeDt = lastIndexedDt.AddMinutes(1) };

            var tableFiller = new TableFiller(new[] { ent0, ent1 });

            var dbMgr = await _dbFxt.CreateDbAsync(tableFiller);

            var seedSrv = new TestSeedService();

            await seedSrv.SaveSeedAsync("foo-index", lastIndexedDt);

            var indexOpts = new IndexOptions
            {
                Id = "foo-index",
                IndexType = IndexType.Heap,
                SyncDbQuery = "select id, content from docs where last_change_dt > @seed limit @offset, @limit",
                SyncPageSize = 1
            };

            var options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var indexResProvider = new TestIndexResourceProvider(indexOpts);

            IDataSourceService srv = new DbDataSourceService(dbMgr, seedSrv, indexResProvider, options);

            var enumerable = await srv.LoadSyncAsync("foo-index");

            //Act
            var loads = await enumerable.ToArrayAsync();

            //Assert
            Assert.Single(loads);
            Assert.Single(loads[0].Batch.Docs);
            Assert.Equal("1", loads[0].Batch.Docs[0].GetIdProperty());
            AssertDoc(ent1, loads[0].Batch.Docs[0]);
        }
    }

    
}

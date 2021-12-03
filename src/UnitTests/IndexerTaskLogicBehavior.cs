using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MyLab.Db;
using MyLab.DbTest;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Services;
using MyLab.TaskApp;
using Xunit;

namespace UnitTests
{
    public partial class IndexerTaskLogicBehavior : IClassFixture<TmpDbFixture>
    {
        [Fact]
        public async Task ShouldNotIndexLastChangeProperty()
        {
            //Arrange
            var lastModified = DateTime.Now;

            var sp = await InitServices(o =>
            {
                o.Namespaces = new[]
                {
                    new NsOptions
                    {
                        NsId = "foojob",
                        LastChangeProperty = nameof(TestEntity.LastModified),
                        IdPropertyName = nameof(TestEntity.Id),
                        SyncDbQuery = "select * from foo_table where LastModified > @seed",
                        NewUpdatesStrategy = NewUpdatesStrategy.Update,
                        EsIndex = "[no mater in this test]"
                    }
                };
            });

            var logic = sp.GetService<ITaskLogic>();
            var dbManager = sp.GetService<IDbManager>();
            var seedService = sp.GetService<ISeedService>();
            var indexer = (TestIndexer)sp.GetService<IDataIndexer>();

            var updatedCount = await UpdateLastModified(dbManager, 2, lastModified);

            _output.WriteLine("Updated count: {0}", updatedCount);


            await seedService.WriteDateTimeAsync("foojob", lastModified.AddSeconds(-1));

            //Act
            await logic.Perform(CancellationToken.None);

            indexer.IndexedEntities.TryGetValue("2", out var entity);
            
            //Assert
            Assert.NotNull(entity);
            Assert.False(entity.Properties.ContainsKey(nameof(TestEntity.LastModified)));
        }

        [Fact]
        public async Task ShouldIndexFullWhenNoSeed()
        {
            //Arrange
            var sp = await InitServices(
                o =>
                {
                    o.Namespaces = new[]
                    {
                        new NsOptions
                        {
                            NsId = "foojob",
                            LastChangeProperty = nameof(TestEntity.LastModified),
                            IdPropertyName = nameof(TestEntity.Id),
                            PageSize = 2,
                            EnablePaging = true,
                            SyncDbQuery = "select * from foo_table limit @limit offset @offset",
                            NewUpdatesStrategy = NewUpdatesStrategy.Update,
                            EsIndex = "[no mater in this test]"
                        }
                    };
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
                Assert.Equal(expectedValue, indexer.IndexedEntities[id].Properties[nameof(TestEntity.Value)].Value);
            }
        }

        [Fact]
        public async Task ShouldUpdateLastModifiedSeed()
        {
            //Arrange
            var lastModified = DateTime.Now;

            var sp = await InitServices(o =>
                {
                    o.Namespaces = new[]
                    {
                        new NsOptions
                        {
                            NsId = "foojob",
                            LastChangeProperty = nameof(TestEntity.LastModified),
                            IdPropertyName = nameof(TestEntity.Id),
                            PageSize = 2,
                            EnablePaging = true,
                            SyncDbQuery = "select * from foo_table limit @limit offset @offset",
                            NewUpdatesStrategy = NewUpdatesStrategy.Update,
                            EsIndex = "[no mater in this test]"
                        }
                    };
                });

            var logic = sp.GetService<ITaskLogic>();
            var dbManager = sp.GetService<IDbManager>();
            var seedService= sp.GetService<ISeedService>();

            var updatedCount = await UpdateLastModified(dbManager, 2, lastModified);

            _output.WriteLine("Updated count: {0}", updatedCount);

            //Act
            await logic.Perform(CancellationToken.None);

            var actualSeed = await seedService.ReadDateTimeAsync("foojob");

            //Assert
            Assert.Equal(lastModified, actualSeed);
        }

        [Fact]
        public async Task ShouldUpdateIdSeed()
        {
            //Arrange
            var sp = await InitServices(o =>
                {
                    o.Namespaces = new[]
                    {
                        new NsOptions
                        {
                            NsId = "foojob",
                            IdPropertyName = nameof(TestEntity.Id),
                            PageSize = 2,
                            EnablePaging = true,
                            SyncDbQuery = "select * from foo_table limit @limit offset @offset",
                            NewUpdatesStrategy = NewUpdatesStrategy.Add,
                            EsIndex = "[no mater in this test]"
                        }
                    };
                });

            var logic = sp.GetService<ITaskLogic>();
            var seedService = sp.GetService<ISeedService>();
            
            //Act
            await logic.Perform(CancellationToken.None);

            var actualSeed = await seedService.ReadIdAsync("foojob");

            //Assert
            Assert.Equal(4, actualSeed);
        }

        [Fact]
        public async Task ShouldIndexLastModified()
        {
            //Arrange
            var lastModified = DateTime.Now;

            var sp = await InitServices(o =>
            {
                o.Namespaces = new[]
                {
                    new NsOptions
                    {
                        NsId = "foojob",
                        LastChangeProperty = nameof(TestEntity.LastModified),
                        IdPropertyName = nameof(TestEntity.Id),
                        SyncDbQuery = "select * from foo_table where LastModified > @seed",
                        NewUpdatesStrategy = NewUpdatesStrategy.Update,
                        EsIndex = "[no mater in this test]"
                    }
                };
            });

            var logic = sp.GetService<ITaskLogic>();
            var dbManager = sp.GetService<IDbManager>();
            var seedService = sp.GetService<ISeedService>();
            var indexer = (TestIndexer)sp.GetService<IDataIndexer>();

            var updatedCount = await UpdateLastModified(dbManager, 2, lastModified);

            _output.WriteLine("Updated count: {0}", updatedCount);


            await seedService.WriteDateTimeAsync("foojob", lastModified.AddSeconds(-1));

            //Act
            await logic.Perform(CancellationToken.None);
            
            //Assert
            Assert.Single(indexer.IndexedEntities);
            Assert.True(indexer.IndexedEntities.ContainsKey("2"));
        }

        [Fact]
        public async Task ShouldIndexNew()
        {
            //Arrange
            var sp = await InitServices(o =>
                {
                    o.Namespaces = new[]
                    {
                        new NsOptions
                        {
                            NsId = "foojob",
                            SyncDbQuery =  "select * from foo_table where Id > @seed",
                            NewUpdatesStrategy = NewUpdatesStrategy.Add,
                            IdPropertyName = nameof(TestEntity.Id),
                            EsIndex = "[no mater in this test]"
                        }
                    };
                });

            var logic = sp.GetService<ITaskLogic>();
            var indexer = (TestIndexer)sp.GetService<IDataIndexer>();

            await SaveSeed(sp, ss => ss.WriteIdAsync("foojob", 3));

            //Act
            await logic.Perform(CancellationToken.None);

            //Assert
            Assert.Single(indexer.IndexedEntities);
            Assert.True(indexer.IndexedEntities.ContainsKey("4"));
        }
    }
}

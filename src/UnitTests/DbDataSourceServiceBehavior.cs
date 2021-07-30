using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using MyLab.DbTest;
using MyLab.Search.Indexer;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests
{
    public class DbDataSourceServiceBehavior : IClassFixture<TmpDbFixture>
    {
        private readonly TmpDbFixture _dvFxt;

        public DbDataSourceServiceBehavior(TmpDbFixture dvFxt, ITestOutputHelper output)
        {
            dvFxt.Output = output;
            _dvFxt = dvFxt;
        }

        [Fact]
        public async Task ShouldReadPages()
        {
            //Arrange
            var dbManager = await _dvFxt.CreateDbAsync(new FiveInserter());

            var options = new IndexerOptions
            {
                PageSize = 2,
                EnablePaging = true
            };

            var seedService = new TestSeedService();

            var service = new DbDataSourceService(dbManager,seedService, options);

            var accum = new List<DataSourceEntity[]>();

            var iterator = await service.Read("select * from foo_table where Id > 0 limit @limit offset @offset");

            //Act
            await foreach(var batch in  iterator)
            {
                accum.Add(batch.Entities);
            }

            //Assert
            Assert.Equal(2, accum.Count);
            Assert.Equal(2, accum[0].Length);
            Assert.NotNull(accum[0][0].Properties);
            Assert.NotNull(accum[0][1].Properties);
            Assert.Equal(2, accum[0][0].Properties.Count);
            Assert.Equal(2, accum[0][1].Properties.Count);
            Assert.Equal("1", accum[0][0].Properties[nameof(TestEntity.Id)]);
            Assert.Equal("1", accum[0][0].Properties[nameof(TestEntity.Value)]);
            Assert.Equal("2", accum[0][1].Properties[nameof(TestEntity.Id)]);
            Assert.Equal("2", accum[0][1].Properties[nameof(TestEntity.Value)]);

            Assert.Equal(2, accum[1].Length);
            Assert.NotNull(accum[1][0].Properties);
            Assert.NotNull(accum[1][1].Properties);
            Assert.Equal(2, accum[1][0].Properties.Count);
            Assert.Equal(2, accum[1][1].Properties.Count);
            Assert.Equal("3", accum[1][0].Properties[nameof(TestEntity.Id)]);
            Assert.Equal("3", accum[1][0].Properties[nameof(TestEntity.Value)]);
            Assert.Equal("4", accum[1][1].Properties[nameof(TestEntity.Id)]);
            Assert.Equal("4", accum[1][1].Properties[nameof(TestEntity.Value)]);
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
                await t.BulkCopyAsync(Enumerable.Repeat<TestEntity>(null, 5).Select((entity, i) => new TestEntity{ Id = i, Value = i.ToString()}));
            }
        }

        class TestSeedService : ISeedService
        {
            public Task WriteAsync(DateTime seed)
            {
                return Task.CompletedTask;
            }

            public Task<DateTime> ReadAsync()
            {
                return Task.FromResult(DateTime.Now);
            }
        }
    }
}

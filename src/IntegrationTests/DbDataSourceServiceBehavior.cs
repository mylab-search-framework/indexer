using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using MyLab.DbTest;
using MyLab.Search.Indexer;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
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
                PageSize = 2
            };

            var service = new DbDataSourceService(dbManager, options);

            var accum = new List<DataSourceEntity[]>();

            //Act
            await foreach(var batch in  service.Read("select * from foo_table where Id > 0 limit {limit} offset {offset}"))
            {
                accum.Add(batch);
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
    }
}

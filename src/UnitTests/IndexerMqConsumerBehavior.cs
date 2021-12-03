using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Services;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using Xunit;

namespace UnitTests
{
    public class IndexerMqConsumerBehavior
    {
        [Fact]
        public async Task ShouldNotIndexLastChangedProperty()
        {
            //Arrange
            var indexerOpt = new IndexerOptions
            {
                Jobs = new[]
                {
                    new JobOptions
                    {
                        JobId = "foo",
                        MqQueue = "bar",
                        EsIndex = "baz",
                        IdPropertyName = nameof(TestEntity.Id),
                        LastChangeProperty = nameof(TestEntity.LastModified),
                        NewUpdatesStrategy = NewUpdatesStrategy.Update
                    },
                }
            };

            var indexer = new TestDataIndexer();
            var pushIndexer = new PushIndexer(indexer);
            var consumer = new IndexerMqConsumer(indexerOpt, pushIndexer);

            var testEntity = new TestEntity
            {
                Id = 2,
                LastModified = DateTime.Now
            };

            var msgContent = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(testEntity));

            var msg = new BasicDeliverEventArgs("bar", 0, false, "", "bar", null, msgContent);

            //Act
            await consumer.ConsumeAsync(msg);

            //Assert
            Assert.Equal("foo", indexer.LastJobId);
            Assert.False(indexer.LstEntities[0].Properties.ContainsKey(nameof(TestEntity.LastModified)));
        }

        [Fact]
        public async Task ShouldIndexEntity()
        {
            //Arrange
            var indexerOpt = new IndexerOptions
            {
                Jobs = new []
                {
                    new JobOptions
                    {
                        JobId = "foo",
                        MqQueue = "bar",
                        EsIndex = "baz",
                        IdPropertyName = nameof(TestEntity.Id)
                    }, 
                }
            };

            var indexer = new TestDataIndexer();
            var pushIndexer = new PushIndexer(indexer);
            var consumer = new IndexerMqConsumer(indexerOpt, pushIndexer);

            var testEntity = new TestEntity
            {
                Id = 2,
                LastModified = DateTime.Now
            };

            var msgContent = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(testEntity));

            var msg = new BasicDeliverEventArgs("bar", 0, false, "", "bar", null, msgContent);

            //Act
            await consumer.ConsumeAsync(msg);

            //Assert
            Assert.Equal("foo", indexer.LastJobId);
        }

        class TestDataIndexer : IDataIndexer
        {
            public DataSourceEntity[] LstEntities { get; private set; }
            public string LastJobId { get; private set; }

            public Task IndexAsync(string jobId, DataSourceEntity[] dataSourceEntities, CancellationToken cancellationToken)
            {
                LastJobId = jobId;
                LstEntities = dataSourceEntities;

                return Task.CompletedTask;
            }
        }

        class TestEntity
        {
            public int Id { get; set; }
            public DateTime LastModified { get; set; }
        }
    }
}

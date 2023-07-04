using System.Threading.Tasks;
using LinqToDB;
using MyLab.Search.IndexerClient;
using Newtonsoft.Json.Linq;
using Xunit;

namespace FuncTests
{
    public partial class IndexerMqBehavior
    {
        [Fact]
        public async Task ShouldPutNew()
        {
            //Arrange
            var doc = TestDoc.Generate();

            var mqMsg = new IndexingMqMessage
            {
                IndexId = _testIndexName,
                Put = new[] { JObject.FromObject(doc) }
            };

            //Act
            _queue.Publish(mqMsg);
            await Task.Delay(500);
            await _kickApi.KickAsync();
            await Task.Delay(1000);

            var found = await SearchByIdAsync(doc.Id);

            //Assert
            Assert.Single(found);
            Assert.Equal(doc.Id, found[0].Id);
            Assert.Equal(doc.Content, found[0].Content);
        }

        [Fact]
        public async Task ShouldPutIndexed()
        {
            //Arrange
            var docV1 = TestDoc.Generate();
            var docV2 = TestDoc.Generate(docV1.Id);
            
            var mqMsg = new IndexingMqMessage
            {
                IndexId = _testIndexName,
                Put = new[] { JObject.FromObject(docV2) }
            };

            await _indexer.IndexAsync(docV1);

            await Task.Delay(500);

            //Act
            _queue.Publish(mqMsg);
            await Task.Delay(500);
            await _kickApi.KickAsync();
            await Task.Delay(1000);

            var found = await SearchByIdAsync(docV1.Id);

            //Assert
            Assert.Single(found);
            Assert.Equal(docV2.Content, found[0].Content);
        }

        [Fact]
        public async Task ShouldPatch()
        {
            //Arrange
            var docV1 = TestDoc.Generate();
            var docV2 = TestDoc.Generate(docV1.Id);

            var mqMsg = new IndexingMqMessage
            {
                IndexId = _testIndexName,
                Patch = new[] { JObject.FromObject(docV2) }
            };

            await _indexer.IndexAsync(docV1);

            await Task.Delay(500);

            //Act
            _queue.Publish(mqMsg);
            await Task.Delay(500);
            await _kickApi.KickAsync();
            await Task.Delay(1000);

            var found = await SearchByIdAsync(docV1.Id);

            //Assert
            Assert.Single(found);
            Assert.Equal(docV2.Content, found[0].Content);
        }

        [Fact]
        public async Task ShouldDelete()
        {
            //Arrange
            var doc = TestDoc.Generate();

            var mqMsg = new IndexingMqMessage
            {
                IndexId = _testIndexName,
                Delete = new []{ doc.Id }
            };

            await _indexer.IndexAsync(doc);

            await Task.Delay(500);

            //Act
            _queue.Publish(mqMsg);
            await Task.Delay(500);
            await _kickApi.KickAsync();
            await Task.Delay(1000);

            var found = await SearchByIdAsync(doc.Id);

            //Assert
            Assert.Empty(found);
        }

        [Fact]
        public async Task ShouldKickNew()
        {
            //Arrange
            var doc = TestDoc.Generate();

            var mqMsg = new IndexingMqMessage
            {
                IndexId = _testIndexName,
                Kick = new []{ doc.Id }
            };
            
            var insertedCount = await _dbMgr.DoOnce().InsertAsync(doc);
            
            //Act
            _queue.Publish(mqMsg);
            await Task.Delay(500);
            await _kickApi.KickAsync();
            await Task.Delay(1000);
            
            var found = await SearchByIdAsync(doc.Id);

            //Assert
            Assert.Equal(1, insertedCount);
            Assert.Single(found);
            Assert.Equal(doc.Id, found[0].Id);
            Assert.Equal(doc.Content, found[0].Content);
        }

        [Fact]
        public async Task ShouldKickIndexed()
        {
            //Arrange
            var docV1 = TestDoc.Generate();
            var docV2 = TestDoc.Generate(docV1.Id);

            var mqMsg = new IndexingMqMessage
            {
                IndexId = _testIndexName,
                Kick = new[] { docV1.Id }
            };

            await _indexer.IndexAsync(docV1);
            
            var insertedCount = await _dbMgr.DoOnce().InsertAsync(docV2);

            await Task.Delay(500);

            //Act
            _queue.Publish(mqMsg);
            await Task.Delay(500);
            await _kickApi.KickAsync();
            await Task.Delay(1000);

            var found = await SearchByIdAsync(docV1.Id);

            //Assert
            Assert.Equal(1, insertedCount);
            Assert.Single(found);
            Assert.Equal(docV2.Content, found[0].Content);
        }
    }
}

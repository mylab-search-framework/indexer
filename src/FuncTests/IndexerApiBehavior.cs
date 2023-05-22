using System.Threading.Tasks;
using LinqToDB;
using MyLab.Search.EsAdapter.Search;
using Newtonsoft.Json.Linq;
using Xunit;

namespace FuncTests
{
    public partial class IndexerApiBehavior
    {
        [Fact]
        public async Task ShouldDelete()
        {
            //Arrange
            var docForDelete = TestDoc.Generate();

            await _esFxt.Tools.Index("baz").CreateAsync();
            await _indexer.CreateAsync(docForDelete);

            //Act
            await _api.DeleteAsync("baz", docForDelete.Id.ToString());
            await Task.Delay(1000);

            var found = await SearchByIdAsync(docForDelete.Id);

            //Assert
            Assert.Empty(found);
        }

        [Fact]
        public async Task ShouldPutNew()
        {
            //Arrange
            var docForPut = TestDoc.Generate();

            //Act
            await _api.PutAsync("baz", JObject.FromObject(docForPut));
            await Task.Delay(1000);

            var found = await SearchByIdAsync(docForPut.Id);
            
            //Assert
            Assert.Single(found);
            Assert.Equal(docForPut.Content, found[0].Content);
        }

        [Fact]
        public async Task ShouldPutIndexed()
        {
            //Arrange
            var newDoc = TestDoc.Generate();
            var docForPut = TestDoc.Generate(newDoc.Id);

            //Act
            await _api.PutAsync("baz", JObject.FromObject(newDoc));
            await Task.Delay(500);
            await _api.PutAsync("baz", JObject.FromObject(docForPut));
            await Task.Delay(1000);

            var found = await SearchByIdAsync(docForPut.Id);
            
            //Assert
            Assert.Single(found);
            Assert.Equal(docForPut.Content, found[0].Content);
        }

        [Fact]
        public async Task ShouldPatch()
        {
            //Arrange
            var newDoc = TestDoc.Generate();
            var docForPatch = TestDoc.Generate(newDoc.Id);

            //Act
            await _api.PutAsync("baz", JObject.FromObject(newDoc));
            await Task.Delay(500);
            await _api.PutAsync("baz", JObject.FromObject(docForPatch));
            await Task.Delay(1000);

            var found = await SearchByIdAsync(docForPatch.Id); ;

            //Assert
            Assert.Single(found);
            Assert.Equal(docForPatch.Content, found[0].Content);
        }

        [Fact]
        public async Task ShouldKickNew()
        {
            //Arrange
            var docForKick = TestDoc.Generate();

            var inserted = await _dbMgr.DoOnce().InsertAsync(docForKick);

            //Act
            await _api.KickAsync("baz", docForKick.Id);
            await Task.Delay(1000);

            var found = await SearchByIdAsync(docForKick.Id);

            //Assert
            Assert.Equal(1, inserted);
            Assert.Single(found);
            Assert.Equal(docForKick.Content, found[0].Content);
        }

        [Fact]
        public async Task ShouldKickIndexed()
        {
            //Arrange
            var newDoc = TestDoc.Generate();
            var docForKick = TestDoc.Generate(newDoc.Id);

            await _api.PutAsync("baz", JObject.FromObject(newDoc));
            await Task.Delay(500);

            var inserted = await _dbMgr.DoOnce().InsertAsync(docForKick);

            //Act
            await _api.KickAsync("baz", docForKick.Id.ToString());
            await Task.Delay(1000);

            var found = await SearchByIdAsync(docForKick.Id);

            //Assert
            Assert.Single(found);
            Assert.Equal(docForKick.Content, found[0].Content);
        }
    }
}
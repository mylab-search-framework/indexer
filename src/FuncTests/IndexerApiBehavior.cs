using System.Threading.Tasks;
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

            await _indexer.CreateAsync(docForDelete);

            //Act
            await _api.DeleteAsync("foo-index", docForDelete.Id.ToString());
            await Task.Delay(500);

            var found = await SearchByIdAsync(docForDelete.Id);

            //Assert
            Assert.Empty(found);
        }

        [Fact]
        public async Task ShouldPost()
        {
            //Arrange
            var docForPost = TestDoc.Generate();

            //Act
            await _api.PostAsync("foo-index", JObject.FromObject(docForPost));
            await Task.Delay(500);

            var found = await SearchByIdAsync(docForPost.Id);

            //Assert
            Assert.Empty(found);
        }

        [Fact]
        public async Task ShouldPutCreate()
        {
            //Arrange
            var docForPut = TestDoc.Generate();

            //Act
            await _api.PutAsync("foo-index", JObject.FromObject(docForPut));
            await Task.Delay(1000);

            var found = await SearchByIdAsync(docForPut.Id);
            
            //Assert
            Assert.Single(found);
            Assert.Equal(docForPut.Content, found[0].Content);
        }

        [Fact]
        public async Task ShouldPutUpdate()
        {
            //Arrange
            var docForPost = TestDoc.Generate();
            var docForPut = TestDoc.Generate(docForPost.Id);

            //Act
            await _api.PostAsync("foo-index", JObject.FromObject(docForPost));
            await Task.Delay(500);
            await _api.PutAsync("foo-index", JObject.FromObject(docForPut));
            await Task.Delay(500);

            var found = await SearchByIdAsync(docForPut.Id);
            
            //Assert
            Assert.Single(found);
            Assert.Equal(docForPut.Content, found[0].Content);
        }

        [Fact]
        public async Task ShouldPatch()
        {
            //Arrange
            var docForPost = TestDoc.Generate();
            var docForPatch = TestDoc.Generate(docForPost.Id);

            //Act
            await _api.PostAsync("foo-index", JObject.FromObject(docForPost));
            await Task.Delay(500);
            await _api.PutAsync("foo-index", JObject.FromObject(docForPatch));
            await Task.Delay(500);

            var found = await SearchByIdAsync(docForPatch.Id); ;

            //Assert
            Assert.Single(found);
            Assert.Equal(docForPatch.Content, found[0].Content);
        }
    }
}
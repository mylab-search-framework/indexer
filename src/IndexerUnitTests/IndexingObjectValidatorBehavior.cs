using System.Text.Json.Nodes;
using Indexer.Domain.Model;
using Indexer.Domain.Validators;
using Newtonsoft.Json.Linq;

namespace DomainTests
{
    public class IndexingObjectValidatorBehavior
    {
        [Fact]
        public void ShouldPassJson()
        {
            //Arrange
            var json = new JsonObject(new Dictionary<string, JsonNode?>
            {
                { "id", "baz" },
                { "foo", "bar" }
            });
            var indexingObject = new IndexingObject(json);

            var validator = new IndexingObjectValidator();

            //Act
            var res = validator.Validate(indexingObject);

            //Assert
            Assert.True(res.IsValid);
        }

        [Fact]
        public void ShouldFailEmptyJson()
        {
            //Arrange
            var json = new JsonObject(new Dictionary<string, JsonNode?>
            {
            });
            var indexingObject = new IndexingObject(json);

            var validator = new IndexingObjectValidator();

            //Act
            var res = validator.Validate(indexingObject);

            //Assert
            Assert.False(res.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("  \t")]
        [InlineData(null)]
        public void ShouldFailWrongId(string idValue)
        {
            //Arrange
            var json = new JsonObject(new Dictionary<string, JsonNode?>
            {
                { "id", idValue }
            });
            var indexingObject = new IndexingObject(json);

            var validator = new IndexingObjectValidator();

            //Act
            var res = validator.Validate(indexingObject);

            //Assert
            Assert.False(res.IsValid);
        }

        [Theory]
        [InlineData("id")]
        [InlineData("Id")]
        [InlineData("ID")]
        public void ShouldPassIndependentOfIdPropNameCases(string idPropertyName)
        {
            //Arrange
            var json = new JsonObject(new Dictionary<string, JsonNode?>
            {
                { idPropertyName, "foo" }
            });
            var indexingObject = new IndexingObject(json);

            var validator = new IndexingObjectValidator();

            //Act
            var res = validator.Validate(indexingObject);

            //Assert
            Assert.True(res.IsValid);

        }
    }
}

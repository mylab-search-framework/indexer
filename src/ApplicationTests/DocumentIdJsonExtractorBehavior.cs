using System.Reflection.Metadata;
using System.Text.Json.Nodes;
using Indexer.Application.Tools;

namespace ApplicationTests
{
    public class DocumentIdJsonExtractorBehavior
    {
        [Theory]
        [InlineData("id")]
        [InlineData("Id")]
        [InlineData("ID")]
        public void ShouldExtractId(string idPropertyName)
        {
            //Arrange
            string json = $"{{\"{idPropertyName}\": \"foo\", \"bar\": \"baz\"}}";
            var jsonObj = JsonNode.Parse(json);

            //Act
            var success = DocumentIdJsonExtractor.TryExtract(jsonObj!, out var docId);

            //Assert
            Assert.True(success);
            Assert.NotNull(docId);
            Assert.Equal("foo", docId.Value);
        }

        [Fact]
        public void ShouldNotExtractWhenAbsent()
        {
            //Arrange
            const string json = """{"bar": "baz"}""";
            var jsonObj = JsonNode.Parse(json);

            //Act
            var success = DocumentIdJsonExtractor.TryExtract(jsonObj!, out var docId);

            //Assert
            Assert.False(success);
            Assert.Null(docId);
        }
    }
}
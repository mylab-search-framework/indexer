using System;
using MyLab.Search.Indexer.Options;
using Xunit;

namespace UnitTests
{
    public class IndexerOptionsBehavior
    {
        [Fact]
        public void ShouldSearchIndexOptions()
        {
            //Arrange
            var indexOpt = new IndexOptions
            {
                Id = "foo"
            };
            var opts = new IndexerOptions
            {
                Indexes = new[]
                {
                    indexOpt
                }
            };

            //Act
            var foundOpts = opts.GetIndexOptions("foo");

            //Assert
            Assert.Equal(indexOpt, foundOpts);
        }

        [Fact]
        public void ShouldFailIfIndexOptsNotFound()
        {
            //Arrange
            var opts = new IndexerOptions();

            //Act & Assert
            Assert.Throws<IndexOptionsNotFoundException>(() => opts.GetIndexOptions("foo"));
        }

        [Theory]
        [InlineData("pre-", "-post", "pre-bar-post")]
        [InlineData(null, "-post", "bar-post")]
        [InlineData("pre-", null, "pre-bar")]
        [InlineData(null, null, "bar")]
        public void ShouldProvideEsIndexName(string prefix, string postfix, string expected)
        {
            //Arrange
            var indexOpt = new IndexOptions
            {
                Id = "foo",
                EsIndex = "bar"
            };
            var opts = new IndexerOptions
            {
                EsIndexNamePrefix = prefix,
                EsIndexNamePostfix = postfix,
                Indexes = new[]
                {
                    indexOpt
                }
            };

            //Act
            var foundName = opts.GetEsIndexName("foo");

            //Assert
            Assert.Equal(expected, foundName);
        }
    }
}

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
        [InlineData("pre-", "bar", "-post", "pre-bar-post")]
        [InlineData(null, "bar", "-Post", "bar-post")]
        [InlineData("PRE-", "bar", null, "pre-bar")]
        [InlineData(null, "bar", null, "bar")]
        [InlineData("pre-", "BAR", "-post", "pre-bar-post")]
        [InlineData(null, "BAR", "-POST", "bar-post")]
        [InlineData("pRe-", "BAR", null, "pre-bar")]
        [InlineData(null, "BAR", null, "bar")]
        public void ShouldProvideEsIndexName(string prefix, string value, string postfix, string expected)
        {
            //Arrange
            var indexOpt = new IndexOptions
            {
                Id = value
            };
            var opts = new IndexerOptions
            {
                EsNamePrefix = prefix,
                EsNamePostfix = postfix,
                Indexes = new[]
                {
                    indexOpt
                }
            };

            //Act
            var foundName = opts.GetEsIndexName(value);

            //Assert
            Assert.Equal(expected, foundName);
        }
    }
}

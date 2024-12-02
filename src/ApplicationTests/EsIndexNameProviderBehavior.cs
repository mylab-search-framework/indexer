using Indexer.Application.Services;
using Indexer.Application.Options;
using Indexer.Domain.ValueObjects;

namespace ApplicationTests
{
    public class EsIndexNameProviderBehavior
    {
        [Theory]
        [MemberData(nameof(GetOptionsCases))]
        
        public void ShouldProvideIndexName(IndexerAppOptions appOptions, string expected)
        {
            //Arrange
            var provider = new EsIndexNameProvider(appOptions);

            //Act
            var actualProvidedValue = provider.Provide(new IndexId("foo"));

            //Assert
            Assert.Equal(expected, actualProvidedValue);
        }

        public static object[][] GetOptionsCases()
        {
            return new object[][]
            {
                new object[]
                {
                    new IndexerAppOptions(),
                    "foo"
                },
                new object[]
                {
                    new IndexerAppOptions
                    {
                        IndexPrefix = "pre-"
                    },
                    "pre-foo"
                },
                new object[]
                {
                    new IndexerAppOptions
                    {
                        IndexPrefix = "pre-",
                        IndexSuffix = "-post"
                    },
                    "pre-foo-post"
                },
                new object[]
                {
                    new IndexerAppOptions
                    {
                        IndexPrefix = "pre-",
                        IndexSuffix = "-post",
                        IndexMap = new Dictionary<string, string>
                        {
                            {"foo", "bar"}
                        }
                    },
                    "pre-bar-post"
                },
            };
        }
    }
}

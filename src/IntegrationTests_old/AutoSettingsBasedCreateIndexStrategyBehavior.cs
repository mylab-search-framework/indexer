using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyLab.DbTest;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Tools;
using Nest;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class AutoSettingsBasedCreateIndexStrategyBehavior : IClassFixture<EsFixture<TestConnProvider>>
    {
        private readonly EsFixture<TestConnProvider> _fxt;

        public AutoSettingsBasedCreateIndexStrategyBehavior(EsFixture<TestConnProvider> fxt, ITestOutputHelper output)
        {
            fxt.Output = output;

            _fxt = fxt;
        }

        [Fact]
        public async Task ShouldCreateIndexWithMapping()
        {
            //Arrange
            var exampleEntity = new DataSourceEntity
            {
                Properties = new Dictionary<string, DataSourcePropertyValue>
                {
                    {
                        "name", new DataSourcePropertyValue
                        {
                            Value = "foo",
                            DbType = DataSourcePropertyType.String
                        }
                    }
                }
            };
            var createIndexStrategy = new AutoSettingsBasedCreateIndexStrategy(exampleEntity);
            string indexName = Guid.NewGuid().ToString("N");

            GetIndexResponse indexResp;

            //Act

            try
            {
                await createIndexStrategy.CreateIndexAsync(_fxt.Manager, indexName, CancellationToken.None);
                indexResp = await _fxt.EsClient.Indices.GetAsync(indexName);
            }
            finally
            {
                await _fxt.Manager.DeleteIndexAsync(indexName);
            }
            
            var mapping = indexResp.Indices[indexName].Mappings;

            //Assert
            Assert.True(indexResp.IsValid);
            Assert.Equal(1, mapping.Properties.Count);
            Assert.True(mapping.Properties.ContainsKey("name"));
            Assert.Equal("text", mapping.Properties["name"].Type);
        }
    }
}

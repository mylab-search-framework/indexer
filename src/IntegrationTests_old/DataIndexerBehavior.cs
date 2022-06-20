using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MyLab.Search.EsAdapter;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class DataIndexerBehavior : IClassFixture<EsFixture<TestConnProvider>>
    {
        private readonly EsFixture<TestConnProvider> _esFxt;

        public DataIndexerBehavior(EsFixture<TestConnProvider> esFxt, ITestOutputHelper output)
        {
            esFxt.Output = output;
            _esFxt = esFxt;
        }

        [Fact]
        public async Task ShouldIndexEntity()
        {
            //Arrange
            string indexName = "test-" + Guid.NewGuid().ToString("N");

            var options = new IndexerOptions
            {
                Indexes = new []
                {
                    new IdxOptions
                    {
                        Id = "foo-idx",
                        NewIndexStrategy = NewIndexStrategy.Auto,
                        IdPropertyName = "Id",
                        EsIndex = indexName
                    }
                }
            };
            
            var esIndexer = _esFxt.CreateIndexer<IndexEntity>();
            var indexToucher = new EsIndexToucher(_esFxt.Manager, null, null);
            var indexer = new DataIndexer(options, esIndexer, _esFxt.Manager, null, new TestIndexMappingService(), indexToucher,  null);

            var searcher = _esFxt
                .CreateSearcher<IndexEntity>()
                .ForIndex(indexName);

            var idValue = Guid.NewGuid().ToString("N");
            var testEntity = new DataSourceEntity
            {
                Properties = new Dictionary<string, DataSourcePropertyValue>
                {
                    { 
                        "Id", 
                        new DataSourcePropertyValue
                        {
                            Value    = idValue,
                            DbType = DataSourcePropertyType.String
                        }
                    },
                    {
                        "Value",
                        new DataSourcePropertyValue
                        {
                            Value    = "foo-val",
                            DbType = DataSourcePropertyType.String
                        }
                    }
                }
            };

            var searchParams = new SearchParams<IndexEntity>(sd =>
                sd.Ids(idqd => idqd.Values(idValue)));

            EsFound<IndexEntity> searchResult;

            //Act
            try
            {
                await indexer.IndexAsync("foo-idx", new []{ testEntity }, CancellationToken.None);

                await Task.Delay(1000);

                searchResult = await searcher.SearchAsync(searchParams);
            }
            finally
            {
                await _esFxt.Manager.DeleteIndexAsync(indexName);
            }
            
            //Assert
            Assert.Equal(1, searchResult.Total);
            Assert.Equal("foo-val", searchResult.First()["Value"]);
        }

        [Fact]
        public async Task ShouldUpdateEntity()
        {
            //Arrange
            string indexName = "test-" + Guid.NewGuid().ToString("N");

            var options = new IndexerOptions
            {
                Indexes = new[]
                {
                    new IdxOptions
                    {
                        Id = "foo-idx",
                        NewIndexStrategy = NewIndexStrategy.Auto,
                        IdPropertyName = "Id",
                        EsIndex = indexName
                    }
                }
            };
            
            var esIndexer = _esFxt.CreateIndexer<IndexEntity>();
            var indexToucher = new EsIndexToucher(_esFxt.Manager, null, null);
            var indexer = new DataIndexer(options, esIndexer, _esFxt.Manager, null, new TestIndexMappingService(), indexToucher, null);

            var searcher = _esFxt
                .CreateSearcher<IndexEntity>()
                .ForIndex(indexName);

            var idValue = Guid.NewGuid().ToString("N");
            var initialTestEntity = new DataSourceEntity
            {
                Properties = new Dictionary<string, DataSourcePropertyValue>
                {
                    {
                        "Id",
                        new DataSourcePropertyValue
                        {
                            Value    = idValue,
                            DbType = DataSourcePropertyType.String
                        }
                    },
                    {
                        "Value",
                        new DataSourcePropertyValue
                        {
                            Value    = "foo-val",
                            DbType = DataSourcePropertyType.String
                        }
                    }
                }
            };

            var lateTestEntity = new DataSourceEntity
            {
                Properties = new Dictionary<string, DataSourcePropertyValue>
                {
                    {
                        "Id",
                        new DataSourcePropertyValue
                        {
                            Value    = idValue,
                            DbType = DataSourcePropertyType.String
                        }
                    },
                    {
                        "Value",
                        new DataSourcePropertyValue
                        {
                            Value    = "bar-val",
                            DbType = DataSourcePropertyType.String
                        }
                    }
                }
            };

            var searchParams = new SearchParams<IndexEntity>(sd =>
                sd.Ids(idqd => idqd.Values(idValue)));

            EsFound<IndexEntity> searchResult;

            //Act
            try
            {
                await indexer.IndexAsync("foo-idx", new[] { initialTestEntity }, CancellationToken.None);
                await indexer.IndexAsync("foo-idx", new[] { lateTestEntity }, CancellationToken.None);

                await Task.Delay(1000);

                searchResult = await searcher.SearchAsync(searchParams);
            }
            finally
            {
                await _esFxt.Manager.DeleteIndexAsync(indexName);
            }

            //Assert
            Assert.Equal(1, searchResult.Total);
            Assert.Equal("bar-val", searchResult.First()["Value"]);
        }
    }

    class TestIndexMappingService : IIndexMappingService
    {
        public Task<IndexMapping> GetIndexMappingAsync(string esIndexName)
        {
            return Task.FromResult(
                new IndexMapping(
                    new[]
                    {
                        new IndexMappingProperty("Id", "text"), 
                        new IndexMappingProperty("Value", "text"), 
                    }
            ));
        }
    }
}

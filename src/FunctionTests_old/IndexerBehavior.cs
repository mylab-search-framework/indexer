using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyLab.ApiClient.Test;
using MyLab.Db;
using MyLab.Search.EsAdapter;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using MyLab.Search.IndexerClient;
using MyLab.TaskApp;
using Nest;
using Xunit;
using Xunit.Abstractions;

namespace FunctionTests
{
    public partial class IndexerBehavior : 
        IClassFixture<IndexerTestDbFixture>, 
        IClassFixture<EsFixture<TestEsConnection>>,
        IClassFixture<MqFixture>,
        IAsyncLifetime
    {
        [Fact]
        public async Task ShouldInterpretNullAsAbsentProperty()
        {
            //Arrange
            string indexName = "test-" + Guid.NewGuid().ToString("N");

            var testEntity = new TestEntity
            {
                Id = 2,
                Value = "foo",
                Bool = null
            };

            var searchParams = new SearchParams<SearchTestEntity>(d => d.Ids(iqd => iqd.Values(testEntity.Id)));

            var client = _taskApi.StartWithProxy(srv =>
            {
                srv.Configure<IndexerDbOptions>(o =>
                {
                    o.Provider = "mysql";
                }
                );

                srv.Configure<IndexerOptions>(o =>
                {
                    o.Indexes = new[]
                    {
                            new IdxOptions
                            {
                                Id = "foo-idx",

                                SyncDbQuery = "select * from test",
                                NewUpdatesStrategy = NewUpdatesStrategy.Add,
                                NewIndexStrategy = NewIndexStrategy.File,
                                IdPropertyName = nameof(TestEntity.Id),
                                EsIndex = indexName
                            }
                        };
                });

                srv.Configure<ElasticsearchOptions>(o =>
                {
                    o.Url = "http://localhost:9200";
                }
                );

                srv.AddSingleton<IConnectionStringProvider, TestDbCsProvider>();
                srv.AddSingleton<ISeedService, TestSeedService>();

                srv.AddSingleton<IIndexResourceProvider>(new TestIndexResourceProvider("test-entity-map.json", null));

                srv.AddLogging(l => l.AddXUnit(_output).AddFilter(l => true));
            }
            );

            await _db.DoOnce().InsertAsync(testEntity);

            //Act

            await client.PostProcessAsync();
            await Task.Delay(2000);

            var searchRes = await _es.ForIndex(indexName).SearchAsync(searchParams);

            //Assert
            Assert.NotNull(searchRes);
            Assert.Single(searchRes);
            Assert.Equal(testEntity.Value, searchRes.First().Value);
            Assert.Null(searchRes.First().Bool);
        }

        [Fact]
        public async Task ShouldUseBoolRight()
        {
            //Arrange
            string indexName = "test-" + Guid.NewGuid().ToString("N");

            var testEntity = new TestEntity
            {
                Id = 2,
                Value = "foo",
                Bool = true
            };

            var searchParams = new SearchParams<SearchTestEntity>(d => d.Ids(iqd => iqd.Values(testEntity.Id)));

            var client = _taskApi.StartWithProxy(srv =>
            {
                srv.Configure<IndexerDbOptions>(o =>
                    {
                        o.Provider = "mysql";
                    }
                );

                srv.Configure<IndexerOptions>(o =>
                {
                    o.Indexes = new[]
                    {
                            new IdxOptions
                            {
                                Id = "foo-idx",

                                SyncDbQuery = "select * from test",
                                NewUpdatesStrategy = NewUpdatesStrategy.Add,
                                NewIndexStrategy = NewIndexStrategy.File,
                                IdPropertyName = nameof(TestEntity.Id),
                                EsIndex = indexName
                            }
                        };
                });

                srv.Configure<ElasticsearchOptions>(o =>
                {
                    o.Url = "http://localhost:9200";
                }
                );

                srv.AddSingleton<IConnectionStringProvider, TestDbCsProvider>();
                srv.AddSingleton<ISeedService, TestSeedService>();

                srv.AddSingleton<IIndexResourceProvider>(new TestIndexResourceProvider("test-entity-map.json", null));

                srv.AddLogging(l => l.AddXUnit(_output).AddFilter(l => true));
            }
            );

            await _db.DoOnce().InsertAsync(testEntity);

            //Act

            await client.PostProcessAsync();
            await Task.Delay(2000);

            var searchRes = await _es.ForIndex(indexName).SearchAsync(searchParams);

            //Assert
            Assert.NotNull(searchRes);
            Assert.Single(searchRes);
            Assert.Equal(testEntity.Value, searchRes.First().Value);
            Assert.True(searchRes.First().Bool);
        }

        [Fact]
        public async Task ShouldIndexFromDb()
        {
            //Arrange
            string indexName = "test-" + Guid.NewGuid().ToString("N");
            
            var testEntity = new TestEntity
            {
                Id = 2,
                Value = "foo"
            };

            var searchParams = new SearchParams<SearchTestEntity>(d => d.Ids(iqd => iqd.Values(testEntity.Id)));

            var client = _taskApi.StartWithProxy(srv =>
                {
                    srv.Configure<IndexerDbOptions>(o =>
                        {
                            o.Provider = "mysql";
                            
                        }
                    );

                    srv.Configure<IndexerOptions>(o =>
                    {
                        o.Indexes = new[]
                        {
                            new IdxOptions
                            {
                                Id = "foo-idx",

                                SyncDbQuery = "select * from test",
                                NewUpdatesStrategy = NewUpdatesStrategy.Add,
                                NewIndexStrategy = NewIndexStrategy.Auto,
                                IdPropertyName = nameof(TestEntity.Id),
                                EsIndex = indexName
                            }
                        };
                    });

                    srv.Configure<ElasticsearchOptions>(o =>
                        {
                            o.Url = "http://localhost:9200";
                        }
                    );

                    srv.AddSingleton<IConnectionStringProvider, TestDbCsProvider>();

                    srv.AddLogging(l => l.AddXUnit(_output).AddFilter(l => true));
                }
            );

            await _db.DoOnce().InsertAsync(testEntity);
            
            //Act

            await client.PostProcessAsync();
            await Task.Delay(2000);

            var searchRes = await _es.ForIndex(indexName).SearchAsync(searchParams);            

            //Assert
            Assert.NotNull(searchRes);
            Assert.Single(searchRes);
            Assert.Equal(testEntity.Value, searchRes.First().Value);
        }

        [Fact]
        public async Task ShouldIndexFromMq()
        {
            //Arrange
            var queue = _mqFxt.CreateWithRandomId();
            _output.WriteLine("MqQueue created: " + queue.Name);

            string indexName = "test-" + Guid.NewGuid().ToString("N");

            var testEntity = new SearchTestEntity
            {
                Id = 2,
                Value = "foo",
                Bool = false
            };

            var searchParams = new SearchParams<SearchTestEntity>(d => d.Ids(iqd => iqd.Values(testEntity.Id)));

            _taskApi.StartWithProxy(srv =>
                {

                    srv.Configure<IndexerOptions>(o =>
                        {
                            o.Indexes = new IdxOptions[]
                            {
                                new IdxOptions
                                {
                                    Id = "foo-idx",
                                    MqQueue = queue.Name,
                                    NewIndexStrategy = NewIndexStrategy.Auto,
                                    IdPropertyName = nameof(TestEntity.Id),
                                    EsIndex = indexName
                                },

                            };
                        }
                    );

                    srv.Configure<ElasticsearchOptions>(o =>
                        {
                            o.Url = "http://localhost:9200";
                        }
                    );

                    srv.ConfigureRabbit(o =>
                        {
                            o.Host = "localhost";
                            o.Port = 5672;
                            o.Password = "guest";
                            o.User = "guest";
                        }
                    );

                    srv.AddLogging(l => l.AddXUnit(_output).AddFilter(l => true));
                }
            );

            //Act
            queue.Publish(testEntity);

            await Task.Delay(2000);

            var searchRes = await _es.ForIndex(indexName).SearchAsync(searchParams);

            //Assert
            Assert.NotNull(searchRes);
            Assert.Single(searchRes);
            Assert.Equal(testEntity.Value, searchRes.First().Value);
        }

        [Fact]
        public async Task ShouldOverrideMqFromDb()
        {
            //Arrange
            string indexName = "test-" + Guid.NewGuid().ToString("N");

            var queue = _mqFxt.CreateWithRandomId();
            _output.WriteLine("MqQueue created: " + queue.Name);

            var initialTestEntity = new TestEntity
            {
                Id = 2,
                Value = "foo"
                
            };

            var overrideTestEntity = new TestEntity
            {
                Id = 2,
                Value = "bar"
            };

            var searchParams = new SearchParams<SearchTestEntity>(d => d.Ids(iqd => iqd.Values(initialTestEntity.Id)));

            var client = _taskApi.StartWithProxy(srv =>
                {
                    srv.Configure<IndexerDbOptions>(o =>
                        {
                            o.Provider = "mysql";
                        }
                    );

                    srv.Configure<IndexerOptions>(o =>
                        {
                            o.Indexes = new[]
                            {
                                new IdxOptions
                                {
                                    Id = "foo-idx",
                                    NewIndexStrategy = NewIndexStrategy.Auto,
                                    IdPropertyName = nameof(TestEntity.Id),
                                    MqQueue = queue.Name,
                                    SyncDbQuery = "select * from test",
                                    NewUpdatesStrategy = NewUpdatesStrategy.Add,
                                    EsIndex = indexName
                                }
                            };
                        }
                    );

                    srv.Configure<ElasticsearchOptions>(o =>
                        {
                            o.Url = "http://localhost:9200";
                        }
                    );

                    srv.ConfigureRabbit(o =>
                    {
                        o.Host = "localhost";
                        o.Port = 5672;
                        o.Password = "guest";
                        o.User = "guest";
                    });

                    srv.AddSingleton<IConnectionStringProvider, TestDbCsProvider>();

                    srv.AddLogging(l => l.AddXUnit(_output).AddFilter(l => true));
                }
            );

            queue.Publish(initialTestEntity);
            await Task.Delay(500);

            await _db.DoOnce().InsertAsync(overrideTestEntity);

            //Act


            await client.PostProcessAsync();
            await Task.Delay(2000);

            var searchRes = await _es.ForIndex(indexName).SearchAsync(searchParams);            

            //Assert
            Assert.NotNull(searchRes);
            Assert.Single(searchRes);
            Assert.Equal(overrideTestEntity.Value, searchRes.First().Value);
        }

        [Fact]
        public async Task ShouldIndexFromApi()
        {
            //Arrange
            var testEntity = new SearchTestEntity
            {
                Id = 2,
                Value = "foo",
                Bool = false
            };

            string indexName = "test-" + Guid.NewGuid().ToString("N");

            var searchParams = new SearchParams<SearchTestEntity>(d => d.Ids(iqd => iqd.Values(testEntity.Id)));

            var api = _indexApi.StartWithProxy(srv =>
            {

                srv.Configure<IndexerOptions>(o =>
                {
                    o.Indexes = new IdxOptions[]
                    {
                        new IdxOptions
                        {
                            Id = "foo-idx",
                            NewIndexStrategy = NewIndexStrategy.Auto,
                            IdPropertyName = nameof(TestEntity.Id),
                            EsIndex = indexName
                        },

                    };
                }
                );

                srv.Configure<ElasticsearchOptions>(o =>
                {
                    o.Url = "http://localhost:9200";
                }
                );

                srv.AddLogging(l => l.AddXUnit(_output).AddFilter(l => true));
            });

            //Act
            await api.IndexAsync("foo-idx", testEntity);

            await Task.Delay(2000);

            var searchRes = await _es.ForIndex(indexName).SearchAsync(searchParams);

            //Assert
            Assert.NotNull(searchRes);
            Assert.Single(searchRes);
            Assert.Equal(testEntity.Value, searchRes.First().Value);
        }

        [Fact]
        public async Task ShouldKickIndexFromApi()
        {
            //Arrange
            string indexName = "test-" + Guid.NewGuid().ToString("N");

            var testEntity = new TestEntity
            {
                Id = 2,
                Value = "foo"
            };

            var searchParams = new SearchParams<SearchTestEntity>(d => d.Ids(iqd => iqd.Values(testEntity.Id)));

            var taskApi = _taskApi.StartWithProxy(srv =>
            {
                srv.Configure<IndexerDbOptions>(o =>
                {
                    o.Provider = "mysql";
                }
                );

                srv.Configure<IndexerOptions>(o =>
                {
                    o.Indexes = new[]
                    {
                            new IdxOptions
                            {
                                Id = "foo-idx",

                                SyncDbQuery = "select * from test",
                                KickDbQuery = "select * from test where Id=@id",
                                NewUpdatesStrategy = NewUpdatesStrategy.Add,
                                NewIndexStrategy = NewIndexStrategy.Auto,
                                IdPropertyName = nameof(TestEntity.Id),
                                EsIndex = indexName
                            }
                        };
                });

                srv.Configure<ElasticsearchOptions>(o =>
                {
                    o.Url = "http://localhost:9200";
                }
                );

                srv.AddSingleton<IConnectionStringProvider, TestDbCsProvider>();

                srv.AddLogging(l => l.AddXUnit(_output).AddFilter(l => true));
            });

            await _db.DoOnce().InsertAsync(testEntity);

            await taskApi.PostProcessAsync();
            await Task.Delay(2000);

            var updated = await _db.DoOnce()
                .Tab<TestEntity>()
                .Where(e => e.Id == 2)
                .Set(e => e.Value, "bar")
                .UpdateAsync();

            if(updated < 1)
                throw new Exception("Not updated");

            var indexApi = _indexApi.StartWithProxy(srv =>
            {
                srv.Configure<IndexerDbOptions>(o =>
                    {
                        o.Provider = "mysql";
                    }
                );

                srv.Configure<IndexerOptions>(o =>
                {
                    o.Indexes = new[]
                    {
                        new IdxOptions
                        {
                            Id = "foo-idx",

                            KickDbQuery = "select * from test where id=@id",
                            NewUpdatesStrategy = NewUpdatesStrategy.Update,
                            NewIndexStrategy = NewIndexStrategy.Auto,
                            IdPropertyName = nameof(TestEntity.Id),
                            IdPropertyType = IdPropertyType.Int,
                            EsIndex = indexName
                        }
                    };
                });

                srv.Configure<ElasticsearchOptions>(o =>
                    {
                        o.Url = "http://localhost:9200";
                    }
                );

                srv.AddSingleton<IConnectionStringProvider, TestDbCsProvider>();

                srv.AddLogging(l => l.AddXUnit(_output).AddFilter(l => true));
            });

            //Act

            await indexApi.KickIndexAsync("foo-idx", "2");
            await Task.Delay(2000);

            var searchRes = await _es.ForIndex(indexName).SearchAsync(searchParams);

            //Assert
            Assert.NotNull(searchRes);
            Assert.Single(searchRes);
            Assert.Equal("bar", searchRes.First().Value);
        }

        [Fact]
        public async Task ShouldUseAutoMapping()
        {
            //Arrange
            var testEntity = new SearchTestEntity
            {
                Id = 2,
                Value = "foo",
                Bool = false
            };

            string indexName = "test-" + Guid.NewGuid().ToString("N");
            
            var api = _indexApi.StartWithProxy(srv =>
            {

                srv.Configure<IndexerOptions>(o =>
                    {
                        o.Indexes = new IdxOptions[]
                        {
                            new IdxOptions
                            {
                                Id = "foo-idx",
                                NewIndexStrategy = NewIndexStrategy.Auto,
                                IdPropertyName = nameof(TestEntity.Id),
                                EsIndex = indexName
                            },

                        };
                    }
                );

                srv.Configure<ElasticsearchOptions>(o =>
                    {
                        o.Url = "http://localhost:9200";
                    }
                );

                srv.AddLogging(l => l.AddXUnit(_output).AddFilter(l => true));
            });

            //Act
            await api.IndexAsync("foo-idx", testEntity);

            await Task.Delay(2000);

            var mappingResp = await _esClient.Indices.GetMappingAsync(new GetMappingRequest(indexName));

            mappingResp.Indices.TryGetValue(indexName, out var indexMapping);

            //Assert
            Assert.NotNull(indexMapping);
            Assert.Equal("long", indexMapping.Mappings.Properties["Id"].Type);
        }

        [Fact]
        public async Task ShouldUseFileMapping()
        {
            //Arrange
            var testEntity = new SearchTestEntity
            {
                Id = 2,
                Value = "foo",
                Bool = false
            };

            string indexName = "test-" + Guid.NewGuid().ToString("N");

            var api = _indexApi.StartWithProxy(srv =>
            {

                srv.Configure<IndexerOptions>(o =>
                    {
                        o.Indexes = new IdxOptions[]
                        {
                            new IdxOptions
                            {
                                Id = "foo-idx",
                                NewIndexStrategy = NewIndexStrategy.File,
                                IdPropertyName = nameof(TestEntity.Id),
                                EsIndex = indexName
                            },

                        };
                    }
                );

                srv.Configure<ElasticsearchOptions>(o =>
                    {
                        o.Url = "http://localhost:9200";
                    }
                );

                srv.AddSingleton<IIndexResourceProvider>(new TestIndexResourceProvider("test-entity-map.json", null));

                srv.AddLogging(l => l.AddXUnit(_output).AddFilter(l => true));
            });

            //Act
            await api.IndexAsync("foo-idx", testEntity);

            await Task.Delay(2000);

            var mappingResp = await _esClient.Indices.GetMappingAsync(new GetMappingRequest(indexName));

            mappingResp.Indices.TryGetValue(indexName, out var indexMapping);

            //Assert
            Assert.NotNull(indexMapping);
            Assert.Equal("text", indexMapping.Mappings.Properties["Value"].Type);
        }

        [Fact]
        public async Task ShouldUseDefaultFileMapping()
        {
            //Arrange
            var testEntity = new SearchTestEntity
            {
                Id = 2,
                Value = "foo",
                Bool = false
            };

            string indexName = "test-" + Guid.NewGuid().ToString("N");

            var api = _indexApi.StartWithProxy(srv =>
            {

                srv.Configure<IndexerOptions>(o =>
                    {
                        o.Indexes = new IdxOptions[]
                        {
                            new IdxOptions
                            {
                                Id = "foo-idx",
                                NewIndexStrategy = NewIndexStrategy.File,
                                IdPropertyName = nameof(TestEntity.Id),
                                EsIndex = indexName
                            },

                        };
                    }
                );

                srv.Configure<ElasticsearchOptions>(o =>
                    {
                        o.Url = "http://localhost:9200";
                    }
                );

                srv.AddSingleton<IIndexResourceProvider>(new TestIndexResourceProvider("absent.json", "default-test-entity-map.json"));

                srv.AddLogging(l => l.AddXUnit(_output).AddFilter(l => true));
            });

            //Act
            await api.IndexAsync("foo-idx", testEntity);

            await Task.Delay(2000);

            var mappingResp = await _esClient.Indices.GetMappingAsync(new GetMappingRequest(indexName));

            mappingResp.Indices.TryGetValue(indexName, out var indexMapping);

            //Assert
            Assert.NotNull(indexMapping);
            Assert.Equal("keyword", indexMapping.Mappings.Properties["Value"].Type);
        }
    }
}

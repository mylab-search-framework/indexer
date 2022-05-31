﻿using System;
using System.Linq;
using System.Threading.Tasks;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace UnitTests
{
    public class InputRequestProcessorBehavior
    {
        readonly IndexingRequestEntity _postEnt = new IndexingRequestEntity
        {
            Id = "post-id",
            Entity = JObject.FromObject(new TestEntity("post-id", "post-data"))
        };

        readonly IndexingRequestEntity _putEnt = new IndexingRequestEntity
        {
            Id = "put-id",
            Entity = JObject.FromObject(new TestEntity("put-id", "put-data"))
        };

        readonly IndexingRequestEntity _patchEnt = new IndexingRequestEntity
        {
            Id = "patch-id",
            Entity = JObject.FromObject(new TestEntity("patch-id", "patch-data"))
        };

        readonly IndexingRequestEntity _kickEnt = new IndexingRequestEntity
        {
            Id = "kick-id",
            Entity = JObject.FromObject(new TestEntity("kick-id", "kick-data"))
        };

        string _deleteId = "delete-id";

        [Fact]
        public async Task ShouldFailIfIndexNotFound()
        {
            //Arrange
            var inputRequest = new InputIndexingRequest
            {
                IndexId = "index-id"
            };

            IDataSourceService dataSourceService = new TestDataSourceService(null);
            TestIndexerService indexerService = new TestIndexerService();

            IndexerOptions options = new IndexerOptions();

            var inputReqProcessor = new InputRequestProcessor(dataSourceService, indexerService, options);

            //Act & Assert
            await Assert.ThrowsAsync<IndexOptionsNotFoundException>(() => inputReqProcessor.IndexAsync(inputRequest));
        }

        [Fact]
        public async Task ShouldSendAsIsIfNoDataSourceEntities()
        {
            //Arrange
            var inputRequest = new InputIndexingRequest
            {
                PostList = new [] { _postEnt },
                PutList = new[] { _putEnt },
                PatchList = new[] { _patchEnt },
                DeleteList = new[] { _deleteId },
                IndexId = "index-id"
            };

            IDataSourceService dataSourceService = new TestDataSourceService(null);
            TestIndexerService indexerService = new TestIndexerService();

            IndexerOptions options = new IndexerOptions
            {
                Indexes = new []
                {
                    new IndexOptions
                    {
                        Id = "index-id"
                    }
                }
            };

            var inputReqProcessor = new InputRequestProcessor(dataSourceService, indexerService, options);

            //Act
            await inputReqProcessor.IndexAsync(inputRequest);

            var actualReq = indexerService.LastRequest;

            //Assert
            Assert.NotNull(actualReq);
            
            Assert.Equal(_postEnt, actualReq.PostList?.FirstOrDefault());
            Assert.Equal(_putEnt, actualReq.PutList?.FirstOrDefault());
            Assert.Equal(_patchEnt, actualReq.PatchList?.FirstOrDefault());
            Assert.Equal("delete-id", actualReq.DeleteList?.FirstOrDefault());
        }
        
        [Fact]
        public async Task ShouldAddDataSourceEntitiesToPostListIfIndexIsStream()
        {
            //Arrange
            var inputRequest = new InputIndexingRequest
            {
                PostList = new[] { _postEnt },
                IndexId = "index-id",
                KickList = new []{ _kickEnt.Id }
            };

            var dataSourceLoad = new DataSourceLoad { 
                Entities = new[]
                {
                    _kickEnt
                },
                SeedSaver = new TestSeedSaver()
            };

            IDataSourceService dataSourceService = new TestDataSourceService(dataSourceLoad);
            TestIndexerService indexerService = new TestIndexerService();

            IndexerOptions options = new IndexerOptions
            {
                Indexes = new[]
                {
                    new IndexOptions
                    {
                        Id = "index-id",
                        IsStream = true
                    }
                }
            };

            var inputReqProcessor = new InputRequestProcessor(dataSourceService, indexerService, options);

            //Act
            await inputReqProcessor.IndexAsync(inputRequest);

            var actualReq = indexerService.LastRequest;

            //Assert
            Assert.NotNull(actualReq);

            Assert.Null(actualReq.PatchList);
            Assert.Null(actualReq.PutList);
            Assert.Null(actualReq.DeleteList);

            Assert.NotNull(actualReq.PostList);
            Assert.Equal(2, actualReq.PostList.Length);
            Assert.Equal(_postEnt, actualReq.PostList[0]);
            Assert.Equal(_kickEnt, actualReq.PostList[1]);
        }

        [Fact]
        public async Task ShouldAddDataSourceEntitiesToPutListIfIndexIsNotStream()
        {
            //Arrange
            var inputRequest = new InputIndexingRequest
            {
                PutList = new[] { _putEnt },
                IndexId = "index-id",
                KickList = new[] { _kickEnt.Id }
            };

            var dataSourceLoad = new DataSourceLoad
            {
                Entities = new[]
                {
                    _kickEnt
                },
                SeedSaver = new TestSeedSaver()
            };

            IDataSourceService dataSourceService = new TestDataSourceService(dataSourceLoad);
            TestIndexerService indexerService = new TestIndexerService();

            IndexerOptions options = new IndexerOptions
            {
                Indexes = new[]
                {
                    new IndexOptions
                    {
                        Id = "index-id",
                        IsStream = false
                    }
                }
            };

            var inputReqProcessor = new InputRequestProcessor(dataSourceService, indexerService, options);

            //Act
            await inputReqProcessor.IndexAsync(inputRequest);

            var actualReq = indexerService.LastRequest;

            //Assert
            Assert.NotNull(actualReq);

            Assert.Null(actualReq.PatchList);
            Assert.Null(actualReq.PostList);
            Assert.Null(actualReq.DeleteList);

            Assert.NotNull(actualReq.PutList);
            Assert.Equal(2, actualReq.PutList.Length);
            Assert.Equal(_putEnt, actualReq.PutList[0]);
            Assert.Equal(_kickEnt, actualReq.PutList[1]);
        }

        [Fact]
        public async Task ShouldFailIfDataSourceEntitiesExistAndSeedSaverIsNull()
        {
            //Arrange
            var inputRequest = new InputIndexingRequest
            {
                IndexId = "index-id",
                KickList = new[] { _kickEnt.Id }
            };

            var dataSourceLoad = new DataSourceLoad
            {
                Entities = new[]
                {
                    _kickEnt
                },
                SeedSaver = null
            };

            IDataSourceService dataSourceService = new TestDataSourceService(dataSourceLoad);
            TestIndexerService indexerService = new TestIndexerService();

            IndexerOptions options = new IndexerOptions
            {
                Indexes = new[]
                {
                    new IndexOptions
                    {
                        Id = "index-id"
                    }
                }
            };

            var inputReqProcessor = new InputRequestProcessor(dataSourceService, indexerService, options);

            //Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => inputReqProcessor.IndexAsync(inputRequest));
        }

        [Fact]
        public async Task ShouldNotFailIfDataSourceEntitiesDoesNotExistAndSeedSaverIsNull()
        {
            //Arrange
            var inputRequest = new InputIndexingRequest
            {
                IndexId = "index-id",
                KickList = new[] { _kickEnt.Id }
            };

            var dataSourceLoad = new DataSourceLoad
            {
                Entities = Array.Empty<IndexingRequestEntity>(),
                SeedSaver = null
            };

            IDataSourceService dataSourceService = new TestDataSourceService(dataSourceLoad);
            TestIndexerService indexerService = new TestIndexerService();

            IndexerOptions options = new IndexerOptions
            {
                Indexes = new[]
                {
                    new IndexOptions
                    {
                        Id = "index-id"
                    }
                }
            };

            var inputReqProcessor = new InputRequestProcessor(dataSourceService, indexerService, options);

            //Act & Assert
            await inputReqProcessor.IndexAsync(inputRequest);
        }

        [Fact]
        public async Task ShouldCallSeedSaverIfDataSourceEntitiesExist()
        {
            //Arrange
            var inputRequest = new InputIndexingRequest
            {
                IndexId = "index-id",
                KickList = new[] { _kickEnt.Id }
            };

            var seedSaver = new TestSeedSaver();

            var dataSourceLoad = new DataSourceLoad
            {
                Entities = new[]
                {
                    _kickEnt
                },
                SeedSaver = seedSaver
            };

            IDataSourceService dataSourceService = new TestDataSourceService(dataSourceLoad);
            TestIndexerService indexerService = new TestIndexerService();

            IndexerOptions options = new IndexerOptions
            {
                Indexes = new[]
                {
                    new IndexOptions
                    {
                        Id = "index-id",
                        IsStream = true
                    }
                }
            };

            var inputReqProcessor = new InputRequestProcessor(dataSourceService, indexerService, options);

            //Act
            await inputReqProcessor.IndexAsync(inputRequest);

            var actualReq = indexerService.LastRequest;

            //Assert
            Assert.NotNull(actualReq);
            Assert.True(seedSaver.HasCalled);
        }

        class TestDataSourceService : IDataSourceService
        {
            private readonly DataSourceLoad _dataSourceLoad;

            public TestDataSourceService(DataSourceLoad dataSourceLoad)
            {
                _dataSourceLoad = dataSourceLoad;
            }
            public Task<DataSourceLoad?> LoadEntitiesAsync(string indexId, string[] idList)
            {
                return Task.FromResult(_dataSourceLoad);
            }
        }

        class TestIndexerService : IIndexerService
        {
            public IndexingRequest LastRequest { get; set; }

            public Task IndexEntities(IndexingRequest indexingRequest)
            {
                LastRequest = indexingRequest;

                return Task.CompletedTask;
            }
        }

        class TestSeedSaver : ISeedSaver
        {
            public bool HasCalled { get; set; }

            public Task SaveAsync()
            {
                HasCalled = true;
                return Task.CompletedTask;
            }
        }

        class TestEntity
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            public string Content { get; set; }

            public TestEntity()
            {

            }

            public TestEntity(string id, string content)
            {
                Id = id;
                Content = content;
            }

            public static TestEntity Generate()
            {
                return new TestEntity
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Content = Guid.NewGuid().ToString("N")
                };
            }
        }
    }
}

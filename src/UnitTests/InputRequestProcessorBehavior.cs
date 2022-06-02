using System.Linq;
using System.Threading.Tasks;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using Xunit;

namespace UnitTests
{
    public partial class InputRequestProcessorBehavior
    {
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
            
            var inputReqProcessor = new InputRequestProcessor(dataSourceService, indexerService, new IndexerOptions());

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

            var indexOpts = new IndexOptions { Id = "index-id" };
            IndexerOptions options = new IndexerOptions { Indexes = new [] { indexOpts } };

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

            var dataSourceLoad = new DataSourceLoad { Batch = new DataSourceLoadBatch { Entities = new[] { _kickEnt } } };

            IDataSourceService dataSourceService = new TestDataSourceService(dataSourceLoad);
            TestIndexerService indexerService = new TestIndexerService();

            var indexOpts = new IndexOptions
            {
                Id = "index-id",
                IndexType = IndexType.Stream
            };
            IndexerOptions options = new IndexerOptions { Indexes = new[] { indexOpts } };

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
        public async Task ShouldAddDataSourceEntitiesToPutListIfIndexIsHeap()
        {
            //Arrange
            var inputRequest = new InputIndexingRequest
            {
                PutList = new[] { _putEnt },
                IndexId = "index-id",
                KickList = new[] { _kickEnt.Id }
            };

            var dataSourceLoad = new DataSourceLoad { Batch = new DataSourceLoadBatch { Entities = new [] { _kickEnt } } };

            IDataSourceService dataSourceService = new TestDataSourceService(dataSourceLoad);
            TestIndexerService indexerService = new TestIndexerService();

            var indexOpts = new IndexOptions
            {
                Id = "index-id",
                IndexType = IndexType.Heap
            };
            IndexerOptions options = new IndexerOptions { Indexes = new[] { indexOpts } };

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
    }
}

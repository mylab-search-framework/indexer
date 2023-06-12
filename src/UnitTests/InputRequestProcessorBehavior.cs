using System.Linq;
using System.Threading.Tasks;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Tools;
using Xunit;

namespace UnitTests
{
    public partial class InputRequestProcessorBehavior
    {

        [Fact]
        public async Task ShouldSendAsIsIfNoDataSourceDocs()
        {
            //Arrange
            var inputRequest = new InputIndexingRequest
            {
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
            
            Assert.Equal(_putEnt, actualReq.PutList?.FirstOrDefault());
            Assert.Equal(_patchEnt, actualReq.PatchList?.FirstOrDefault());
            Assert.Equal("delete-id", actualReq.DeleteList?.FirstOrDefault());
        }
        
        [Fact]
        public async Task ShouldAddDataSourceDocsToPutList()
        {
            //Arrange
            var inputRequest = new InputIndexingRequest
            {
                PutList = new[] { _putEnt },
                IndexId = "index-id",
                KickList = new[] { _kickEnt.GetIdProperty() }
            };

            var dataSourceLoad = new DataSourceLoad { Batch = new DataSourceLoadBatch { Docs = new [] { _kickEnt } } };

            IDataSourceService dataSourceService = new TestDataSourceService(dataSourceLoad);
            TestIndexerService indexerService = new TestIndexerService();

            var indexOpts = new IndexOptions
            {
                Id = "index-id",
                IsStream = false
            };
            IndexerOptions options = new IndexerOptions { Indexes = new[] { indexOpts } };

            var inputReqProcessor = new InputRequestProcessor(dataSourceService, indexerService, options);

            //Act
            await inputReqProcessor.IndexAsync(inputRequest);

            var actualReq = indexerService.LastRequest;

            //Assert
            Assert.NotNull(actualReq);

            Assert.Null(actualReq.PatchList);
            Assert.Null(actualReq.DeleteList);

            Assert.NotNull(actualReq.PutList);
            Assert.Equal(2, actualReq.PutList.Length);
            Assert.Equal(_putEnt, actualReq.PutList[0]);
            Assert.Equal(_kickEnt, actualReq.PutList[1]);
        }
    }
}

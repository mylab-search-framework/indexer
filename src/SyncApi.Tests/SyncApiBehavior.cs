using System.Net;
using System.Text.Json.Nodes;
using Indexer.Application.UseCases.DeleteDocument;
using Indexer.Application.UseCases.PatchDocument;
using Indexer.Application.UseCases.PutDocument;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MyLab.ApiClient.Test;
using Xunit.Abstractions;

namespace SyncApi.Tests
{
    public class SyncApiBehavior : IClassFixture<TestApiFixture<Program ,ISyncApiContract>>
    {
        private readonly TestApiFixture<Program, ISyncApiContract> _fxt;

        public SyncApiBehavior(TestApiFixture<Program, ISyncApiContract> fxt, ITestOutputHelper output)
        {
            fxt.Output = output;
            _fxt = fxt;
        }

        [Fact]
        public async Task ShouldPutDocument()
        {
            //Arrange
            var putHandlerMock = new Mock<IRequestHandler<PutDocumentCommand>>();
            var client = _fxt.StartWithProxy(srv =>
            {
                srv.AddTransient(_ => putHandlerMock.Object);
            });

            const string docJson = """{"id": "bar", "baz": "qoz"}""";
            var docNode = JsonNode.Parse(docJson);

            //Act
            var res = await client.ApiClient.PutAsync("foo", docNode!);

            //Assert
            Assert.NotNull(res);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);

            putHandlerMock.Verify(h => h.Handle
                (
                    It.Is<PutDocumentCommand>
                        (c => 
                            c.IndexId == "foo" && 
                            JsonNode.DeepEquals(c.Document, docNode)
                        ),
                    It.IsAny<CancellationToken>()
                    )
            );
        }

        [Fact]
        public async Task ShouldPatchDocument()
        {
            //Arrange
            var patchHandlerMock = new Mock<IRequestHandler<PatchDocumentCommand>>();
            var client = _fxt.StartWithProxy(srv =>
            {
                srv.AddTransient(_ => patchHandlerMock.Object);
            });

            const string docJson = """{"id": "bar", "baz": "qoz"}""";
            var docNode = JsonNode.Parse(docJson);

            //Act
            var res = await client.ApiClient.PatchAsync("foo", docNode!);

            //Assert
            Assert.NotNull(res);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);

            patchHandlerMock.Verify(h => h.Handle
                (
                    It.Is<PatchDocumentCommand>
                    (c =>
                        c.IndexId == "foo" &&
                        JsonNode.DeepEquals(c.DocumentPart, docNode)
                    ),
                    It.IsAny<CancellationToken>()
                )
            );
        }

        [Fact]
        public async Task ShouldDeleteDocument()
        {
            //Arrange
            var deleteHandlerMock = new Mock<IRequestHandler<DeleteDocumentCommand>>();
            var client = _fxt.StartWithProxy(srv =>
            {
                srv.AddTransient(_ => deleteHandlerMock.Object);
            });

            //Act
            var res = await client.ApiClient.DeleteAsync("foo", "bar");

            //Assert
            Assert.NotNull(res);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);

            deleteHandlerMock.Verify(h => h.Handle
                (
                    It.Is<DeleteDocumentCommand>
                    (c =>
                        c.IndexId == "foo" &&
                        c.DocumentId == "bar"
                    ),
                    It.IsAny<CancellationToken>()
                )
            );
        }

        [Fact]
        public async Task ShouldFailIfWrongIndexId()
        {
            //Arrange
            var putHandlerMock = new Mock<IRequestHandler<PutDocumentCommand>>();
            var client = _fxt.StartWithProxy(srv =>
            {
                srv.AddTransient(_ => putHandlerMock.Object);
            });

            const string docJson = """{"id": "bar", "baz": "qoz"}""";
            var docNode = JsonNode.Parse(docJson);

            //Act
            var res = await client.ApiClient.PutAsync("", docNode!);

            //Assert
            Assert.NotNull(res);
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);

            putHandlerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldFailIfDocumentIdNotFound()
        {
            //Arrange
            var putHandlerMock = new Mock<IRequestHandler<PutDocumentCommand>>();
            var client = _fxt.StartWithProxy(srv =>
            {
                srv.AddTransient(_ => putHandlerMock.Object);
            });

            const string docJson = """{"baz": "qoz"}""";
            var docNode = JsonNode.Parse(docJson);

            //Act
            var res = await client.ApiClient.PutAsync("foo", docNode!);

            //Assert
            Assert.NotNull(res);
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);

            putHandlerMock.VerifyNoOtherCalls();
        }
    }
}

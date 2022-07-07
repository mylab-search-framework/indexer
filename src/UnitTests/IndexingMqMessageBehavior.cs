using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MyLab.Search.Indexer.Models;
using MyLab.Search.Indexer.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace UnitTests
{
    public class IndexingMqMessageBehavior
    {
        [Fact]
        public void ShouldValidateRequest()
        {
            //Arrange
            var request = new IndexingMqMessage
            {
                IndexId = "bar",
                Post = new[]
                {
                    JObject.FromObject(new TestIdDoc())
                },
                Put = new[]
                {
                    JObject.FromObject(new TestIdDoc())
                },
                Patch = new[]
                {
                    JObject.FromObject(new TestIdDoc())
                },
                Kick = new[]
                {
                    "foo"
                },
            };

            //Act and Assert
            request.Validate();
        }

        [Fact]
        public void ShouldValidateRequestWithoutLists()
        {
            //Arrange
            var request = new IndexingMqMessage
            {
                IndexId = "bar"
            };

            //Act and Assert
            request.Validate();
        }

        [Fact]
        public void ShouldNotValidateIfIndexIdNotSpecified()
        {
            //Arrange
            var request = new IndexingMqMessage
            {
            };

            //Act and Assert
            Assert.Throws<ValidationException>(() => request.Validate());
        }

        [Fact]
        public void ShouldNotValidateIfPutDocWithoutIdProperty()
        {
            //Arrange
            var request = new IndexingMqMessage
            {
                IndexId = "bar",
                Post = new[]
                {
                    JObject.FromObject(new TestDoc())
                },
                Put = new[]
                {
                    JObject.FromObject(new TestDoc()),
                },
                Patch = new[]
                {
                    JObject.FromObject(new TestIdDoc())
                },
                Kick = new[]
                {
                    "foo"
                },
            };

            //Act and Assert
            Assert.Throws<ValidationException>(() => request.Validate());
        }

        [Fact]
        public void ShouldNotValidateIfPatchDocWithoutIdProperty()
        {
            //Arrange
            var request = new IndexingMqMessage
            {
                IndexId = "bar",
                Post = new[]
                {
                    JObject.FromObject(new TestDoc()),
                },
                Put = new[]
                {
                    JObject.FromObject(new TestIdDoc())
                },
                Patch = new[]
                {
                    JObject.FromObject(new TestDoc()),
                },
                Kick = new[]
                {
                    "foo"
                },
            };

            //Act and Assert
            Assert.Throws<ValidationException>(() => request.Validate());
        }

        [Fact]
        public void ShouldBeDeserializable()
        {
            //Arrange
            

            //Act
            var jsonObj = JObject.Parse(
                "{\"indexId\":\"foo\",\"post\":[{\"Id\":\"3b60d17d9fa54708a25148dac6717bdb\"}],\"put\":null,\"patch\":null,\"kick\":[\"bar\"]}");
            var req = jsonObj.ToObject<IndexingMqMessage>();

            //Assert
            Assert.NotNull(req);
            Assert.Equal("foo", req.IndexId);
            Assert.NotNull(req.Post);
            Assert.Single(req.Post);
            Assert.NotNull(req.Post[0].Property("Id"));
            Assert.Equal("3b60d17d9fa54708a25148dac6717bdb", req.Post[0]["Id"].ToString());
            Assert.Equal("bar", req.Kick?.FirstOrDefault());
        }

        [Fact]
        public void ShouldExtractIndexingRequest()
        {
            //Arrange

            var postEntWithoutId = new TestDoc
            {
                Content = Guid.NewGuid().ToString("N")
            };

            var postEnt = TestIdDoc.Generate();
            var putEnt = TestIdDoc.Generate();
            var patchEnt = TestIdDoc.Generate();
            var deleteId = Guid.NewGuid().ToString("N");
            var kickId = Guid.NewGuid().ToString("N");

            var mqMsg = new IndexingMqMessage
            {
                IndexId = "foo",
                Post = new[]
                {
                    JObject.FromObject(postEntWithoutId),
                    JObject.FromObject(postEnt)
                },
                Put = new[]
                {
                    JObject.FromObject(putEnt)
                },
                Patch = new[]
                {
                    JObject.FromObject(patchEnt)
                },
                Delete = new[]
                {
                    deleteId
                },
                Kick = new []
                {
                    kickId
                }
            };

            //Act
            var indexingRequest = mqMsg.ExtractIndexingRequest();

            //Assert
            Assert.Equal("foo", indexingRequest.IndexId);

            Assert.Equal(2, indexingRequest.PostList.Length);
            Assert.Null(indexingRequest.PostList[0].GetIdProperty());
            Assert.Equal(postEntWithoutId.Content, indexingRequest.PostList[0]?.ToObject<TestDoc>()?.Content);
            Assert.Equal(postEnt.Id, indexingRequest.PostList[1].GetIdProperty());
            Assert.Equal(postEnt.Content, indexingRequest.PostList[1]?.ToObject<TestIdDoc>()?.Content);

            Assert.Single(indexingRequest.PutList);
            Assert.Equal(putEnt.Id, indexingRequest.PutList[0].GetIdProperty());
            Assert.Equal(putEnt.Content, indexingRequest.PutList[0]?.ToObject<TestIdDoc>()?.Content);

            Assert.Single(indexingRequest.PatchList);
            Assert.Equal(patchEnt.Id, indexingRequest.PatchList[0].GetIdProperty());
            Assert.Equal(patchEnt.Content, indexingRequest.PatchList[0]?.ToObject<TestIdDoc>()?.Content);

            Assert.Single(indexingRequest.DeleteList);
            Assert.Equal(deleteId, indexingRequest.DeleteList[0]);

            Assert.Single(indexingRequest.KickList);
            Assert.Equal(kickId, indexingRequest.KickList[0]);
        }

        class TestDoc
        {
            public string Content { get; set; }
        }

        class TestIdDoc
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("content")]
            public string Content { get; set; }

            public static TestIdDoc Generate()
            {
                return new TestIdDoc
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Content = Guid.NewGuid().ToString("N")
                };
            }
        }
    }
}

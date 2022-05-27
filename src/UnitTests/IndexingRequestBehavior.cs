using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MyLab.Search.Indexer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace UnitTests
{
    public class IndexingRequestBehavior
    {
        [Fact]
        public void ShouldValidateRequest()
        {
            //Arrange
            var request = new IndexingRequest
            {
                IndexId = "bar",
                Post = new[]
                {
                    JObject.FromObject(new TestEntity())
                },
                Put = new[]
                {
                    JObject.FromObject(new TestIdEntity())
                },
                Patch = new[]
                {
                    JObject.FromObject(new TestIdEntity())
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
            var request = new IndexingRequest
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
            var request = new IndexingRequest
            {
            };

            //Act and Assert
            Assert.Throws<ValidationException>(() => request.Validate());
        }

        [Fact]
        public void ShouldNotValidateIfPutEntityWithoutIdProperty()
        {
            //Arrange
            var request = new IndexingRequest
            {
                IndexId = "bar",
                Post = new[]
                {
                    JObject.FromObject(new TestEntity())
                },
                Put = new[]
                {
                    JObject.FromObject(new TestEntity()),
                },
                Patch = new[]
                {
                    JObject.FromObject(new TestIdEntity())
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
        public void ShouldNotValidateIfPatchEntityWithoutIdProperty()
        {
            //Arrange
            var request = new IndexingRequest
            {
                IndexId = "bar",
                Post = new[]
                {
                    JObject.FromObject(new TestEntity()),
                },
                Put = new[]
                {
                    JObject.FromObject(new TestIdEntity())
                },
                Patch = new[]
                {
                    JObject.FromObject(new TestEntity()),
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
            var req = jsonObj.ToObject<IndexingRequest>();

            //Assert
            Assert.NotNull(req);
            Assert.Equal("foo", req.IndexId);
            Assert.NotNull(req.Post);
            Assert.Single(req.Post);
            Assert.NotNull(req.Post[0].Property("Id"));
            Assert.Equal("3b60d17d9fa54708a25148dac6717bdb", req.Post[0]["Id"].ToString());
            Assert.Equal("bar", req.Kick?.FirstOrDefault());
        }

        class TestEntity
        {

        }

        class TestIdEntity
        {
            [JsonProperty("id")]
            public string Id { get; set; }
        }
    }
}

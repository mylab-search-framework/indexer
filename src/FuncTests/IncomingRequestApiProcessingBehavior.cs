using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MyLab.Search.Indexer.Services;
using Newtonsoft.Json.Linq;
using Xunit;

namespace FuncTests
{
    public partial class IncomingRequestApiProcessingBehavior : IDisposable
    {
        [Fact]
        public async Task ShouldProcessPostWithoutId()
        {
            //Arrange
            var inputSrvProc = new TestInputRequestProcessor();

            var api = _testApi.StartWithProxy(srv => 
                    srv.AddSingleton<IInputRequestProcessor>(inputSrvProc)
                );

            var testEntity = new TestEntity(null,"foo-content");

            TestEntity actualEntity = null;

            //Act
            await api.PostAsync("foo-index", JObject.FromObject(testEntity));
            
            var item = inputSrvProc.LastRequest?.PostList?.FirstOrDefault();

            if (item?.Entity != null)
                actualEntity = item.Entity.ToObject<TestEntity>();

            //Assert
            Assert.Equal("foo-index", inputSrvProc.LastRequest?.IndexId);
            Assert.NotNull(actualEntity);
            Assert.Null(item.Id);
            Assert.Equal(testEntity.Content, actualEntity.Content);
        }

        [Fact]
        public async Task ShouldProcessPost()
        {
            //Arrange
            var inputSrvProc = new TestInputRequestProcessor();

            var api = _testApi.StartWithProxy(srv =>
                srv.AddSingleton<IInputRequestProcessor>(inputSrvProc)
            );

            var testEntity = new TestEntity("foo-id","foo-content");

            TestEntity actualEntity = null;

            //Act
            await api.PostAsync("foo-index", JObject.FromObject(testEntity));

            var item = inputSrvProc.LastRequest?.PostList?.FirstOrDefault();
            
            if (item?.Entity != null)
                actualEntity = item.Entity.ToObject<TestEntity>();

            //Assert
            Assert.Equal("foo-index", inputSrvProc.LastRequest?.IndexId);
            Assert.NotNull(actualEntity);
            Assert.Equal("foo-id", item.Id);
            Assert.Equal("foo-content", actualEntity.Content);
        }

        [Fact]
        public async Task ShouldProcessPut()
        {
            //Arrange
            var inputSrvProc = new TestInputRequestProcessor();

            var api = _testApi.StartWithProxy(srv =>
                srv.AddSingleton<IInputRequestProcessor>(inputSrvProc)
            );

            var testEntity = new TestEntity("foo-id","foo-content");

            TestEntity actualEntity = null;

            //Act
            await api.PutAsync("foo-index", JObject.FromObject(testEntity));

            var item = inputSrvProc.LastRequest?.PutList?.FirstOrDefault();

            if (item?.Entity != null)
                actualEntity = item.Entity.ToObject<TestEntity>();

            //Assert
            Assert.Equal("foo-index", inputSrvProc.LastRequest?.IndexId);
            Assert.NotNull(actualEntity);
            Assert.Equal("foo-id", item.Id);
            Assert.Equal("foo-content", actualEntity.Content);
        }

        [Fact]
        public async Task ShouldProcessPatch()
        {
            //Arrange
            var inputSrvProc = new TestInputRequestProcessor();

            var api = _testApi.StartWithProxy(srv =>
                srv.AddSingleton<IInputRequestProcessor>(inputSrvProc)
            );

            var testEntity = new TestEntity("foo-id","foo-content");

            TestEntity actualEntity = null;

            //Act
            await api.PatchAsync("foo-index", JObject.FromObject(testEntity));

            var item = inputSrvProc.LastRequest?.PatchList?.FirstOrDefault();

            if (item?.Entity != null)
                actualEntity = item.Entity.ToObject<TestEntity>();

            //Assert
            
            Assert.NotNull(inputSrvProc.LastRequest);
            Assert.Equal("foo-index", inputSrvProc.LastRequest?.IndexId);
            Assert.NotNull(actualEntity);
            Assert.Equal("foo-id", item.Id);
            Assert.Equal("foo-content", actualEntity.Content);
        }

        [Fact]
        public async Task ShouldProcessDelete()
        {
            //Arrange
            var inputSrvProc = new TestInputRequestProcessor();

            var api = _testApi.StartWithProxy(srv =>
                srv.AddSingleton<IInputRequestProcessor>(inputSrvProc)
            );
            

            //Act
            await api.DeleteAsync("foo-index", "foo-id");

            var item = inputSrvProc.LastRequest?.DeleteList?.FirstOrDefault();
            
            //Assert
            Assert.Equal("foo-index", inputSrvProc.LastRequest?.IndexId);
            Assert.Equal("foo-id", item);
        }
    }
}

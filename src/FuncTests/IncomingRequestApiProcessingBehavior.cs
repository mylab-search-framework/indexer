using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Tools;
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
            var inputSrvProc = new IncomingRequestApiProcessingBehavior.TestInputRequestProcessor();

            var api = _testApi.StartWithProxy(srv =>
                    ServiceCollectionServiceExtensions.AddSingleton<IInputRequestProcessor>(srv, inputSrvProc)
                );

            var testDoc = TestDoc.Generate(null);

            //Act
            await api.PostAsync("foo-index", JObject.FromObject(testDoc));

            var item = inputSrvProc.LastRequest?.PostList?.FirstOrDefault();
            
            var actualDoc = item?.ToObject<TestDoc>();

            //Assert
            Assert.Equal("foo-index", inputSrvProc.LastRequest?.IndexId);
            Assert.NotNull(actualDoc);
            Assert.Null(item.GetIdProperty());
            Assert.Equal(testDoc.Content, actualDoc.Content);
        }

        [Fact]
        public async Task ShouldProcessPost()
        {
            //Arrange
            var inputSrvProc = new IncomingRequestApiProcessingBehavior.TestInputRequestProcessor();

            var api = _testApi.StartWithProxy(srv =>
                ServiceCollectionServiceExtensions.AddSingleton<IInputRequestProcessor>(srv, inputSrvProc)
            );

            var testDoc = TestDoc.Generate();

            //Act
            await api.PostAsync("foo-index", JObject.FromObject(testDoc));

            var item = inputSrvProc.LastRequest?.PostList?.FirstOrDefault();
            
            var actualDoc = item?.ToObject<TestDoc>();

            //Assert
            Assert.Equal("foo-index", inputSrvProc.LastRequest?.IndexId);
            Assert.NotNull(actualDoc);
            Assert.Equal(testDoc.Id, item.GetIdProperty());
            Assert.Equal(testDoc.Content, actualDoc.Content);
        }

        [Fact]
        public async Task ShouldProcessPut()
        {
            //Arrange
            var inputSrvProc = new IncomingRequestApiProcessingBehavior.TestInputRequestProcessor();

            var api = _testApi.StartWithProxy(srv =>
                ServiceCollectionServiceExtensions.AddSingleton<IInputRequestProcessor>(srv, inputSrvProc)
            );

            var testDoc = TestDoc.Generate();


            //Act
            await api.PutAsync("foo-index", JObject.FromObject(testDoc));

            var item = inputSrvProc.LastRequest?.PutList?.FirstOrDefault();

            var actualDoc = item?.ToObject<TestDoc>();

            //Assert
            Assert.Equal("foo-index", inputSrvProc.LastRequest?.IndexId);
            Assert.NotNull(actualDoc);
            Assert.Equal(testDoc.Id, item.GetIdProperty());
            Assert.Equal(testDoc.Content, actualDoc.Content);
        }

        [Fact]
        public async Task ShouldProcessPatch()
        {
            //Arrange
            var inputSrvProc = new IncomingRequestApiProcessingBehavior.TestInputRequestProcessor();

            var api = _testApi.StartWithProxy(srv =>
                ServiceCollectionServiceExtensions.AddSingleton<IInputRequestProcessor>(srv, inputSrvProc)
            );

            var testDoc = TestDoc.Generate();

            //Act
            await api.PatchAsync("foo-index", JObject.FromObject(testDoc));

            var item = inputSrvProc.LastRequest?.PatchList?.FirstOrDefault();

            var actualDoc = item?.ToObject<TestDoc>();

            //Assert

            Assert.NotNull(inputSrvProc.LastRequest);
            Assert.Equal("foo-index", inputSrvProc.LastRequest?.IndexId);
            Assert.NotNull(actualDoc);
            Assert.Equal(testDoc.Id, item.GetIdProperty());
            Assert.Equal(testDoc.Content, actualDoc.Content);
        }

        [Fact]
        public async Task ShouldProcessDelete()
        {
            //Arrange
            var inputSrvProc = new IncomingRequestApiProcessingBehavior.TestInputRequestProcessor();

            var api = _testApi.StartWithProxy(srv =>
                ServiceCollectionServiceExtensions.AddSingleton<IInputRequestProcessor>(srv, inputSrvProc)
            );


            //Act
            await api.DeleteAsync("foo-index", "foo-id");

            var item = inputSrvProc.LastRequest?.DeleteList?.FirstOrDefault();

            //Assert
            Assert.Equal("foo-index", inputSrvProc.LastRequest?.IndexId);
            Assert.Equal("foo-id", item);
        }

        [Fact]
        public async Task ShouldProcessKick()
        {
            //Arrange
            var inputSrvProc = new IncomingRequestApiProcessingBehavior.TestInputRequestProcessor();

            var api = _testApi.StartWithProxy(srv =>
                ServiceCollectionServiceExtensions.AddSingleton<IInputRequestProcessor>(srv, inputSrvProc)
            );


            //Act
            await api.KickAsync("foo-index", "foo-id");

            var item = inputSrvProc.LastRequest?.KickList?.FirstOrDefault();

            //Assert
            Assert.Equal("foo-index", inputSrvProc.LastRequest?.IndexId);
            Assert.Equal("foo-id", item);
        }
    }
}

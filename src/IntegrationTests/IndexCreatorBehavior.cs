using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MyLab.Log.XUnit;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Services.ResourceUploading;
using MyLab.Search.Indexer.Tools;
using Nest;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using IndexOptions = MyLab.Search.Indexer.Options.IndexOptions;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace IntegrationTests
{
    public class IndexCreatorBehavior : IClassFixture<EsFixture<TestEsFixtureStrategy>>, IAsyncLifetime
    {
        private readonly EsFixture<TestEsFixtureStrategy> _fxt;
        private readonly ITestOutputHelper _output;
        private readonly string _indexName;

        public IndexCreatorBehavior(EsFixture<TestEsFixtureStrategy> fxt, ITestOutputHelper output)
        {
            fxt.Output = output;
            _fxt = fxt;
            _output = output;

            _indexName = "test-index-" + nameof(IndexCreatorBehavior).ToLower() + "-" + Guid.NewGuid().ToString("N");
        }

        [Fact]
        public async Task ShouldCreateIndexWithSettings()
        {
            //Arrange
            TypeMapping mapping = new TypeMapping
            {
                Properties = new Properties
                {
                    { new PropertyName("text"), new TextProperty() }
                }
            };

            var idxMappingProvider = new Mock<IIndexMappingProvider>();

            idxMappingProvider.Setup(p => p.ProvideAsync(It.Is<string>(s => s == _indexName)))
                .Returns<string>(_ => Task.FromResult( 
                    new IndexMappingDesc
                    {
                        Mapping = mapping,
                        SourceHash = "hash"
                    })
                );

            var srv = new ServiceCollection()
                .AddLogging(l => l
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddXUnit(_output)
                )
                .Configure<IndexerOptions>(o =>
                {
                    o.EnableEsIndexAutoCreation = true;
                })
                .AddSingleton(_fxt.Tools)
                .AddSingleton(idxMappingProvider.Object)
                .AddSingleton<IndexCreator>()
                .BuildServiceProvider();

            var indexCreator = srv.GetService<IndexCreator>();

            //Act
            await indexCreator.CreateIndexAsync(_indexName, _indexName, CancellationToken.None);

            var indexInfo = await _fxt.Tools.Index(_indexName).TryGetAsync();

            //Assert
            Assert.NotNull(indexInfo);
            Assert.NotNull(indexInfo.Mappings);
            Assert.NotNull(indexInfo.Mappings.Properties);
            Assert.Single(indexInfo.Mappings.Properties);
            Assert.Contains(indexInfo.Mappings.Properties, p => p.Key == "text" && p.Value is TextProperty);
        }

        [Fact]
        public async Task ShouldAddIndexMapping()
        {
            //Arrange
            var indexTemplateName = "test-index-template" + Guid.NewGuid().ToString("N");

            var idxTemplateMappingMeta = new MappingMetadata
            {
                Template = new MappingMetadata.TemplateDesc
                {
                    Owner = "foo-owner",
                    SourceName = indexTemplateName
                }
            };

            var idxTemplateMappingMetaDict = new Dictionary<string, object>();
            idxTemplateMappingMeta.Save(idxTemplateMappingMetaDict);

            var indexTemplateRequest = new PutIndexTemplateV2Request(indexTemplateName)
            {
                Template = new Template
                {
                    Mappings = new TypeMapping
                    {
                        Properties = new Properties
                        {
                            { "number", new NumberProperty() }
                        },
                        Meta = idxTemplateMappingMetaDict
                    }
                },
                IndexPatterns = new[] { _indexName }
            };

            await using var idxTemplateDisposer = await _fxt.Tools.IndexTemplate(indexTemplateName).PutAsync(indexTemplateRequest);

            TypeMapping mapping = new TypeMapping
            {
                Properties = new Properties
                {
                    { new PropertyName("text"), new TextProperty() }
                },
            };

            var idxMappingProvider = new Mock<IIndexMappingProvider>();

            idxMappingProvider.Setup(p => p.ProvideAsync(It.Is<string>(s => s == _indexName)))
                .Returns<string>(_ => Task.FromResult(
                    new IndexMappingDesc
                    {
                        Mapping = mapping,
                        SourceHash = "hash"
                    })
                );

            var srv = new ServiceCollection()
                .AddLogging(l => l
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddXUnit(_output)
                )
                .Configure<IndexerOptions>(o =>
                {
                    o.EnableEsIndexAutoCreation = true;
                    o.AppId = "test-app";
                })
                .AddSingleton(_fxt.Tools)
                .AddSingleton(idxMappingProvider.Object)
                .AddSingleton<IndexCreator>()
                .BuildServiceProvider();

            var indexCreator = srv.GetService<IndexCreator>();

            //Act
            await indexCreator.CreateIndexAsync(_indexName, _indexName, CancellationToken.None);

            var indexInfo = await _fxt.Tools.Index(_indexName).TryGetAsync();

            MappingMetadata idxMappingMeta = null;

            if (indexInfo?.Mappings?.Meta != null)
            {
                MappingMetadata.TryGet(indexInfo.Mappings.Meta, out idxMappingMeta);
            }

            //Assert
            Assert.NotNull(indexInfo);
            Assert.NotNull(indexInfo.Mappings);
            Assert.NotNull(indexInfo.Mappings.Properties);
            Assert.Contains(indexInfo.Mappings.Properties, p => p.Key == "text" && p.Value is TextProperty);
            Assert.Contains(indexInfo.Mappings.Properties, p => p.Key == "number" && p.Value is NumberProperty);
            
            Assert.NotNull(idxMappingMeta);
            Assert.NotNull(idxMappingMeta.Creator);
            Assert.Equal("test-app",idxMappingMeta.Creator.Owner);
            Assert.Equal("hash",idxMappingMeta.Creator.SourceHash);

            Assert.NotNull(idxMappingMeta.Template);
            Assert.Equal("foo-owner", idxMappingMeta.Template.Owner);
            Assert.Equal(indexTemplateName, idxMappingMeta.Template.SourceName);
        }

        [Fact]
        public async Task ShouldNotCreateIndexWhenAutoCreationIsDisabled()
        {
            //Arrange
            var idxMappingProvider = new Mock<IIndexMappingProvider>();

            var srv = new ServiceCollection()
                .AddLogging(l => l
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddXUnit(_output)
                )
                .Configure<IndexerOptions>(o =>
                {
                    o.EnableEsIndexAutoCreation = false;
                })
                .AddSingleton(_fxt.Tools)
                .AddSingleton(idxMappingProvider.Object)
                .AddSingleton<IndexCreator>()
                .BuildServiceProvider();

            var indexCreator = srv.GetService<IndexCreator>();

            //Act & Assert
            await Assert.ThrowsAsync<IndexCreationDeniedException>(() =>
                indexCreator.CreateIndexAsync(_indexName, _indexName, CancellationToken.None));
        }

        string SerializeMapping(TypeMapping mapping)
        {
            using var stream = new MemoryStream();
            _fxt.Tools.Serializer.Serialize(mapping, stream, SerializationFormatting.Indented);

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            _fxt.Output = null;
            var indexExists = await _fxt.Tools.Index(_indexName).ExistsAsync();
            if(indexExists)
                await _fxt.Tools.Index(_indexName).DeleteAsync();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MyLab.Log.XUnit;
using MyLab.Search.EsTest;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using MyLab.Search.Indexer.Services.ComponentUploading;
using MyLab.Search.Indexer.Tools;
using Nest;
using Xunit;
using Xunit.Abstractions;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace IntegrationTests
{
    public class IndexCreatorBehavior : IClassFixture<EsFixture<TestEsFixtureStrategy>> , IAsyncLifetime
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
        public async Task ShouldCreateIndexWithMapping()
        {
            //Arrange
            TypeMapping mapping = new TypeMapping
            {
                Properties = new Properties
                {
                    { new PropertyName("text"), new TextProperty() }
                }
            };

            var resourceProvider= new Mock<IResourceProvider>();
            resourceProvider.SetupGet(p => p.IndexDirectory)
                .Returns(() => new IndexResourceDirectory
                {
                    Named = new Dictionary<string, IndexResources>
                    {
                        {
                            _indexName,
                            new IndexResources
                            {
                                Mapping = new Resource<TypeMapping>
                                {
                                    Content = mapping,
                                    Name = "foo",
                                    Hash = "hash"
                                }
                            }
                        }
                    }
                });

            var srv = new ServiceCollection()
                .AddLogging(l => l
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddXUnit(_output)
                )
                .Configure<IndexerOptions>(o =>
                {
                    o.EnableAutoCreation = true;
                })
                .AddSingleton(_fxt.Tools)
                .AddSingleton<IndexCreator>()
                .AddSingleton(resourceProvider.Object)
                .BuildServiceProvider();

            var indexCreator = srv.GetRequiredService<IndexCreator>();
            IndexState indexInfo;

            //Act
            var createdIndexDescription = await indexCreator.CreateIndexAsync(_indexName, CancellationToken.None);

            try
            {
                indexInfo = await _fxt.Tools.Index(createdIndexDescription.Name).TryGetAsync();
            }
            finally
            {
                await createdIndexDescription.Deleter.DisposeAsync();
            }

            //Assert
            Assert.NotNull(indexInfo);
            Assert.NotNull(indexInfo.Mappings);
            Assert.NotNull(indexInfo.Mappings.Properties);
            Assert.Single(indexInfo.Mappings.Properties);
            Assert.Contains(indexInfo.Mappings.Properties, p => p.Key == "text" && p.Value is TextProperty);
        }

        [Fact]
        public async Task ShouldCreateStream()
        {
            //Arrange
            var resourceProvider = new Mock<IResourceProvider>();
            resourceProvider.SetupGet(p => p.IndexDirectory)
                .Returns(() => new IndexResourceDirectory());

            var srv = new ServiceCollection()
                .AddLogging(l => l
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddXUnit(_output)
                )
                .Configure<IndexerOptions>(o =>
                {
                    o.EnableAutoCreation = true;
                    o.DefaultIndex.IsStream = true;
                })
                .AddSingleton(_fxt.Tools)
                .AddSingleton<IndexCreator>()                          
                .AddSingleton(resourceProvider.Object)                          
                .BuildServiceProvider();

            var indexTemplateName = "test-index-template" + Guid.NewGuid().ToString("N");
            var templateDisposer = await CreateIndexTemplate(indexTemplateName, "foo-owner", true);

            var indexCreator = srv.GetRequiredService<IndexCreator>();
            bool streamExists;

            //Act

            var createdIndexDescription = await indexCreator.CreateIndexAsync(_indexName, CancellationToken.None);

            try
            {
                streamExists = await _fxt.Tools.Stream(createdIndexDescription.Name).ExistsAsync();
            }
            finally
            {
                await createdIndexDescription.Deleter.DisposeAsync();
                await templateDisposer.DisposeAsync();
            }

            //Assert
            Assert.True(streamExists);
        }

        [Fact]
        public async Task ShouldAddIndexMapping()
        {
            //Arrange
            var indexTemplateName = "test-index-template-" + Guid.NewGuid().ToString("N");

            TypeMapping mapping = new TypeMapping
            {
                Properties = new Properties
                {
                    { new PropertyName("text"), new TextProperty() }
                },
            };

            var resourceProvider = new Mock<IResourceProvider>();
            resourceProvider.SetupGet(p => p.IndexDirectory)
                .Returns(() => new IndexResourceDirectory
                {
                    Named = new Dictionary<string, IndexResources>
                    {
                        {
                            _indexName,
                            new IndexResources
                            {
                                Mapping = new Resource<TypeMapping>
                                {
                                    Content = mapping,
                                    Name = "foo",
                                    Hash = "hash"
                                }
                            }
                        }
                    }
                });

            var srv = new ServiceCollection()
                .AddLogging(l => l
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddXUnit(_output)
                )
                .Configure<IndexerOptions>(o =>
                {
                    o.EnableAutoCreation = true;
                    o.AppId = "test-app";
                })
                .AddSingleton(_fxt.Tools)
                .AddSingleton(resourceProvider.Object)
                .AddSingleton<IndexCreator>()
                .BuildServiceProvider();

            var indexCreator = srv.GetRequiredService<IndexCreator>();
            IndexState indexInfo;
            MappingMetadata idxMappingMeta = null;

            //Act
            CreatedIndexDescription createdIndexDescription;

            await using (var templateDisposer = await CreateIndexTemplate(indexTemplateName, "foo-owner", false))
            {
                createdIndexDescription = await indexCreator.CreateIndexAsync(_indexName, CancellationToken.None);
            }

            try
            {
                indexInfo = await _fxt.Tools.Index(createdIndexDescription.Name).TryGetAsync();

                if (indexInfo?.Mappings?.Meta != null)
                {
                    MappingMetadata.TryGet(indexInfo.Mappings.Meta, out idxMappingMeta);
                }

            }
            finally
            {
                await createdIndexDescription.Deleter.DisposeAsync();
            }

            //Assert
            Assert.False(createdIndexDescription.IsStream);
            Assert.Equal(_indexName, createdIndexDescription.Alias);
            Assert.Contains(_indexName, createdIndexDescription.Name);

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
            var resourceProvider = new Mock<IResourceProvider>();

            var srv = new ServiceCollection()
                .AddLogging(l => l
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddXUnit(_output)
                )
                .Configure<IndexerOptions>(o =>
                {
                    o.EnableAutoCreation = false;
                })
                .AddSingleton(_fxt.Tools)
                .AddSingleton(resourceProvider.Object)
                .AddSingleton<IndexCreator>()
                .BuildServiceProvider();

            var indexCreator = srv.GetRequiredService<IndexCreator>();

            //Act & Assert
            await Assert.ThrowsAsync<IndexCreationDeniedException>(() =>
                indexCreator.CreateIndexAsync(_indexName, CancellationToken.None));
        }

        private async Task<IAsyncDisposable> CreateIndexTemplate(string templateName, string owner, bool forStream)
        {
            var idxTemplateMappingMeta = new MappingMetadata
            {
                Template = new MappingMetadata.TemplateDesc
                {
                    Owner = owner,
                    SourceName = templateName
                }
            };

            var idxTemplateMappingMetaDict = new Dictionary<string, object>();
            idxTemplateMappingMeta.Save(idxTemplateMappingMetaDict);

            Func<PutIndexTemplateV2Descriptor, IPutIndexTemplateV2Request> putDescriptor = d =>
            {
                var res = d.Template(td => td
                        .Mappings(md => md
                            .Properties(pd => pd
                                .Number(npd => npd.Name("number"))
                            )
                            .Meta(idxTemplateMappingMetaDict)
                        )
                    )
                    .IndexPatterns(UnderneathName.ToPattern(_indexName));

                if(forStream)
                    res = res.DataStream(new DataStream());

                return res;
            };

            return await _fxt.Tools.IndexTemplate(templateName).PutAsync(putDescriptor);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}

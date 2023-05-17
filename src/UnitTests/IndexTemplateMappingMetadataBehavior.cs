using MyLab.Search.Indexer.Services.ResourceUploading;
using System.Collections.Generic;
using MyLab.Search.Indexer.Tools;
using Xunit;

namespace UnitTests
{
    public class IndexTemplateMappingMetadataBehavior
    {
        [Fact]
        public void ShouldBeDeserializable()
        {
            //Arrange
            var dict = new Dictionary<string, object>
            {
                {
                    IndexTemplateMappingMetadata.MetadataKey,
                    new Dictionary<string,object>
                    {
                        {
                            "foo-index-template",
                            new Dictionary<string,object>
                            {
                                {"owner", "foo-owner"},
                                {"source_name", "foo-source"}
                            }
                        },
                        {
                            "bar-index-template",
                            new Dictionary<string,object>
                            {
                                {"owner", "bar-owner"},
                                {"source_name", "bar-source"}
                            }
                        }
                    }
                }
            };

            //Act
            IndexTemplateMappingMetadata.TryGet(dict, out var templateMetadata);

            templateMetadata.Entities.TryGetValue("foo-index-template", out var fooTemplateMetadata);
            templateMetadata.Entities.TryGetValue("bar-index-template", out var barTemplateMetadata);

            //Assert
            Assert.NotNull(fooTemplateMetadata);
            Assert.Equal("foo-owner", fooTemplateMetadata.Owner);
            Assert.Equal("foo-source", fooTemplateMetadata.SourceName);
            Assert.NotNull(barTemplateMetadata);
            Assert.Equal("bar-owner", barTemplateMetadata.Owner);
            Assert.Equal("bar-source", barTemplateMetadata.SourceName);
        }

        [Fact]
        public void ShouldSaveAndMerge()
        {
            //Arrange
            var dict = new Dictionary<string, object>
            {
                {
                    IndexTemplateMappingMetadata.MetadataKey,
                    new Dictionary<string,object>
                    {
                        {
                            "foo-index-template",
                            new Dictionary<string,object>
                            {
                                {"owner", "foo-owner"},
                                {"source_name", "foo-source"}
                            }
                        }
                    }
                }
            };

            var barMetadata = new IndexTemplateMappingMetadata(
                "bar-index-template",
                new IndexTemplateMappingMetadata.Item
                {
                    Owner = "bar-owner",
                    SourceName = "bar-source"
                }
            );

            //Act
            barMetadata.Save(dict);

            IndexTemplateMappingMetadata.TryGet(dict, out var templateMetadata);

            templateMetadata.Entities.TryGetValue("foo-index-template", out var fooTemplateMetadata);
            templateMetadata.Entities.TryGetValue("bar-index-template", out var barTemplateMetadata);

            //Assert
            Assert.NotNull(fooTemplateMetadata);
            Assert.Equal("foo-owner", fooTemplateMetadata.Owner);
            Assert.Equal("foo-source", fooTemplateMetadata.SourceName);
            Assert.NotNull(barTemplateMetadata);
            Assert.Equal("bar-owner", barTemplateMetadata.Owner);
            Assert.Equal("bar-source", barTemplateMetadata.SourceName);
        }
    }
}

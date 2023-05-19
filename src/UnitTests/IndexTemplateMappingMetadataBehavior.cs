using MyLab.Search.Indexer.Services.ResourceUploading;
using System.Collections.Generic;
using System.Linq;
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
                    MappingMetadata.MetadataKey,
                    new Dictionary<string,object>
                    {
                        {
                            MappingMetadata.CreatorKey,
                            new Dictionary<string,object>
                            {
                                {"owner", "foo-owner"},
                                {"source_hash", "foo-hash"}
                            }
                        },
                        {
                            MappingMetadata.TemplateKey,
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
            MappingMetadata.TryGet(dict, out var templateMetadata);

            //Assert
            Assert.NotNull(templateMetadata);
            Assert.Equal("foo-owner", templateMetadata.Creator.Owner);
            Assert.Equal("foo-hash", templateMetadata.Creator.SourceHash);
            Assert.Equal("bar-owner", templateMetadata.Template.Owner);
            Assert.Equal("bar-source", templateMetadata.Template.SourceName);
        }

        [Fact]
        public void ShouldSave()
        {
            //Arrange
            var dict = new Dictionary<string, object>();

            var barMetadata = new MappingMetadata
            {
                Template = new MappingMetadata.TemplateDesc
                {
                    Owner = "bar-owner",
                    SourceName = "bar-source"
                },
                Creator = new MappingMetadata.CreatorDesc
                {
                    Owner = "foo-owner",
                    SourceHash = "foo-hash"
                }
            };

            IDictionary<string, object> creatorDict = null;
            IDictionary<string, object> templateDict = null;

            //Act
            barMetadata.Save(dict);

            dict.TryGetValue(MappingMetadata.MetadataKey, out var metaDictObj);
            if (metaDictObj is IDictionary<string, object> metadataDict)
            {
                metadataDict.TryGetValue(MappingMetadata.CreatorKey, out var creatorDictObj);
                creatorDict = creatorDictObj as IDictionary<string, object>;

                metadataDict.TryGetValue(MappingMetadata.TemplateKey, out var templateDictObj);
                templateDict = templateDictObj as IDictionary<string, object>;
            }


            //Assert
            Assert.NotNull(creatorDict);
            Assert.Contains(creatorDict, p => p.Key == "owner" && (string)p.Value == "foo-owner");
            Assert.Contains(creatorDict, p => p.Key == "source_hash" && (string)p.Value == "foo-hash");
            Assert.NotNull(templateDict);
            Assert.Contains(templateDict, p => p.Key == "owner" && (string)p.Value == "bar-owner");
            Assert.Contains(templateDict, p => p.Key == "source_name" && (string)p.Value == "bar-source");
        }
    }
}

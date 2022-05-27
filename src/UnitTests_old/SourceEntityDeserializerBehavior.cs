using MyLab.Search.Indexer;
using MyLab.Search.Indexer.DataContract;
using MyLab.Search.Indexer.Tools;
using Xunit;

namespace UnitTests
{
    public class SourceEntityDeserializerBehavior
    {
        [Fact]
        public void ShouldDeserialize()
        {
            //Arrange
            const string input = "{\"Id\":\"foo\",\"ValueStr\":\"bar\",\"ValueDouble\":\"1.1\",\"ValueInt\":\"123\",\"ValueBool\":\"true\",\"ValueDt\":\"2003-01-01 12:22:21\"}";

            var deserializer = new SourceEntityDeserializer(detectTypes: false);

            //Act
            var entity = deserializer.Deserialize(input);

            //Assert
            Assert.Equal("foo", entity.Properties["Id"].Value);
            Assert.Equal("bar", entity.Properties["ValueStr"].Value);
            Assert.Equal("1.1", entity.Properties["ValueDouble"].Value);
            Assert.Equal("123", entity.Properties["ValueInt"].Value);
            Assert.Equal("true", entity.Properties["ValueBool"].Value);
            Assert.Equal("2003-01-01 12:22:21", entity.Properties["ValueDt"].Value);
        }

        [Fact]
        public void ShouldDeserializeWithoutTypeDetection()
        {
            //Arrange
            const string input = "{\"Id\":\"foo\",\"ValueStr\":\"bar\",\"ValueDouble\":\"1.1\",\"ValueInt\":\"123\",\"ValueBool\":\"true\",\"ValueDt\":\"2003-01-01 12:22:21\"}";

            var deserializer = new SourceEntityDeserializer(detectTypes: false);

            //Act
            var entity = deserializer.Deserialize(input);

            //Assert
            Assert.Equal(DataSourcePropertyType.Undefined, entity.Properties["Id"].DbType);
            Assert.Equal(DataSourcePropertyType.Undefined, entity.Properties["ValueStr"].DbType);
            Assert.Equal(DataSourcePropertyType.Undefined, entity.Properties["ValueDouble"].DbType);
            Assert.Equal(DataSourcePropertyType.Undefined, entity.Properties["ValueInt"].DbType);
            Assert.Equal(DataSourcePropertyType.Undefined, entity.Properties["ValueBool"].DbType);
            Assert.Equal(DataSourcePropertyType.Undefined, entity.Properties["ValueDt"].DbType);
        }

        [Fact]
        public void ShouldDeserializeWithTypeDetection()
        {
            //Arrange
            const string input = "{\"Id\":\"foo\",\"ValueStr\":\"bar\",\"ValueDouble\":\"1.1\",\"ValueInt\":\"123\",\"ValueBool\":\"true\",\"ValueDt\":\"2003-01-01 12:22:21\"}";

            var deserializer = new SourceEntityDeserializer(detectTypes: true);

            //Act
            var entity = deserializer.Deserialize(input);

            //Assert
            Assert.Equal(DataSourcePropertyType.String, entity.Properties["Id"].DbType);
            Assert.Equal(DataSourcePropertyType.String, entity.Properties["ValueStr"].DbType);
            Assert.Equal(DataSourcePropertyType.Double, entity.Properties["ValueDouble"].DbType);
            Assert.Equal(DataSourcePropertyType.Numeric, entity.Properties["ValueInt"].DbType);
            Assert.Equal(DataSourcePropertyType.Boolean, entity.Properties["ValueBool"].DbType);
            Assert.Equal(DataSourcePropertyType.DateTime, entity.Properties["ValueDt"].DbType);
        }
    }
}

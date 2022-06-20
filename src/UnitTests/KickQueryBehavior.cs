using LinqToDB;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Tools;
using Xunit;

namespace UnitTests
{
    public class KickQueryBehavior
    {
        private const string QueryPattern = "select * from table where id in (@id)";

        [Theory]
        [InlineData("foo-id", IdPropertyType.String, DataType.Text)]
        [InlineData("34", IdPropertyType.Int, DataType.Int64)]
        public void ShouldBuildSingleIdQuery(string id, IdPropertyType idPropertyType, DataType expectedDbType)
        {
            //Arrange
            var ids = new [] { id };
                
            //Act
            var query = KickQuery.Build(QueryPattern, ids, idPropertyType);

            //Assert
            Assert.Equal(QueryPattern, query.Query);
            Assert.Single(query.Parameters);
            Assert.Equal("id", query.Parameters[0].Name);
            Assert.Equal(id, query.Parameters[0].Value);
            Assert.Equal(expectedDbType, query.Parameters[0].DataType);
        }

        [Theory]
        [InlineData("foo,bar", "select * from table where id in (@id0,@id1)", IdPropertyType.String, DataType.Text)]
        [InlineData("56,123", "select * from table where id in (@id0,@id1)", IdPropertyType.Int, DataType.Int64)]
        public void ShouldBuildMultipleIdQuery(string idsString, string expectedQuery, IdPropertyType idPropertyType, DataType expectedDataType)
        {
            //Arrange
            var ids = idsString.Split(',');

            //Act
            var query = KickQuery.Build(QueryPattern, ids, idPropertyType);

            //Assert
            Assert.Equal(expectedQuery, query.Query);
            Assert.Equal(ids.Length, query.Parameters.Length);

            for (int i = 0; i < ids.Length; i++)
            {
                Assert.Equal("id" + i, query.Parameters[i].Name);
                Assert.Equal(ids[i], query.Parameters[i].Value);
                Assert.Equal(expectedDataType, query.Parameters[i].DataType);
            }
        }
    }
}

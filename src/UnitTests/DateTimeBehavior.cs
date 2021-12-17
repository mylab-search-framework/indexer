using System;
using MyLab.Search.Indexer.Tools;
using Xunit;

namespace UnitTests
{
    public class UnixDateTimeConverterBehavior
    {
        [Fact]
        public void ShouldConvertDateToUnixDt()
        {
            //Arrange
            var dt = new DateTime(2021, 1, 1, 0, 0, 0);

            //Act
            var unixDt = UnixDateTimeConverter.ToUnixDt(dt);

            //Assert
            Assert.Equal(1609448400000, unixDt);
        }

        [Fact]
        public void ShouldConvertStringDateToUnixDt()
        {
            //Arrange
            var dt = "2021-01-01 00:00:00";

            //Act
            var unixDt = UnixDateTimeConverter.ToUnixDt(dt);

            //Assert
            Assert.Equal(1609448400000, unixDt);
        }
    }
}

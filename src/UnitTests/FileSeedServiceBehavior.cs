using System;
using System.IO;
using System.Threading.Tasks;
using MyLab.Search.Indexer.Tools;
using Xunit;

namespace UnitTests
{
    public class FileSeedServiceBehavior
    {
        [Fact]
        public async Task ShouldProvideInitialIdSeed()
        {
            //Arrange
            var seedSrv = new FileSeedService(Directory.GetCurrentDirectory());
            var indexId = Guid.NewGuid().ToString("N");

            //Act
            var seed = await seedSrv.LoadIdSeedAsync("foo");

            //Assert
            Assert.Equal(-1, seed);
        }

        [Fact]
        public async Task ShouldProvideInitialDateTimeSeed()
        {
            //Arrange
            var seedSrv = new FileSeedService(Directory.GetCurrentDirectory());
            var indexId = Guid.NewGuid().ToString("N");

            //Act
            var seed = await seedSrv.LoadDtSeedAsync(indexId);

            //Assert
            Assert.Equal(DateTime.MinValue, seed);
        }

        [Fact]
        public async Task ShouldSaveAndLoadIdSeed()
        {
            //Arrange
            var seedSrv = new FileSeedService(Directory.GetCurrentDirectory());
            var indexId = Guid.NewGuid().ToString("N");
            int actualSeed = new Random(DateTime.Now.Millisecond).Next();

            //Act
            await seedSrv.SaveSeedAsync(indexId, actualSeed);
            var loadedSeed = await seedSrv.LoadIdSeedAsync(indexId);

            //Assert
            Assert.Equal(actualSeed, loadedSeed);
        }

        [Fact]
        public async Task ShouldSaveAndLoadDateTimeSeed()
        {
            //Arrange
            var seedSrv = new FileSeedService(Directory.GetCurrentDirectory());
            var indexId = Guid.NewGuid().ToString("N");
            var actualSeed = DateTime.Now;

            //Act
            await seedSrv.SaveSeedAsync(indexId, actualSeed);
            var loadedSeed = await seedSrv.LoadDtSeedAsync(indexId);

            //Assert
            Assert.Equal(actualSeed, loadedSeed);
        }
    }
}

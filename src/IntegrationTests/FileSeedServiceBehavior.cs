using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyLab.Search.Indexer;
using MyLab.Search.Indexer.Services;
using Xunit;

namespace IntegrationTests
{
    public class FileSeedServiceBehavior
    {
        [Fact]
        public async Task ShouldCreateDirectoryIfNotExists()
        {
            //Arrange
            if(Directory.Exists("tmp"))
                Directory.Delete("tmp", true);
            
            var options = new IndexerOptions
            {
                SeedPath = "tmp"
            };

            var srv = new FileSeedService(options);

            string expectedFilePath = Path.Combine("tmp", "foo");

            try
            {
                //Act
                await srv.WriteAsync("foo", "bar");

                //Assert
                Assert.True(File.Exists(expectedFilePath));
            }
            finally
            {
                if (Directory.Exists("tmp"))
                    Directory.Delete("tmp", true);
            }
        }
    }
}

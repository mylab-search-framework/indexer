using System;
using System.Collections.Generic;
using LinqToDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLab.Db;
using MyLab.Search.Indexer;
using Xunit;

namespace UnitTests
{
    public class ConfiguredDataProviderSourceBehavior
    {
        [Fact]
        public void ShouldProvideConfiguredProvider()
        {
            //Arrange
            var dbProviderSource = InitDbProviderSource("sqlite");

            //Act
            var dbProvider = dbProviderSource.Provide(string.Empty);

            //Assert
            Assert.NotNull(dbProvider);
            Assert.Equal(ProviderName.SQLite, dbProvider.Name);
        }

        [Fact]
        public void ShouldThrowIfNotSupported()
        {
            //Arrange
            var dbProviderSource = InitDbProviderSource("not-suported");

            //Act & Assert
            Assert.Throws<NotSupportedException>(() => dbProviderSource.Provide(string.Empty));
        }

        private static IDbProviderSource InitDbProviderSource(string providerName)
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("DB", "foo-connection-string"),
                    new KeyValuePair<string, string>("Indexer:DbProvider", providerName),
                })
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddDbTools<ConfiguredDataProviderSource>(config)
                .Configure<IndexerOptions>(config.GetSection("Indexer"))
                .BuildServiceProvider();

            var dbProviderSource = serviceProvider.GetService<IDbProviderSource>();
            return dbProviderSource;
        }
    }
}

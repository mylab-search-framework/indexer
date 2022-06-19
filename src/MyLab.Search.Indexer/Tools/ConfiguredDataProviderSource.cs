using System;
using LinqToDB;
using LinqToDB.DataProvider;
using LinqToDB.DataProvider.MySql;
using LinqToDB.DataProvider.Oracle;
using LinqToDB.DataProvider.SQLite;
using Microsoft.Extensions.Options;
using MyLab.Db;
using MyLab.Log;
using MyLab.Search.Indexer.Options;

namespace MyLab.Search.Indexer.Tools
{
    public class ConfiguredDataProviderSource : IDbProviderSource
    {
        private readonly IndexerDbOptions _options;

        public ConfiguredDataProviderSource(IOptions<IndexerDbOptions> options)
            : this(options.Value)
        {

        }

        public ConfiguredDataProviderSource(IndexerDbOptions options)
        {
            _options = options;
        }

        public IDataProvider Provide(string connectionStringName)
        {
            switch (_options.Provider)
            {
                case "sqlite": return new SQLiteDataProvider(ProviderName.SQLite);
                case "mysql": return new MySqlDataProvider(ProviderName.MySql);
                case "oracle": return new OracleDataProvider(ProviderName.Oracle);
                case "":
                case null:
                    throw new InvalidOperationException("Data Provider is not defined");
                default:
                    throw new InvalidOperationException("Data Provider not supported")
                        .AndFactIs("Provider", _options.Provider);
            }
        }
    }
}

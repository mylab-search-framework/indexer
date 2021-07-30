using System;
using LinqToDB;
using LinqToDB.DataProvider;
using LinqToDB.DataProvider.MySql;
using LinqToDB.DataProvider.Oracle;
using LinqToDB.DataProvider.SQLite;
using Microsoft.Extensions.Options;
using MyLab.Db;
using MyLab.Log;

namespace MyLab.Search.Indexer
{
    class ConfiguredDataProviderSource : IDbProviderSource
    {
        private readonly IndexerOptions _options;

        public ConfiguredDataProviderSource(IOptions<IndexerOptions> options)
            :this(options.Value)
        {
            
        }

        public ConfiguredDataProviderSource(IndexerOptions options)
        {
            _options = options;
        }

        public IDataProvider Provide(string connectionStringName)
        {
            switch (_options.DbProvider)
            {
                case "sqlite": return new SQLiteDataProvider(ProviderName.SQLite);
                case "mysql": return new MySqlDataProvider(ProviderName.MySql);
                case "oracle": return new OracleDataProvider(ProviderName.Oracle);
                default:
                    throw new NotSupportedException("Data provider not supported")
                        .AndFactIs("provider", _options.DbProvider);
            }
        }
    }
}
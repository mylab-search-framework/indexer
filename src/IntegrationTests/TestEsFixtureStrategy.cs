using System;
using Elasticsearch.Net;
using MyLab.Search.EsAdapter.Serialization;
using MyLab.Search.EsTest;
using Nest;

namespace IntegrationTests
{
    public class TestEsFixtureStrategy : EsFixtureStrategy
    {
        public override IConnectionPool ProvideConnection()
        {
            return new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
        }

        public override ConnectionSettings CreateConnectionSettings(IConnectionPool connection)
        {
            return new ConnectionSettings(connection, (builtin, settings) => new NewtonJsonEsSerializer());
        }
    }
}
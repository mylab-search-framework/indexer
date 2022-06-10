using System;
using Elasticsearch.Net;
using MyLab.Search.EsAdapter.Inter;
using MyLab.Search.EsTest;
using Nest;
using Newtonsoft.Json.Linq;

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
            return new ConnectionSettings(connection, (_, _) => new NewtonJsonEsSerializer())
                .DefaultMappingFor(typeof(JObject), m => m.IdProperty("id"));
        }
    }
}
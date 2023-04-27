using System;
using Elasticsearch.Net;
using MyLab.Search.EsAdapter.Inter;
using MyLab.Search.EsTest;
using Nest;

namespace FuncTests
{
    public class TestEsFixtureStrategy : EsFixtureStrategy
    {
        public override IConnectionPool ProvideConnection()
        {
            return new SingleNodeConnectionPool(new Uri(TestTools.EsUrl));
        }

        public override ConnectionSettings CreateConnectionSettings(IConnectionPool connection)
        {
            return new ConnectionSettings(connection);
            //return new ConnectionSettings(connection, new NewtonJsonEsSerializerFactory().Create);
        }
    }
}
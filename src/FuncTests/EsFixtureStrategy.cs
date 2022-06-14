using System;
using Elasticsearch.Net;
using MyLab.Search.EsTest;

namespace FuncTests
{
    public class TestEsFixtureStrategy : EsFixtureStrategy
    {
        public override IConnectionPool ProvideConnection()
        {
            return new SingleNodeConnectionPool(new Uri(TestTools.EsUrl));
        }
    }
}
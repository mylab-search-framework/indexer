using System;
using Elasticsearch.Net;
using MyLab.Elastic.Test;

namespace IntegrationTests
{
    public class TestConnProvider : IConnectionProvider
    {
        public IConnectionPool Provide()
        {
            return new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
        }
    }
}
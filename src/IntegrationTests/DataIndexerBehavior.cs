using MyLab.Elastic.Test;
using MyLab.Search.Indexer.Services;
using Xunit;

namespace IntegrationTests
{
    public class DataIndexerBehavior : IClassFixture<EsIndexFixture<IndexEntity, TestConnProvider>>
    {

    }
}

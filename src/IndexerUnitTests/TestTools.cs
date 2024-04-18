using MyLab.Search.Indexer.Model;
using System.Text.Json.Nodes;

namespace IndexerUnitTests
{
    static class TestTools
    {
        public static IndexingObject CreateEmptyIndexingObject(string id)
        {
            var json = new JsonObject(new Dictionary<string, JsonNode?>
            {
                { "id", id }
            });

            return new IndexingObject(json);
        }
    }
}

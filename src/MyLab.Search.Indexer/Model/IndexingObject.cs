using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MyLab.Search.Indexer.Model;

public class IndexingObject
{
    public LiteralId? Id { get; private set; }
    public JsonObject Value { get; }

    [JsonConstructor]
    public IndexingObject(JsonObject value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));

        Id = value
            .Where(p => p.Key.Equals("id", StringComparison.InvariantCultureIgnoreCase))
            .Select(p => p.Value?.ToString())
            .FirstOrDefault();
    }
}
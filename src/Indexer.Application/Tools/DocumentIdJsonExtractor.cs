using System.Text.Json.Nodes;
using Indexer.Domain.ValueObjects;

namespace Indexer.Application.Tools
{
    static class DocumentIdJsonExtractor
    {
        public static bool TryExtract(JsonNode json, out DocumentId? documentId)
        {
            documentId = null;

            var idNode = json["id"] ?? json["Id"] ?? json["ID"];

            if (idNode == null || !DocumentId.TryCreate(idNode.ToString(), out documentId))
                return false;

            return true;
        }
    }
}

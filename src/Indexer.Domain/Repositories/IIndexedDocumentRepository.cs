using System.Text.Json.Nodes;
using Indexer.Domain.ValueObjects;

namespace Indexer.Domain.Repositories
{
    public interface IIndexedDocumentRepository
    {
        Task PutDocumentAsync(string idxName, DocumentId docId, JsonNode docJson, CancellationToken cancellationToken);
        Task PatchDocumentAsync(string idxName, DocumentId docId, JsonNode docJson, CancellationToken cancellationToken);
        Task DeleteDocumentAsync(string idxName, DocumentId docId, CancellationToken cancellationToken);
    }
}

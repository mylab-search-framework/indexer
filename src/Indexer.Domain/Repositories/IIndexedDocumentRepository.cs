using System.Text.Json.Nodes;
using Indexer.Domain.ValueObjects;

namespace Indexer.Domain.Repositories
{
    public interface IIndexedDocumentRepository
    {
        Task PutDocumentAsync(IndexId idxId, DocumentId docId, JsonNode docJson, CancellationToken cancellationToken);
        Task PatchDocumentAsync(IndexId idxId, DocumentId docId, JsonNode docJson, CancellationToken cancellationToken);
        Task DeleteDocumentAsync(IndexId idxId, DocumentId docId, CancellationToken cancellationToken);
    }
}

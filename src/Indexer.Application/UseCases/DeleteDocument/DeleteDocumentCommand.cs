using System.Text.Json.Nodes;
using MediatR;

namespace Indexer.Application.UseCases.DeleteDocument
{
    public record DeleteDocumentCommand(string IndexId, string DocumentId) : IRequest;
}

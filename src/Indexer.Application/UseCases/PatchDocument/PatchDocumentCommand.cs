using System.Text.Json.Nodes;
using MediatR;

namespace Indexer.Application.UseCases.PatchDocument
{
    public record PatchDocumentCommand(string IndexId, JsonNode Document) : IRequest;
}

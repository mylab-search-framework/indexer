using System.Text.Json.Nodes;
using MediatR;

namespace MyLab.Search.Indexer.Application.UseCases.PutDocument
{
    public record PutDocumentCommand(string IndexId, string DocumentId, JsonObject Document) : IRequest;
}

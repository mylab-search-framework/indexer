using System.Text.Json.Nodes;
using FluentValidation;
using MediatR;

namespace Indexer.Application.UseCases.PutDocument
{
    public record PutDocumentCommand(string IndexId, JsonNode Document) : IRequest;
}

using System.ComponentModel.DataAnnotations;
using Indexer.Application.Tools;
using Indexer.Domain.Repositories;
using Indexer.Domain.ValueObjects;
using MediatR;

namespace Indexer.Application.UseCases.PutDocument;

class PutDocumentHandler(IIndexedDocumentRepository repository) : IRequestHandler<PutDocumentCommand>
{
    public Task Handle(PutDocumentCommand cmd, CancellationToken cancellationToken)
    {
        cmd.ValidateAndThrow();

        if (!DocumentIdJsonExtractor.TryExtract(cmd.Document, out var documentId))
            throw new ValidationException("Document identifier not found");

        if (!IndexId.TryCreate(cmd.IndexId, out var indexId))
            throw new ValidationException("Invalid index id");

        return repository.PutDocumentAsync(indexId!, documentId!, cmd.Document, cancellationToken);
    }
}
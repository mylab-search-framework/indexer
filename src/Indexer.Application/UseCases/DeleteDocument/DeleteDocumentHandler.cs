using System.ComponentModel.DataAnnotations;
using Indexer.Domain.Repositories;
using Indexer.Domain.ValueObjects;
using MediatR;

namespace Indexer.Application.UseCases.DeleteDocument;

class DeleteDocumentHandler(IIndexedDocumentRepository repository) : IRequestHandler<DeleteDocumentCommand>
{
    public Task Handle(DeleteDocumentCommand cmd, CancellationToken cancellationToken)
    {
        cmd.ValidateAndThrow();

        if (!DocumentId.TryCreate(cmd.DocumentId, out var documentId))
            throw new ValidationException("Invalid document id");

        if (!IndexId.TryCreate(cmd.IndexId, out var indexId))
            throw new ValidationException("Invalid index id");

        return repository.DeleteDocumentAsync(indexId!, documentId!, cancellationToken);
    }
}
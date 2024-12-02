using System.ComponentModel.DataAnnotations;
using Indexer.Application.Services;
using Indexer.Domain.Repositories;
using Indexer.Domain.ValueObjects;
using MediatR;

namespace Indexer.Application.UseCases.DeleteDocument;

class DeleteDocumentHandler(IIndexedDocumentRepository repository, IEsIndexNameProvider indexNameProvider) : IRequestHandler<DeleteDocumentCommand>
{
    public Task Handle(DeleteDocumentCommand cmd, CancellationToken cancellationToken)
    {
        cmd.ValidateAndThrow();

        if (!DocumentId.TryParse(cmd.DocumentId, out var documentId))
            throw new ValidationException("Invalid document id");

        if (!IndexId.TryParse(cmd.IndexId, out var indexId))
            throw new ValidationException("Invalid index id");

        var indexName = indexNameProvider.Provide(indexId!);

        return repository.DeleteDocumentAsync(indexName, documentId!, cancellationToken);
    }
}
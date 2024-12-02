using System.ComponentModel.DataAnnotations;
using Indexer.Application.Services;
using Indexer.Application.Tools;
using Indexer.Domain.Repositories;
using Indexer.Domain.ValueObjects;
using MediatR;

namespace Indexer.Application.UseCases.PutDocument;

class PutDocumentHandler(IIndexedDocumentRepository repository, IEsIndexNameProvider indexNameProvider) : IRequestHandler<PutDocumentCommand>
{
    public Task Handle(PutDocumentCommand cmd, CancellationToken cancellationToken)
    {
        cmd.ValidateAndThrow();

        if (!DocumentIdJsonExtractor.TryExtract(cmd.Document, out var documentId))
            throw new ValidationException("DocumentPart identifier not found");

        if (!IndexId.TryParse(cmd.IndexId, out var indexId))
            throw new ValidationException("Invalid index id");

        var indexName = indexNameProvider.Provide(indexId!);

        return repository.PutDocumentAsync(indexName, documentId!, cmd.Document, cancellationToken);
    }
}
using MediatR;

namespace MyLab.Search.Indexer.Application.UseCases.PutDocument;

class PutDocumentHandler : IRequestHandler<PutDocumentCommand>
{
    public Task Handle(PutDocumentCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
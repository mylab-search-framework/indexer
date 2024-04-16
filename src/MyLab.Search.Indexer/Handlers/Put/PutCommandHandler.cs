using MediatR;

namespace MyLab.Search.Indexer.Handlers.Put
{
    class PutCommandHandler : IRequestHandler<PutchCommand>
    {
        public Task Handle(PutchCommand request, CancellationToken cancellationToken)
        {
            
        }
    }
}

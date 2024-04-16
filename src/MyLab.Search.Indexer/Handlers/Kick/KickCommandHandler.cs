using MediatR;

namespace MyLab.Search.Indexer.Handlers.Kick
{
    class KickCommandHandler : IRequestHandler<KickCommand>
    {
        public Task Handle(KickCommand request, CancellationToken cancellationToken)
        {
            
        }
    }
}

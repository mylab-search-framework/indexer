using MediatR;

namespace MyLab.Search.Indexer.Handlers.Patch
{
    class PatchCommandHandler : IRequestHandler<PatchCommand>
    {
        public Task Handle(PatchCommand request, CancellationToken cancellationToken)
        {
            
        }
    }
}

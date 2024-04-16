using MediatR;

namespace MyLab.Search.Indexer.Handlers.Delete
{
    class DeleteCommandHandler : IRequestHandler<DeleteCommand>
    {
        public Task Handle(DeleteCommand request, CancellationToken cancellationToken)
        {
            
        }
    }
}

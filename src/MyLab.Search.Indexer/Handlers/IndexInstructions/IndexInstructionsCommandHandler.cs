using MediatR;

namespace MyLab.Search.Indexer.Handlers.IndexInstructions
{
    class IndexInstructionsCommandHandler : IRequestHandler<IndexInstructionsCommand>
    {
        public Task Handle(IndexInstructionsCommand request, CancellationToken cancellationToken)
        {
            
        }
    }
}

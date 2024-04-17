using MediatR;
using MyLab.Search.Indexer.Model;

namespace MyLab.Search.Indexer.Handlers.Kick
{
    class KickCommand : IRequest
    {
        public required LiteralId IndexId { get; init; }
        public required LiteralId DocumentId { get; init; }
    }
}

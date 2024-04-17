using MediatR;
using MyLab.Search.Indexer.Model;

namespace MyLab.Search.Indexer.Handlers.Put
{
    class PutCommand : IRequest
    {
        public required LiteralId IndexId { get; init; }
        public required IndexingObject Document { get; init; }
    }
}

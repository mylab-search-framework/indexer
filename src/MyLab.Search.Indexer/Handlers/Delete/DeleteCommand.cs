using System.Collections.ObjectModel;
using MediatR;
using MyLab.Search.Indexer.Model;

namespace MyLab.Search.Indexer.Handlers.Delete
{
    class DeleteCommand : IRequest
    {
        public required LiteralId IndexId { get; init; }
        public required LiteralId DocumentId { get; init; }
    }
}

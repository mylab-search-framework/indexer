using System.Collections.ObjectModel;
using MediatR;
using MyLab.Search.Indexer.Model;

namespace MyLab.Search.Indexer.Handlers.Patch
{
    class PatchCommand : IRequest
    {
        public required LiteralId IndexId { get; init; }
        public required IndexingObject Document { get; init; }
    }
}

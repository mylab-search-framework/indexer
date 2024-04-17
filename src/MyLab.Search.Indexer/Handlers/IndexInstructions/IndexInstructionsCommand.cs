using MediatR;
using MyLab.Search.Indexer.Model;

namespace MyLab.Search.Indexer.Handlers.IndexInstructions
{
    class IndexInstructionsCommand : IRequest
    {
        public required LiteralId IndexId { get; set; }
        public IndexingObject[]? PutList { get; set; }
        public IndexingObject[]? PatchList { get; set; }
        public LiteralId[]? DeleteList { get; set; }
    }
}

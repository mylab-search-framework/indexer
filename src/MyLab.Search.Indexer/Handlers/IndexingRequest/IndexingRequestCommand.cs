using MediatR;
using MyLab.Search.Indexer.Model;

namespace MyLab.Search.Indexer.Handlers.IndexingRequest
{
    class IndexingRequestCommand : IRequest
    {
        public required LiteralId IndexId { get; set; }
        public IndexingObject[]? PutList { get; set; }
        public IndexingObject[]? PatchList { get; set; }
        public LiteralId[]? DeleteList { get; set; }
        public LiteralId[]? KickList { get; set; }
    }
}

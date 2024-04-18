using AutoMapper;
using MediatR;
using MyLab.Log.Dsl;
using MyLab.Search.Indexer.Model;
using MyLab.Search.Indexer.Services;

namespace MyLab.Search.Indexer.Handlers.IndexingRequest
{
    class IndexingRequestCommandHandler
    (
        IDocumentRepository docRepo,
        IIndexingProcessor indexingProcessor,
        IMapper mapper, 
        ILogger<IndexingRequestCommandHandler>? log = null
    ) : IRequestHandler<IndexingRequestCommand>
    {
        private readonly IDslLogger? _log = log?.Dsl();

        public async Task Handle(IndexingRequestCommand request, CancellationToken cancellationToken)
        {
            var indexInstructions = mapper.Map<IndexingRequestCommand, IndexInstructions>(request);
                
            if (request.KickList is { Length: > 0 })
            {
                var kickDocs = await docRepo.GetDocumentsAsync(request.KickList);

                CheckForFullFound(kickDocs, request.KickList);

                var newPutList = new List<IndexingObject>();

                if (request.PutList != null)
                    newPutList.AddRange(request.PutList);
                newPutList.AddRange(kickDocs);

                indexInstructions.PutList = newPutList;
            }

            await indexingProcessor.ProcessAsync(indexInstructions);
        }

        private void CheckForFullFound(IReadOnlyList<IndexingObject> foundDocs, LiteralId[] requestKickList)
        {
            var notFound = requestKickList
                .Where(kickId => foundDocs.All(foundDoc => kickId != foundDoc.Id))
                .Select(notFoundDoc => notFoundDoc.ToString())
                .Where(id => id != null)
                .Cast<string>()
                .ToArray();

            if (notFound is { Length: > 0 } && _log != null)
            {
                var foundDocIds = requestKickList
                    .Where(id => notFound.All(nfId => nfId != id))
                    .Select(id => id.ToString())
                    .Where(id => id != null)
                    .Cast<string>()
                    .ToArray();
                
                _log?.Warning("Some kick-documents not found")
                    .AndFactIs("found", string.Join(", ", foundDocIds))
                    .AndFactIs("not-found", string.Join(", ", notFound))
                    .Write();
            }
        }
    }
}

using System.Collections.ObjectModel;
using MediatR;
using MyLab.Search.Indexer.Model;

namespace MyLab.Search.Indexer.Handlers.Patch
{
    class PatchCommand : IRequest
    {
        public IReadOnlyList<IndexingObject> PatchList { get; }

        public PatchCommand(IndexingObject[] patchList)
        {
            PatchList = new ReadOnlyCollection<IndexingObject>(patchList);
        }
    }
}

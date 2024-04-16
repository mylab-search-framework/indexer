using System.Collections.ObjectModel;
using MediatR;
using MyLab.Search.Indexer.Model;

namespace MyLab.Search.Indexer.Handlers.Put
{
    class PutchCommand : IRequest
    {
        public IReadOnlyList<IndexingObject> PutList { get; }

        public PutchCommand(IndexingObject[] putList)
        {
            PutList = new ReadOnlyCollection<IndexingObject>(putList);
        }
    }
}

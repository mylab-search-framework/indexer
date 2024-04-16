using System.Collections.ObjectModel;
using MediatR;
using MyLab.Search.Indexer.Model;

namespace MyLab.Search.Indexer.Handlers.Delete
{
    class DeleteCommand : IRequest
    {
        public IReadOnlyList<LiteralId> PutList { get; }

        public DeleteCommand(LiteralId[] putList)
        {
            PutList = new ReadOnlyCollection<LiteralId>(putList);
        }
    }
}

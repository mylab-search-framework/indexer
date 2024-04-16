using System.Collections.ObjectModel;
using MediatR;
using MyLab.Search.Indexer.Model;

namespace MyLab.Search.Indexer.Handlers.Kick
{
    class KickCommand : IRequest
    {
        public IReadOnlyList<LiteralId> PutList { get; }

        public KickCommand(LiteralId[] putList)
        {
            PutList = new ReadOnlyCollection<LiteralId>(putList);
        }
    }
}

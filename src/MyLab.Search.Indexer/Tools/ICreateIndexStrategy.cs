using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MyLab.Search.EsAdapter;

namespace MyLab.Search.Indexer.Tools
{
    interface ICreateIndexStrategy
    {
        Task CreateIndexAsync(IEsManager esMgr, string name, CancellationToken cancellationToken);
    }
}

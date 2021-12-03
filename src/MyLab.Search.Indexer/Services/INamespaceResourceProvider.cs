using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Services
{
    public interface INamespaceResourceProvider
    {
        Task<string> ReadFileAsync(string nsId, string filename);
    }
}

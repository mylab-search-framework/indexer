using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Services
{
    public interface IJobResourceProvider
    {
        Task<string> ReadFileAsync(string jobId, string filename);
    }
}

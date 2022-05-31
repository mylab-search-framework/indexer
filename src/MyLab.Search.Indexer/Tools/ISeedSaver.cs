using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Tools
{
    public interface ISeedSaver
    {
        Task SaveAsync();
    }
}

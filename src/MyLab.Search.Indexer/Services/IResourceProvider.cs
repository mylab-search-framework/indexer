using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Services
{
    public interface IResourceProvider
    {
        Task<string> ProvideKickQueryAsync(string indexId);
        Task<string> ProvideSyncQueryAsync(string indexId);
        Task<string> ProvideIndexMappingAsync(string indexId);

        IResource[] ProvideLifecyclePolicies();
        IResource[] ProvideIndexTemplates();
        IResource[] ProvideComponentTemplates();
    }

    public interface IResource
    {
        string Name { get; }

        Stream OpenRead();
    }

    class FileResource : IResource
    {
        private readonly FileInfo _file;
        public string Name { get; }
        public Stream OpenRead()
        {
            return _file.OpenRead();
        }
        
        public FileResource(FileInfo file)
        {
            Name = Path.GetFileNameWithoutExtension(file.Name);
            _file = file;
        }

    }
}

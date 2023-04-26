using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Services
{
    public interface IResourceProvider
    {
        Task<string> ProvideKickQueryAsync(string indexId);
        Task<string> ProvideSyncQueryAsync(string indexId);
        Task<string> ProvideIndexSettingsAsync(string indexId);

        IResource[] ProvideLifecyclePoliciesAsync();
        IResource[] ProvideIndexTemplatesAsync();
        IResource[] ProvideComponentTemplatesAsync();
    }

    public interface IResource
    {
        public string Name { get; }
        public Stream OpenRead();
        public Task<string> ReadAllTextAsync();
    }

    public class FileResource : IResource
    {
        private readonly FileInfo _fileInfo;
        public string Name { get; }
        public Stream OpenRead()
        {
            return _fileInfo.OpenRead();
        }

        public Task<string> ReadAllTextAsync()
        {
            return _fileInfo.OpenText().ReadToEndAsync();
        }

        public FileResource(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
            Name = Path.GetFileNameWithoutExtension(fileInfo.Name);
        }
    }

    public class StringResource : IResource
    {
        private readonly string _contentStr;
        private byte[] _contentBin;
        public string Name { get; }
        public Stream OpenRead()
        {
            _contentBin ??= Encoding.UTF8.GetBytes(_contentStr);

            return new MemoryStream(_contentBin);
        }

        public Task<string> ReadAllTextAsync()
        {
            return Task.FromResult(_contentStr);
        }

        public StringResource(string name, string content)
        {
            Name = name;
            _contentStr = content;
        }
    }
}

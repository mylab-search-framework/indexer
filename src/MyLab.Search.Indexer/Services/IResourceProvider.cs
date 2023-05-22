using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MyLab.Search.Indexer.Tools;
using Nest;

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

        IndexResourceDirectory IndexDirectory { get; }
        IReadOnlyDictionary<string, IRes<LifecyclePolicyResource>> Lifecycles { get; }
        IReadOnlyDictionary<string, IRes<IndexTemplateResource>> IndexTemplates { get; }
        IReadOnlyDictionary<string, IRes<ComponentTemplateResource>> ComponentTemplates { get; }
    }


    public interface IRes<T>
    {
        string Name { get; }
        string Hash { get; }
        T Content { get; }
    }

    public class IndexResources 
    {
        public string IndexId { get; init; }
        public SqlResource KickQuery { get; init; }
        public SqlResource  SyncQuery { get; init; }
        public MappingResource Mapping { get; init; }
    }

    public class IndexResourceDirectory
    {
        public IReadOnlyDictionary<string, IndexResources> Named { get; init; }
        public MappingResource DefaultMapping { get; init; }

    }

    public abstract class ResBase
    {
        public string Name { get; init;  }
        public string Hash { get; init; }
    }

    public class SqlResource : ResBase, IRes<string>
    {
        public string Content { get; init; }
    }

    public class MappingResource : ResBase, IRes<TypeMapping>
    {
        public TypeMapping Content { get; init; }
    }

    public class LifecyclePolicyResource : ResBase,  IRes<LifecyclePolicy>
    {
        public LifecyclePolicy Content { get; init; }
    }

    public class IndexTemplateResource : ResBase, IRes<IndexTemplate>
    {
        public IndexTemplate Content { get; init; }
    }

    public class ComponentTemplateResource : ResBase, IRes<ComponentTemplate>
    {
        public ComponentTemplate Content { get; init; }
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

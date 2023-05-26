using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Nest;

namespace MyLab.Search.Indexer.Services
{
    public interface IResourceProvider
    {
        IndexResourceDirectory IndexDirectory { get; }
        NamedResources<LifecyclePolicy> LifecyclePolicies { get; }
        NamedResources<IndexTemplate> IndexTemplates { get; }
        NamedResources<ComponentTemplate> ComponentTemplates { get; }
    }

    public class NamedResources<T> : ReadOnlyDictionary<string, IResource<T>> where T : class, new()
    {
        public NamedResources(IDictionary<string, IResource<T>> dictionary) : base(dictionary)
        {
        }

        public NamedResources(IDictionary<string, Resource<T>> dictionary) 
            : base(dictionary
                .Select(kv => new KeyValuePair<string,IResource<T>>(kv.Key, kv.Value))
                .ToDictionary(kv => kv.Key, kv => kv.Value))
        {
        }

        public NamedResources(string name, T content, string hash = null)            
            : base(new Dictionary<string, IResource<T>> 
            {
                { name,  new Resource<T>(){ Content = content, Name = name, Hash = hash}}
            })
        {
            
        }
    }

    public static class ResourceProviderExtensions
    {
        public static IResource<string> ProvideKickQuery(this IResourceProvider resourceProvider, string indexId)
        {
            var indexResources = ProvideIndexResources(resourceProvider, indexId);

            return indexResources?.KickQuery;
        }

        public static IResource<string> ProvideSyncQuery(this IResourceProvider resourceProvider, string indexId)
        {
            var indexResources = ProvideIndexResources(resourceProvider, indexId);

            return indexResources?.SyncQuery;
        }

        public static IResource<TypeMapping> ProvideIndexMapping(this IResourceProvider resourceProvider, string indexId)
        {
            var indexResources = ProvideIndexResources(resourceProvider, indexId);

            return indexResources?.Mapping;
        }

        static IndexResources ProvideIndexResources(IResourceProvider resourceProvider, string indexId)
        {
            if (resourceProvider == null) throw new ArgumentNullException(nameof(resourceProvider));
            if (indexId == null) throw new ArgumentNullException(nameof(indexId));

            if (resourceProvider.IndexDirectory.Named == null)
                return null;

            if (!resourceProvider.IndexDirectory.Named.TryGetValue(indexId, out var indexResources))
                return null;

            return indexResources;
        }
    }


    public interface IResource<T>
    {
        string Name { get; }
        string Hash { get; }
        T Content { get; }
    }

    public class IndexResources 
    {
        public string IndexId { get; init; }
        public IResource<string> KickQuery { get; init; }
        public IResource<string> SyncQuery { get; init; }
        public IResource<TypeMapping> Mapping { get; init; }
    }

    public class IndexResourceDirectory
    {
        public IReadOnlyDictionary<string, IndexResources> Named { get; init; }
        public IResource<TypeMapping> CommonMapping { get; init; }

    }

    public class Resource<T> : IResource<T>
    {
        public string Name { get; set;  }
        public string Hash { get; set; }
        public T Content { get; set; }
    }
    
}

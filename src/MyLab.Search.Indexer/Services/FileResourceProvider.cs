using System;
using System.Collections.Generic;
using MyLab.Log;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyLab.Search.Indexer.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Hosting;
using MyLab.Search.EsAdapter.Tools;
using MyLab.Search.Indexer.Tools;
using MySql.Data.MySqlClient;
using Nest;

namespace MyLab.Search.Indexer.Services
{
    class FileResourceProvider : IResourceProvider
    {
        private readonly IEsTools _esTools;
        private readonly IndexerOptions _opts;

        private const string KickFilename = "kick.sql";
        private const string SyncFilename = "sync.sql";
        private const string MappingFilename = "mapping.json";
        private const string IndexTemplatesDirname = "index-templates";
        private const string ComponentTemplatesDirname = "component-templates";
        private const string LifecyclesDirname = "lifecycle-policies";
        private const string IndexesDirname = "indexes";

        public IndexResourceDirectory IndexDirectory { get; private set; }
        public NamedResources<LifecyclePolicy> LifecyclePolicies { get; private set; }
        public NamedResources<IndexTemplate> IndexTemplates { get; private set; }
        public NamedResources<ComponentTemplate> ComponentTemplates { get; private set; }

        public FileResourceProvider(
            IEsTools esTools,
            IOptions<IndexerOptions> opts)
            : this(esTools, opts.Value)
        {
            
        }
        
        public FileResourceProvider(
            IEsTools esTools,
            IndexerOptions opts)
        {
            _esTools = esTools;
            _opts = opts;
        }

        public async Task LoadAsync(CancellationToken cancellationToken)
        {
            await LoadIndexesDirectoryAsync(cancellationToken);

            var lifecyclesData = await LoadComponentsAsync<LifecyclePolicy>(LifecyclesDirname, cancellationToken);
            if (lifecyclesData != null)
            {
                LifecyclePolicies = new NamedResources<LifecyclePolicy>(
                    lifecyclesData.ToDictionary(
                        l => l.Name, 
                        l => new Resource<LifecyclePolicy>()
                        {
                            Name = l.Name,
                            Hash = l.Hash,
                            Content = l.Content
                        }
                    )
                );
            }
            else
            {
                LifecyclePolicies = new NamedResources<LifecyclePolicy>(
                        new Dictionary<string, IResource<LifecyclePolicy>>()
                    );
            }

            var indexTemplatesData = await LoadComponentsAsync<IndexTemplate>(IndexTemplatesDirname, cancellationToken);
            if (indexTemplatesData != null)
            {
                IndexTemplates = new NamedResources<IndexTemplate>(
                    indexTemplatesData.ToDictionary(
                        l => l.Name, 
                        l => new Resource<IndexTemplate>()
                        {
                            Name = l.Name,
                            Hash = l.Hash,
                            Content = l.Content
                        }
                    )
                );
            }
            else
            {
                IndexTemplates = new NamedResources<IndexTemplate>(
                    new Dictionary<string, IResource<IndexTemplate>>()
                );
            }

            var componentTemplatesData = await LoadComponentsAsync<ComponentTemplate>(ComponentTemplatesDirname, cancellationToken);
            if (componentTemplatesData != null)
            {
                ComponentTemplates = new NamedResources<ComponentTemplate>(
                    componentTemplatesData.ToDictionary(
                        l => l.Name, 
                        l => new Resource<ComponentTemplate>()
                        {
                            Name = l.Name,
                            Hash = l.Hash,
                            Content = l.Content
                        }
                    )
                );
            }
            else
            {
                ComponentTemplates = new NamedResources<ComponentTemplate>(
                    new Dictionary<string, IResource<ComponentTemplate>>()
                );
            }
        }

        async Task<IEnumerable<(string Name, string Hash, T Content)>> LoadComponentsAsync<T>(string subDirName, CancellationToken cancellationToken)
        {
            var subDirPath = Path.Combine(_opts.ResourcesPath, subDirName);
            var subDir = new DirectoryInfo(subDirPath);

            if (!subDir.Exists)
                return null;

            var res = new List<(string Name, string Hash, T Content)>();
            
            foreach (var file in subDir.GetFiles("*.json"))
            {
                var componentData = await LoadResourceAsync<T>(file, cancellationToken);
                res.Add((componentData.Name, componentData.Hash, componentData.Content));
            }

            return res;
        }

        private async Task LoadIndexesDirectoryAsync(CancellationToken cancellationToken)
        {
            var indexResourcePath = Path.Combine(_opts.ResourcesPath, IndexesDirname);
            var resDir = new DirectoryInfo(indexResourcePath);

            if (!resDir.Exists)
                throw new InvalidOperationException("Index directory not found")
                    .AndFactIs("path", indexResourcePath);

            Resource<TypeMapping> commonMapping = null;
            byte[] commonMappingBin = null;

            var commonMappingFile = new FileInfo(Path.Combine(indexResourcePath, MappingFilename));
            if (commonMappingFile.Exists)
            {
                var commonMappingData = await LoadResourceAsync<TypeMapping>(commonMappingFile, cancellationToken);
                commonMapping = new Resource<TypeMapping>()
                {
                    Name = commonMappingData.Name,
                    Content = commonMappingData.Content,
                    Hash = commonMappingData.Hash
                };
                commonMappingBin = commonMappingData.Bin;
            }

            var namedDirs = new Dictionary<string, IndexResources>();

            foreach (var directoryInfo in resDir.GetDirectories())
            {
                Resource<TypeMapping> mappingResource = null;
                var mappingFile = new FileInfo(Path.Combine(directoryInfo.FullName, MappingFilename));
                if (mappingFile.Exists)
                {
                    var mappingResourceData = await LoadAndMergeMappingAsync(mappingFile, commonMappingBin, cancellationToken);

                    mappingResource = new Resource<TypeMapping>
                    {
                        Name = mappingResourceData.Name,
                        Content = mappingResourceData.Content,
                        Hash = mappingResourceData.Hash
                    };
                }

                Resource<string> kickQueryResource = null;
                var kickQueryFile = new FileInfo(Path.Combine(directoryInfo.FullName, KickFilename));
                if (kickQueryFile.Exists)
                {
                    var kickQueryResourceData = await LoadStringResourceAsync(kickQueryFile, cancellationToken);

                    kickQueryResource = new Resource<string>
                    {
                        Name = kickQueryResourceData.Name,
                        Content = kickQueryResourceData.Content,
                        Hash = kickQueryResourceData.Hash
                    };
                }

                Resource<string> syncQueryResource = null;
                var syncQueryFile = new FileInfo(Path.Combine(directoryInfo.FullName, SyncFilename));
                if (syncQueryFile.Exists)
                {
                    var syncQueryResourceData = await LoadStringResourceAsync(syncQueryFile, cancellationToken);

                    syncQueryResource = new Resource<string>
                    {
                        Name = syncQueryResourceData.Name,
                        Content = syncQueryResourceData.Content,
                        Hash = syncQueryResourceData.Hash
                    };
                }

                namedDirs.Add(directoryInfo.Name, new IndexResources
                {
                    IndexId = directoryInfo.Name,
                    Mapping = mappingResource,
                    SyncQuery = syncQueryResource,
                    KickQuery = kickQueryResource
                });
            }

            IndexDirectory = new IndexResourceDirectory
            {
                CommonMapping = commonMapping,
                Named = namedDirs
            };
        }

        async Task<(string Name, string Hash, byte[] Bin, string Content)> LoadStringResourceAsync(FileInfo file, CancellationToken cancellationToken)
        {
            var buff = new byte[file.Length];
            await using var readStream = file.OpenRead();
            // ReSharper disable once MustUseReturnValue
            await readStream.ReadAsync(buff, cancellationToken);

            var content = Encoding.UTF8.GetString(buff);

            await using var stream = new MemoryStream(buff);
            var hash = HashCalculator.Calculate(buff);

            return (Path.GetFileNameWithoutExtension(file.Name), hash, buff, content);
        }

        async Task<(string Name, string Hash, byte[] Bin, T Content)> LoadResourceAsync<T>(FileInfo file, CancellationToken cancellationToken)
        {
            var buff = new byte[file.Length];
            await using var readStream = file.OpenRead();
            // ReSharper disable once MustUseReturnValue
            await readStream.ReadAsync(buff, cancellationToken);

            await using var stream = new MemoryStream(buff);
                var content = _esTools.Serializer.Deserialize<T>(stream);
                
            var hash = HashCalculator.Calculate(buff);

            return (Path.GetFileNameWithoutExtension(file.Name), hash, buff, content);
        }

        async Task<(string Name, string Hash, TypeMapping Content)> LoadAndMergeMappingAsync(FileInfo file, byte[] commonPart, CancellationToken cancellationToken)
        {
            var buff = new byte[file.Length];
            await using var readStream = file.OpenRead();
            // ReSharper disable once MustUseReturnValue
            await readStream.ReadAsync(buff, cancellationToken);

            byte[] resultMappingBin;

            if (commonPart != null)
            {

                var commonMappingJson = Encoding.UTF8.GetString(commonPart);
                var mappingJson = Encoding.UTF8.GetString(buff);

                var mappingJObj = JObject.Parse(TrimContent(mappingJson));
                var resultMappingJson = JObject.Parse(TrimContent(commonMappingJson));
                resultMappingJson.Merge(mappingJObj);

                resultMappingBin = Encoding.UTF8.GetBytes(resultMappingJson.ToString(Formatting.None));
            }
            else
            {
                resultMappingBin = buff;
            }

            await using var stream = new MemoryStream(resultMappingBin);
            var content = _esTools.Serializer.Deserialize<TypeMapping>(stream);

            var hash = HashCalculator.Calculate(buff);

            return (Path.GetFileNameWithoutExtension(file.Name), hash, content);
        }

        string TrimContent(string content) => content.Trim('\uFEFF', '\u200B');
    }
}

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
using Nest;

namespace MyLab.Search.Indexer.Services
{
    class FileResourceProvider : IResourceProvider, IHostedService
    {
        private readonly IEsTools _esTools;
        private readonly IndexerOptions _opts;
        private readonly string _indexResourcePath;
        private readonly string _lifecyclePoliciesPath;
        private readonly string _indexTemplatesPath;
        private readonly string _componentTemplatesPath;

        private const string KickFilename = "kick.sql";
        private const string SyncFilename = "sync.sql";
        private const string MappingFilename = "mapping.json";

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

        public async Task<string> ProvideKickQueryAsync(string indexId)
        {
            var filePath = Path.Combine(_indexResourcePath, indexId, KickFilename);

            return await ReadFileAsync(filePath);
        }

        public async Task<string> ProvideSyncQueryAsync(string indexId)
        {
            var filePath = Path.Combine(_indexResourcePath, indexId, SyncFilename);

            return await ReadFileAsync(filePath);
        }

        public async Task<string> ProvideIndexMappingAsync(string indexId)
        {
            var mappingJsonPath = Path.Combine(_indexResourcePath, indexId, MappingFilename);
            var mappingJson = await ReadFileAsync(mappingJsonPath, throwIfDoesNotExists: false);

            var commonMappingJsonPath = Path.Combine(_indexResourcePath, MappingFilename);
            var commonMappingJson = await ReadFileAsync(commonMappingJsonPath, throwIfDoesNotExists: false);

            if (mappingJson == null && commonMappingJson == null)
                throw new FileNotFoundException("Mapping not found")
                    .AndFactIs("index-id", indexId)
                    .AndFactIs("mapping-file", mappingJsonPath)
                    .AndFactIs("common-file", commonMappingJsonPath);

            if (commonMappingJson == null)
                return mappingJson;

            if (mappingJson == null)
                return commonMappingJson;

            var mappingJObj = JObject.Parse(mappingJson);
            var resultMappingJson = JObject.Parse(commonMappingJson);

            resultMappingJson.Merge(mappingJObj);

            return resultMappingJson.ToString(Formatting.None);
        }

        public IResource[] ProvideLifecyclePolicies()
        {
            return ProvideJsonResources(_lifecyclePoliciesPath);
        }

        public IResource[] ProvideIndexTemplates()
        {
            return ProvideJsonResources(_indexTemplatesPath);
        }

        public IResource[] ProvideComponentTemplates()
        {
            return ProvideJsonResources(_componentTemplatesPath);
        }

        public IndexResourceDirectory IndexDirectory { get; private set; }
        public IReadOnlyDictionary<string, LifecyclePolicyResource> Lifecycles { get; private set; }
        public IReadOnlyDictionary<string, IndexTemplateResource> IndexTemplates { get; private set; }
        public IReadOnlyDictionary<string, ComponentTemplateResource> ComponentTemplates { get; private set; }

        async Task<string> ReadFileAsync(string path, bool throwIfDoesNotExists = true)
        {
            if (!File.Exists(path))
            {
                if (!throwIfDoesNotExists)
                    return null;

                throw new FileNotFoundException("Resource file not found")
                    .AndFactIs("full-path", path);
            }

            return await File.ReadAllTextAsync(path);
        }
        
        IResource[] ProvideJsonResources(string dirPath)
        {
            var resDir = new DirectoryInfo(dirPath);

            if (!resDir.Exists)
                return Array.Empty<IResource>();

            var fileList = resDir
                .EnumerateFiles("*.json")
                .ToArray();

            if (fileList.Length == 0)
                return Array.Empty<IResource>();

            var res = new List<IResource>();

            foreach (var file in fileList)
            {
                using var stream = file.OpenText();

                res.Add(new FileResource(file));
            }

            return res.ToArray();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await LoadIndexesDirectoryAsync(cancellationToken);

            var lifecyclesData = await LoadComponentsAsync<LifecyclePolicy>("lifecycle-policies", cancellationToken);
            if (lifecyclesData != null)
            {
                Lifecycles = lifecyclesData.ToDictionary(l => l.Name, l => new LifecyclePolicyResource
                {
                    Name = l.Name,
                    Hash = l.Hash,
                    Content = l.Content
                });
            }

            var indexTemplatesData = await LoadComponentsAsync<IndexTemplate>("index-templates", cancellationToken);
            if (indexTemplatesData != null)
            {
                IndexTemplates = indexTemplatesData.ToDictionary(l => l.Name, l => new IndexTemplateResource
                {
                    Name = l.Name,
                    Hash = l.Hash,
                    Content = l.Content
                });
            }

            var componentTemplatesData = await LoadComponentsAsync<ComponentTemplate>("component-templates", cancellationToken);
            if (componentTemplatesData != null)
            {
                ComponentTemplates = componentTemplatesData.ToDictionary(l => l.Name, l => new ComponentTemplateResource
                {
                    Name = l.Name,
                    Hash = l.Hash,
                    Content = l.Content
                });
            }

            //_lifecyclePoliciesPath = Path.Combine(_opts.ResourcesPath, "lifecycle-policies");
            //_indexTemplatesPath = Path.Combine(_opts.ResourcesPath, "index-templates");
            //_componentTemplatesPath = Path.Combine(_opts.ResourcesPath, "component-templates");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
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
                res.Add(componentData);
            }

            return res;
        }

        private async Task LoadIndexesDirectoryAsync(CancellationToken cancellationToken)
        {
            var indexResourcePath = Path.Combine(_opts.ResourcesPath, "indexes");
            var resDir = new DirectoryInfo(indexResourcePath);

            if (!resDir.Exists)
                throw new InvalidOperationException("Index directory not found")
                    .AndFactIs("path", indexResourcePath);

            MappingResource defaultMapping = null;

            var defaultMappingFile = new FileInfo(Path.Combine(indexResourcePath, MappingFilename));
            if (defaultMappingFile.Exists)
            {
                var defaultMappingData = await LoadResourceAsync<TypeMapping>(defaultMappingFile, cancellationToken);
                defaultMapping = new MappingResource
                {
                    Name = defaultMappingData.Name,
                    Content = defaultMappingData.Content,
                    Hash = defaultMappingData.Hash
                };
            }

            var namedDirs = new Dictionary<string, IndexResources>();

            foreach (var directoryInfo in resDir.GetDirectories())
            {
                MappingResource mappingResource = null;
                var mappingFile = new FileInfo(Path.Combine(directoryInfo.FullName, MappingFilename));
                if (mappingFile.Exists)
                {
                    var mappingResourceData = await LoadResourceAsync<TypeMapping>(mappingFile, cancellationToken);

                    mappingResource = new MappingResource
                    {
                        Name = mappingResourceData.Name,
                        Content = mappingResourceData.Content,
                        Hash = mappingResourceData.Hash
                    };
                }

                SqlResource kickQueryResource = null;
                var kickQueryFile = new FileInfo(Path.Combine(directoryInfo.FullName, KickFilename));
                if (kickQueryFile.Exists)
                {
                    var kickQueryResourceData = await LoadResourceAsync<string>(kickQueryFile, cancellationToken);

                    kickQueryResource = new SqlResource
                    {
                        Name = kickQueryResourceData.Name,
                        Content = kickQueryResourceData.Content,
                        Hash = kickQueryResourceData.Hash
                    };
                }

                SqlResource syncQueryResource = null;
                var syncQueryFile = new FileInfo(Path.Combine(directoryInfo.FullName, SyncFilename));
                if (syncQueryFile.Exists)
                {
                    var syncQueryResourceData = await LoadResourceAsync<string>(syncQueryFile, cancellationToken);

                    syncQueryResource = new SqlResource
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
                DefaultMapping = defaultMapping,
                Named = namedDirs
            };
        }

        async Task<(string Name, string Hash, T Content)> LoadResourceAsync<T>(FileInfo file, CancellationToken cancellationToken)
        {
            var buff = new byte[file.Length];
            await using var readStream = file.OpenRead();
            // ReSharper disable once MustUseReturnValue
            await readStream.ReadAsync(buff, cancellationToken);

            T content;

            //if (typeof(T) == typeof(string))
            //{
            //    content = (T)Encoding.UTF8.GetString(buff);
            //}
            //else
            //{
                await using var stream = new MemoryStream(buff);
                content = _esTools.Serializer.Deserialize<T>(stream);
            //}
            

            var hash = HashCalculator.Calculate(buff);

            return (Path.GetFileNameWithoutExtension(file.Name), hash, content);
        }

        public async Task<string> ProvideIndexMappingAsync(string indexId)
        {
            var mappingJsonPath = Path.Combine(_indexResourcePath, indexId, MappingFilename);
            var mappingJson = await ReadFileAsync(mappingJsonPath, throwIfDoesNotExists: false);

            var commonMappingJsonPath = Path.Combine(_indexResourcePath, MappingFilename);
            var commonMappingJson = await ReadFileAsync(commonMappingJsonPath, throwIfDoesNotExists: false);

            if (mappingJson == null && commonMappingJson == null)
                throw new FileNotFoundException("Mapping not found")
                    .AndFactIs("index-id", indexId)
                    .AndFactIs("mapping-file", mappingJsonPath)
                    .AndFactIs("common-file", commonMappingJsonPath);

            if (commonMappingJson == null)
                return mappingJson;

            if (mappingJson == null)
                return commonMappingJson;

            var mappingJObj = JObject.Parse(mappingJson);
            var resultMappingJson = JObject.Parse(commonMappingJson);

            resultMappingJson.Merge(mappingJObj);

            return resultMappingJson.ToString(Formatting.None);
        }
    }
}

using System;
using System.Collections.Generic;
using MyLab.Log;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyLab.Search.Indexer.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using MyLab.Log.Dsl;

namespace MyLab.Search.Indexer.Services
{
    class FileResourceProvider : IResourceProvider
    {
        private readonly IndexerOptions _opts;

        private readonly string _indexResourcePath;
        private readonly string _lifecyclePoliciesPath;
        private readonly string _indexTemplatesPath;
        private readonly string _componentTemplatesPath;
        private readonly IDslLogger _log;

        private const string KickFilename = "kick.sql";
        private const string SyncFilename = "sync.sql";
        private const string IndexFilename = "index.json";
        private const string MappingFilename = "mapping.json";

        public FileResourceProvider(IOptions<IndexerOptions> opts,
            ILogger<FileResourceProvider> logger = null)
            : this(opts.Value, logger)
        {
            
        }
        
        public FileResourceProvider(IndexerOptions opts,
            ILogger<FileResourceProvider> logger = null)
        {
            _opts = opts;
            _log = logger?.Dsl();
            _indexResourcePath = Path.Combine(opts.ResourcesPath, "indexes");
            _lifecyclePoliciesPath = Path.Combine(opts.ResourcesPath, "lifecycle-policies");
            _indexTemplatesPath = Path.Combine(opts.ResourcesPath, "index-templates");
            _componentTemplatesPath = Path.Combine(opts.ResourcesPath, "component-templates");
        }

        public async Task<string> ProvideKickQueryAsync(string indexId)
        {
            var idxOpts = _opts.GetIndexOptions(indexId);

            if (idxOpts.KickDbQuery != null) 
                return idxOpts.KickDbQuery;

            var filePath = Path.Combine(_indexResourcePath, indexId, KickFilename);

            return await ReadFileAsync(filePath);
        }

        public async Task<string> ProvideSyncQueryAsync(string indexId)
        {
            var idxOpts = _opts.GetIndexOptions(indexId);

            if (idxOpts.SyncDbQuery != null)
                return idxOpts.SyncDbQuery;

            var filePath = Path.Combine(_indexResourcePath, indexId, SyncFilename);

            return await ReadFileAsync(filePath);
        }

        public async Task<string> ProvideIndexMappingAsync(string indexId)
        {
            var indexJsonPath = Path.Combine(_indexResourcePath, indexId, IndexFilename);
            if (File.Exists(indexJsonPath))
            {
                _log?.Warning("'index.json' is no longer supported")
                    .AndFactIs("filename", indexJsonPath)
                    .Write();
            }

            var commonIndexJsonPath = Path.Combine(_indexResourcePath, IndexFilename);
            if (File.Exists(commonIndexJsonPath))
            {
                _log?.Warning("'index.json' is no longer supported")
                    .AndFactIs("filename", commonIndexJsonPath)
                    .Write();
            }

            var mappingJsonPath = Path.Combine(_indexResourcePath, indexId, MappingFilename);
            var mappingJson = await ReadFileAsync(mappingJsonPath, throwIfDoesNotExists: false);

            var commonMappingJsonPath = Path.Combine(_indexResourcePath, MappingFilename);
            var commonMappingJson = await ReadFileAsync(commonMappingJsonPath, throwIfDoesNotExists: false);

            if (mappingJson == null && commonMappingJson == null)
                throw new FileNotFoundException("Resource not found")
                    .AndFactIs("index-id", indexId)
                    .AndFactIs("index-file", indexJsonPath)
                    .AndFactIs("common-file", commonIndexJsonPath);

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
    }
}

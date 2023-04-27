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

namespace MyLab.Search.Indexer.Services
{
    class FileResourceProvider : IResourceProvider
    {
        private readonly IndexerOptions _opts;

        private readonly string _indexResourcePath;
        private readonly string _lifecyclePoliciesPath;
        private readonly string _indexTemplatesPath;
        private readonly string _componentTemplatesPath;

        private const string KickFilename = "kick.sql";
        private const string SyncFilename = "sync.sql";
        private const string IndexFilename = "index.json";

        public FileResourceProvider(IOptions<IndexerOptions> opts)
            : this(opts.Value)
        {
            
        }
        
        public FileResourceProvider(IndexerOptions opts)
        {
            _opts = opts;
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

            return await ReadFileAsync(filePath, throwIfDoesNotExists: true);
        }

        public async Task<string> ProvideSyncQueryAsync(string indexId)
        {
            var idxOpts = _opts.GetIndexOptions(indexId);

            if (idxOpts.SyncDbQuery != null)
                return idxOpts.SyncDbQuery;

            var filePath = Path.Combine(_indexResourcePath, indexId, SyncFilename);

            return await ReadFileAsync(filePath, throwIfDoesNotExists: true);
        }

        public async Task<string> ProvideIndexSettingsAsync(string indexId)
        {
            var indexJsonPath = Path.Combine(_indexResourcePath, indexId, IndexFilename);
            var indexJson = await ReadFileAsync(indexJsonPath, throwIfDoesNotExists: false);

            var commonIndexJsonPath = Path.Combine(_indexResourcePath, IndexFilename);
            var commonIndexJson = await ReadFileAsync(commonIndexJsonPath, throwIfDoesNotExists: false);

            if (indexJson == null && commonIndexJson == null)
                throw new FileNotFoundException("Resource not found")
                    .AndFactIs("index-id", indexId)
                    .AndFactIs("index-file", indexJsonPath)
                    .AndFactIs("common-file", commonIndexJsonPath);

            if (commonIndexJson == null)
                return indexJson;

            if (indexJson == null)
                return commonIndexJson;

            var indexJObj = JObject.Parse(indexJson);
            var resultJson = JObject.Parse(commonIndexJson);

            resultJson.Merge(indexJObj);

            return resultJson.ToString(Formatting.None);
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

        async Task<string> ReadFileAsync(string path, bool throwIfDoesNotExists)
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

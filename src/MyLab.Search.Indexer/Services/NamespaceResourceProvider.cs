using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyLab.Log;

namespace MyLab.Search.Indexer.Services
{
    class NamespaceResourceProvider : INamespaceResourceProvider
    {
        private readonly IndexerOptions _options;

        public NamespaceResourceProvider(IOptions<IndexerOptions> options)
            :this(options.Value)
        {
        }
        public NamespaceResourceProvider(IndexerOptions options)
        {
            _options = options;
        }
        public Task<string> ReadFileAsync(string nsId, string filename)
        {
            var path = Path.Combine(_options.NamespacesPath, nsId, filename);

            if(!File.Exists(path))
                throw new InvalidOperationException("Job file not found")
                    .AndFactIs("filepath", path);

            return File.ReadAllTextAsync(path);
        }
    }
}
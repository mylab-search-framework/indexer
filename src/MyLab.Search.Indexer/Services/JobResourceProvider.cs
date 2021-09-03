using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyLab.Logging;

namespace MyLab.Search.Indexer.Services
{
    class JobResourceProvider : IJobResourceProvider
    {
        private readonly IndexerOptions _options;

        public JobResourceProvider(IOptions<IndexerOptions> options)
            :this(options.Value)
        {
        }
        public JobResourceProvider(IndexerOptions options)
        {
            _options = options;
        }
        public Task<string> ReadFileAsync(string jobId, string filename)
        {
            var path = Path.Combine(_options.JobPath, jobId, filename);

            if(!File.Exists(path))
                throw new InvalidOperationException("Job file not found")
                    .AndFactIs("filepath", path);

            return File.ReadAllTextAsync(path);
        }
    }
}
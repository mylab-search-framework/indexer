using MyLab.Search.EsTest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using MyLab.Search.EsAdapter.Tools;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class FileResourceProviderBehavior : IClassFixture<EsFixture<TestEsFixtureStrategy>>
    {
        private readonly FileResourceProvider _resourceProvider;

        public FileResourceProviderBehavior(EsFixture<TestEsFixtureStrategy> fxt, ITestOutputHelper output)
        {
            fxt.Output = output;

            var opts = new IndexerOptions
            {
                ResourcesPath = Path.Combine(Directory.GetCurrentDirectory(), "for-loaders-test")
            };

            _resourceProvider = new FileResourceProvider(fxt.Tools, opts);
        }

        void Should
    }
}

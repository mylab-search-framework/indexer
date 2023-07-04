using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MyLab.RabbitClient;
using MyLab.RabbitClient.Connection;
using MyLab.Search.EsAdapter.Tools;
using MyLab.Search.Indexer.Services;
using Nest;

namespace FuncTests
{
    static class TestTools
    {
        public const string EsUrl = "http://localhost:9200";

        public static IRabbitChannelProvider RabbitChannelProvider { get; }
        
        static TestTools()
        {
            var rabbitConnectionProvider = new LazyRabbitConnectionProvider(new RabbitOptions
            {
                Host = "localhost",
                User = "guest",
                Password = "guest"
            });
            RabbitChannelProvider = new RabbitChannelProvider(rabbitConnectionProvider);
        }

        public static async Task RemoveTargetFromAliasAsync(IEsTools tools, string alias)
        {
            var aliases = await tools.Aliases().GetAliasesAsync(a => a.Name(alias));
            var foundAlias = aliases.FirstOrDefault(a => a.AliasName == alias);
            if (foundAlias != null)
            {
                var indexExists = await tools.Index(foundAlias.TargetName).ExistsAsync();
                if (indexExists) await tools.Index(foundAlias.TargetName).DeleteAsync();

                var streamExists = await tools.Stream(foundAlias.TargetName).ExistsAsync();
                if (streamExists) await tools.Stream(foundAlias.TargetName).DeleteAsync();
            }
        }

        public static string CreateTestName<TTest>()
        {
            return $"test-{typeof(TTest).Name.ToLower()}-{Guid.NewGuid():N}";
        }

        public static Func<IServiceProvider, IResourceProvider> CreateResourceProviderWrapper(string mapFromIdxId, string mapToIdxId)
        {
            return sp =>
            {
                var resourceProviderMock = new Mock<IResourceProvider>();

                var originResourceProvider = ActivatorUtilities.CreateInstance<FileResourceProvider>(sp);

                resourceProviderMock.SetupAllProperties();

                resourceProviderMock
                    .SetupGet(rp => rp.IndexDirectory)
                    .Returns(() => new IndexResourceDirectory
                    {
                        Named = new Dictionary<string, IndexResources>
                        {
                            {
                                mapFromIdxId,
                                originResourceProvider.IndexDirectory.Named[mapToIdxId]
                            }
                        }
                    });
                resourceProviderMock.SetupGet(rp => rp.IndexTemplates)
                    .Returns(() => new NamedResources<IndexTemplate>());
                resourceProviderMock.SetupGet(rp => rp.ComponentTemplates)
                    .Returns(() => new NamedResources<ComponentTemplate>());
                resourceProviderMock.SetupGet(rp => rp.LifecyclePolicies)
                    .Returns(() => new NamedResources<LifecyclePolicy>());

                resourceProviderMock
                    .Setup(rp => rp.LoadAsync(It.IsAny<CancellationToken>()))
                    .Returns<CancellationToken>(ct => originResourceProvider.LoadAsync(ct));

                return resourceProviderMock.Object;
            };
        }
    }
}
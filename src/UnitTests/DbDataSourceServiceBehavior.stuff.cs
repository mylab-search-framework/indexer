using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using Moq;
using MyLab.DbTest;
using MyLab.Search.Indexer.Options;
using MyLab.Search.Indexer.Services;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests
{
    public partial class DbDataSourceServiceBehavior
    {
        private readonly TmpDbFixture<DbInitializer> _dbFxt;

        public DbDataSourceServiceBehavior(TmpDbFixture<DbInitializer> dbFxt, ITestOutputHelper output)
        {
            _dbFxt = dbFxt;
            _dbFxt.Output = output;
        }

        void AssertDoc(TestDoc doc, JObject jObject)
        {
            Assert.NotNull(jObject);
            Assert.Equal(doc.Id, jObject.Property("id").Value.Value<int>());
            Assert.Equal(doc.Content, jObject.Property("content").Value.Value<string>());
        }
        

        private class TestSeedService : ISeedService
        {
            public Seed Seed { get; set; }

            public Task SaveSeedAsync(string indexId, Seed seed)
            {
                Seed = seed;
                return Task.CompletedTask;
            }

            public Task<Seed> LoadSeedAsync(string indexId)
            {
                return Task.FromResult(Seed);
            }
        }

        public class DbInitializer : ITestDbInitializer
        {
            public async Task InitializeAsync(DataConnection dc)
            {
                await dc.CreateTableAsync<TestDoc>();
            }
        }

        class TableFiller : ITestDbInitializer
        {
            private readonly TestDoc[] _initialDocs;

            public TableFiller(TestDoc[] initialDocs)
            {
                _initialDocs = initialDocs;
            }
            public async Task InitializeAsync(DataConnection dc)
            {
                await dc.BulkCopyAsync(_initialDocs);
            }
        }

        [Table("docs")]
        private class TestDoc
        {
            [PrimaryKey, Column("id")] public long Id { get; set; }
            [Column("content")] public string Content { get; set; }
            [Column("last_change_dt")] public DateTime LastChangeDt { get; set; }
        }

        IResourceProvider CreateKickResourceProvider(string indexId, string kickQuery)
        {
            var resourceProvider = new Mock<IResourceProvider>();
            resourceProvider.SetupGet(p => p.IndexDirectory)
                .Returns(() => new IndexResourceDirectory
                {
                    Named = new Dictionary<string, IndexResources>
                    {
                        {
                            indexId,
                            new IndexResources
                            {
                                KickQuery = new Resource<string>
                                {
                                    Content = kickQuery,
                                    Name = "foo",
                                    Hash = "hash"
                                }
                            }
                        }
                    }
                });

            return resourceProvider.Object;
        }

        IResourceProvider CreateSyncResourceProvider(string indexId, string syncQuery)
        {
            var resourceProvider = new Mock<IResourceProvider>();
            resourceProvider.SetupGet(p => p.IndexDirectory)
                .Returns(() => new IndexResourceDirectory
                {
                    Named = new Dictionary<string, IndexResources>
                    {
                        {
                            indexId,
                            new IndexResources
                            {
                                SyncQuery = new Resource<string>
                                {
                                    Content = syncQuery,
                                    Name = "foo",
                                    Hash = "hash"
                                }
                            }
                        }
                    }
                });

            return resourceProvider.Object;
        }
    }
}
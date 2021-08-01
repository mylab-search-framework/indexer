using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MyLab.Log;
using MyLab.Log.Dsl;
using MyLab.Search.EsAdapter;

namespace MyLab.Search.Indexer.Tools
{
    class JsonSettingsBasedCreateIndexStrategy : ICreateIndexStrategy
    {
        private readonly string _settingsJson;

        public IDslLogger Log { get; set; }

        public JsonSettingsBasedCreateIndexStrategy(string settingsJson)
        {
            _settingsJson = settingsJson;
        }

        public async Task CreateIndexAsync(IEsManager esMgr, string name, CancellationToken cancellationToken)
        {
            Log?.Action("Create index")
                .AndFactIs("name", name)
                .AndFactIs("settings", _settingsJson)
                .Write();


            await esMgr.CreateIndexAsync(name, _settingsJson, cancellationToken);
        }

        public static async Task<JsonSettingsBasedCreateIndexStrategy> LoadFormFileAsync(string filename, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filename))
                throw new InvalidOperationException("Index settings file not found")
                    .AndFactIs("File path", filename);

            var settings = await File.ReadAllTextAsync(filename, cancellationToken);

            return new JsonSettingsBasedCreateIndexStrategy(settings);
        }
    }
}
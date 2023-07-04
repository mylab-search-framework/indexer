using System.Collections.Generic;
using System.Threading.Tasks;
using MyLab.Search.EsAdapter.Tools;

namespace IntegrationTests
{
    static class TestTools
    {
        public static string GetComponentVer(IReadOnlyDictionary<string, object> metadata)
        {
            return !metadata.TryGetValue("ver", out object verObj) || verObj is not string verStr ? "1" : verStr;
        }
        public static string GetComponentVer(IDictionary<string, object> metadata)
        {
            return !metadata.TryGetValue("ver", out object verObj) || verObj is not string verStr ? "1" : verStr;
        }
    }
}

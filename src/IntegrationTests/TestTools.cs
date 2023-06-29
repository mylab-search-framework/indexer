using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MyLab.Search.Indexer.Services;

namespace IntegrationTests
{
    static class TestTools
    {
        //public static async Task<string> GetResourceHashAsync(IResource resource)
        //{
        //    await using var readStream = resource.OpenRead();

        //    byte[] buff = new byte[readStream.Length];

        //    await readStream.ReadAsync(buff);

        //    var hashBin = MD5.HashData(buff);

        //    return BitConverter.ToString(hashBin).Replace("-", "").ToLower();
        //}

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

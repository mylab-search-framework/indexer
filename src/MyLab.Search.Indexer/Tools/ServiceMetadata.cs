using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Nest;

namespace MyLab.Search.Indexer.Tools
{
    static class ServiceMetadata
    {
        private const string ComponentHashKey = "mylab-indexer:src-hash";

        public static bool TryGetComponentHash(IReadOnlyDictionary<string, object> metadata, out string hash)
        {
            if (metadata == null || !metadata.TryGetValue(ComponentHashKey, out var hashObj) || hashObj is not string hasStr)
            {
                hash = null;
                return false;
            }

            hash = hasStr;
            return true;
        }

        public static bool TryGetComponentHash(IDictionary<string, object> metadata, out string hash)
        {
            if (metadata == null || !metadata.TryGetValue(ComponentHashKey, out var hashObj) || hashObj is not string hasStr)
            {
                hash = null;
                return false;
            }

            hash = hasStr;
            return true;
        }

        public static void SaveComponentHash(IDictionary<string, object> metadata, string newHash)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));
            if (newHash == null) throw new ArgumentNullException(nameof(newHash));

            if (metadata.ContainsKey(ComponentHashKey))
            {
                metadata[ComponentHashKey] = newHash;
            }
            else
            {
                metadata.Add(ComponentHashKey, newHash);
            }
        }


        static string NormHash(string hash) => hash.Replace("-", "").ToLower();
    }
}

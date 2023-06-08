using System;
using System.Security.Cryptography;

namespace MyLab.Search.Indexer.Tools
{
    static class HashCalculator
    {
        public static string Calculate(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            return NormalizeHash(BitConverter.ToString(MD5.HashData(data)).Replace("-", ""));
        }

        public static string NormalizeHash(string hash) => hash.ToLower();
    }
}

using System;
using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Services
{
    static class SeedServiceExtensions
    {
        public static Task WriteDateTimeAsync(this ISeedService srv, string nsId, DateTime seed)
        {
            return srv.WriteAsync(nsId, seed.ToString("O"));
        }

        public static async Task<DateTime> ReadDateTimeAsync(this ISeedService srv, string nsId)
        {
            var strSeed = await srv.ReadAsync(nsId);

            return strSeed != null
                ? DateTime.Parse(strSeed)
                : DateTime.MinValue;
        }

        public static Task WriteIdAsync(this ISeedService srv, string nsId, long seed)
        {
            return srv.WriteAsync(nsId, seed.ToString());
        }

        public static async Task<long> ReadIdAsync(this ISeedService srv, string nsId)
        {
            var strSeed = await srv.ReadAsync(nsId);

            return strSeed != null
                ? long.Parse(strSeed)
                : long.MinValue;
        }
    }
}
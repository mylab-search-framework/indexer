using System;
using System.Threading.Tasks;

namespace MyLab.Search.Indexer.Services
{
    static class SeedServiceExtensions
    {
        public static Task WriteDateTimeAsync(this ISeedService srv, DateTime seed)
        {
            return srv.WriteAsync(seed.ToString("O"));
        }

        public static async Task<DateTime> ReadDateTimeAsync(this ISeedService srv)
        {
            var strSeed = await srv.ReadAsync();

            return strSeed != null
                ? DateTime.Parse(strSeed)
                : DateTime.MinValue;
        }

        public static Task WriteIdAsync(this ISeedService srv, long seed)
        {
            return srv.WriteAsync(seed.ToString());
        }

        public static async Task<long> ReadIdAsync(this ISeedService srv)
        {
            var strSeed = await srv.ReadAsync();

            return strSeed != null
                ? long.Parse(strSeed)
                : long.MinValue;
        }
    }
}
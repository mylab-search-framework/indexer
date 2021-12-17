using System;

namespace MyLab.Search.Indexer.Tools
{
    static class UnixDateTimeConverter
    {
        public static double ToUnixDt(DateTime dt)
        {
            return (dt.ToUniversalTime() - DateTime.UnixEpoch).TotalMilliseconds;
        }

        public static double ToUnixDt(string dt)
        {
            return ToUnixDt(DateTime.Parse(dt));
        }
    }
}

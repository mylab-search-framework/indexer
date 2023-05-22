using System;
using System.Threading.Tasks;
using MyLab.Log;

namespace MyLab.Search.Indexer.Services
{
    public interface ISeedService
    {
        Task SaveSeedAsync(string indexId, Seed seed);
        Task<Seed> LoadSeedAsync(string indexId);
    }

    public class Seed
    {
        public long Long { get; }
        public DateTime DataTime { get; }
        
        public bool IsLong { get; }
        public bool IsDateTime { get; }

        public Seed(long longValue)
        {
            Long = longValue;
            IsLong = true;
        }

        public Seed(DateTime dateTimeValue)
        {
            DataTime = dateTimeValue;
            IsDateTime = true;
        }

        public override string ToString()
        {
            return IsLong ? Long.ToString("D") : DataTime.ToString("O");
        }

        public static Seed Parse(string strValue)
        {
            if (long.TryParse(strValue, out var longVal))
            {
                return new Seed(longVal);
            }

            if (DateTime.TryParse(strValue, out var dtVal))
            {
                return new Seed(dtVal);
            }

            throw new FormatException("A seed has wrong format")
                .AndFactIs("origin-value", strValue);
        }

        public static implicit operator Seed(long longValue) => new (longValue);
        public static implicit operator Seed(DateTime dateTimeValue) => new (dateTimeValue);
    }
}

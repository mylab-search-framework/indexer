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

        public static bool operator ==(Seed seed, long longValue) => seed is { IsLong: true } && seed.Long == longValue;
        public static bool operator !=(Seed seed, long longValue) => !(seed == longValue);
        public static bool operator ==(Seed seed, DateTime dateTimeValue) => seed is {IsDateTime: true } && seed.DataTime == dateTimeValue;
        public static bool operator !=(Seed seed, DateTime dateTimeValue) => !(seed== dateTimeValue);

        public override bool Equals(object obj)
        {
            if(obj == null) return false;

            if (obj is DateTime dateTimeValue)
                return Equals(dateTimeValue);

            if (obj is long longValue)
                return Equals(longValue);

            if (obj is Seed seedVal)
                return Equals(seedVal);

            return base.Equals(obj);
        }

        protected bool Equals(DateTime dtValue)
        {
            return this == dtValue;
        }

        protected bool Equals(long longValue)
        {
            return this == longValue;
        }
        protected bool Equals(Seed other)
        {
            return Long == other.Long && DataTime.Equals(other.DataTime) && IsLong == other.IsLong && IsDateTime == other.IsDateTime;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Long, DataTime, IsLong, IsDateTime);
        }
    }
}

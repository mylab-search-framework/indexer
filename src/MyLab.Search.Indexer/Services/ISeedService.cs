using System;
using System.Threading.Tasks;
using MyLab.Log;
using Org.BouncyCastle.Asn1.Cms;

namespace MyLab.Search.Indexer.Services
{
    public interface ISeedService
    {
        Task SaveSeedAsync(string indexId, Seed seed);
        Task<Seed> LoadSeedAsync(string indexId);
    }

    public class Seed
    {
        public static readonly Seed Empty = new ();

        public long Long { get; } = -1;
        public DateTime DateTime { get; }
        
        public bool IsLong { get; }
        public bool IsDateTime { get; }

        public bool IsEmpty => !IsLong && !IsDateTime || IsLong && Long == -1 || IsDateTime && DateTime == default;

        public Seed(long longValue)
        {
            Long = longValue;
            IsLong = true;
        }

        public Seed(DateTime dateTimeValue)
        {
            DateTime = dateTimeValue;
            IsDateTime = true;
        }
        
        public Seed()
        {
        }

        public override string ToString()
        {
            return IsEmpty ? "empty" : IsLong ? Long.ToString("D") : DateTime.ToString("O");
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
        public static bool operator ==(Seed seed, DateTime dateTimeValue) => seed is {IsDateTime: true } && seed.DateTime == dateTimeValue;
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
            if (other.IsEmpty && IsEmpty) return true;

            return Long == other.Long && DateTime.Equals(other.DateTime) && IsLong == other.IsLong && IsDateTime == other.IsDateTime;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Long, DateTime, IsLong, IsDateTime);
        }
    }
}

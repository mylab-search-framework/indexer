using System;
using System.Collections.Generic;

namespace MyLab.Search.Indexer.Tools
{
    class ServiceMetadata
    {
        public const string MyCreator = "mylab:searchfx-indexer";

        public string Creator { get; set; }
        public DateTime PutDt { get; set; }
        public string CreatorVer { get; set; }
        public string Ver { get; set; }

        public bool IsMyCreator() => Creator == MyCreator;

        public static ServiceMetadata Read(IDictionary<string, object> metadata)
        {
            if (metadata == null)
                return null;

            metadata.TryGetValue("mylab:creator", out var creator);
            metadata.TryGetValue("mylab:put-dt", out var putDt);
            metadata.TryGetValue("mylab:creator-ver", out var creatorVer);
            metadata.TryGetValue("mylab:ver", out var ver);

            DateTime putDateTime = default;

            if (putDt is string putDtStr)
            {
                DateTime.TryParse(putDtStr, out putDateTime);
            }

            return new ServiceMetadata
            {
                Creator = creator as string,
                CreatorVer = creatorVer as string,
                PutDt = putDateTime,
                Ver = ver as string
            };
        }
    }
}

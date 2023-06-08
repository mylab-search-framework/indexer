using System;
using System.Collections.Generic;
using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.Services.ComponentUploading
{
    class ComponentMetadata
    {
        public const string MetadataKey = "mylab_indexer";

        [DictProperty("owner")]
        public string Owner { get; set; }

        [DictProperty("src_hash")]
        public string SourceHash { get; set; }

        public static bool TryGet(IReadOnlyDictionary<string, object> metadata, out ComponentMetadata srvMeta)
        {
            if (metadata == null || !metadata.TryGetValue(MetadataKey, out var mdObject) || mdObject is not IDictionary<string, object> mdDict)
            {
                srvMeta = null;
                return false;
            }

            srvMeta = DictionarySerializer.Deserialize<ComponentMetadata>(mdDict);
            return true;
        }

        public static bool TryGet(IDictionary<string, object> metadata, out ComponentMetadata srvMeta)
        {
            if (metadata == null || !metadata.TryGetValue(MetadataKey, out var mdObject) || mdObject is not IDictionary<string, object> mdDict)
            {
                srvMeta = null;
                return false;
            }

            srvMeta = DictionarySerializer.Deserialize<ComponentMetadata>(mdDict);
            return true;
        }

        public void Save(IDictionary<string, object> metadata)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));

            var dstDict = new Dictionary<string, object>();
            DictionarySerializer.Serialize(dstDict, this);

            if (metadata.ContainsKey(MetadataKey))
            {
                metadata[MetadataKey] = dstDict;
            }
            else
            {
                metadata.Add(MetadataKey, dstDict);
            }
        }
    }
}

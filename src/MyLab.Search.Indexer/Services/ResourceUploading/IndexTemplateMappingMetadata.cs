using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.Services.ResourceUploading
{
    class IndexTemplateMappingMetadata
    {
        public const string MetadataKey = "mylab_indexer";

        private readonly Dictionary<string, Item> _entities;
        public IReadOnlyDictionary<string, Item> Entities { get; }

        public static bool TryGet(IDictionary<string, object> dict, out IndexTemplateMappingMetadata metadata)
        {
            if (dict == null || !dict.TryGetValue(MetadataKey, out var foundNode) ||
                foundNode is not IDictionary<string, object> foundDict)
            {
                metadata = null;
                return false;
            }

            var resultDict = new Dictionary<string, Item>();

            foreach (var p in foundDict)
            {
                if (p.Value is IDictionary<string, object> dictValue)
                {
                    resultDict.Add(p.Key, DictionarySerializer.Deserialize<Item>(dictValue));
                }
            }

            metadata = new IndexTemplateMappingMetadata(resultDict);
            return true;
        }

        public void Save(IDictionary<string, object> dict)
        {
            if (dict == null) throw new ArgumentNullException(nameof(dict));

            Dictionary<string, object> metadataDict;

            if (dict.TryGetValue(MetadataKey, out var foundMetadata))
            {
                if (foundMetadata is Dictionary<string, object> castedMetadataDict)
                {
                    metadataDict = castedMetadataDict;
                }
                else
                {
                    dict[MetadataKey] = metadataDict = new Dictionary<string, object>();
                }
            }
            else
            {
                dict.Add(MetadataKey, metadataDict = new Dictionary<string, object>());
            }

            foreach (var entity in _entities)
            {
                var entityDict = new Dictionary<string, object>();

                DictionarySerializer.Serialize(entityDict, entity.Value);

                metadataDict.Add(entity.Key, entityDict);
            }
        }

        public IndexTemplateMappingMetadata(string key,Item item)
            : this(new Dictionary<string, Item>{ {key, item} })
        {
        }

        public IndexTemplateMappingMetadata()
            :this(new Dictionary<string, Item>())
        {
        }

        public IndexTemplateMappingMetadata(IDictionary<string, Item> initial)
        {
            _entities = new Dictionary<string, Item>(initial);
            Entities = new ReadOnlyDictionary<string, Item>(_entities);
        }

        public class Item
        {
            [DictProperty("owner")]
            public string Owner { get; set; }

            [DictProperty("source_name")]
            public string SourceName { get; set; }
        }
    }
}
using System;
using System.Collections.Generic;
using MyLab.Search.Indexer.Tools;

namespace MyLab.Search.Indexer.Services.ComponentUploading
{
    class MappingMetadata
    {
        public const string MetadataKey = "mylab_indexer";

        public const string CreatorKey = "creator"; 
        public const string TemplateKey = "template"; 

        public TemplateDesc Template { get; set; }
        public CreatorDesc Creator { get; init; }

        public static bool TryGet(IDictionary<string, object> dict, out MappingMetadata metadata)
        {
            if (dict == null || !dict.TryGetValue(MetadataKey, out var foundNode) ||
                foundNode is not IDictionary<string, object> foundDict)
            {
                metadata = null;
                return false;
            }

            CreatorDesc creator = null;

            if (foundDict.TryGetValue(CreatorKey, out var creatorDictObj) &&
                creatorDictObj is IDictionary<string, object> creatorDict)
            {
                creator = DictionarySerializer.Deserialize<CreatorDesc>(creatorDict);
            }

            TemplateDesc template = null;

            if (foundDict.TryGetValue(TemplateKey, out var templateDictObj) &&
                templateDictObj is IDictionary<string, object> templateDict)
            {
                template = DictionarySerializer.Deserialize<TemplateDesc>(templateDict);
            }

            metadata = new MappingMetadata
            {
                Creator = creator,
                Template = template
            };

            return true;
        }

        public void Save(IDictionary<string, object> dict)
        {
            if (dict == null) throw new ArgumentNullException(nameof(dict));

            var metaDict = new Dictionary<string, object>();

            if (Creator != null)
            {
                if (dict.ContainsKey(CreatorKey))
                    throw new Exception("A creator mapping info already exists");

                var creatorDict = new Dictionary<string, object>();
                DictionarySerializer.Serialize(creatorDict, Creator);

                metaDict.Add(CreatorKey, creatorDict);
            }

            if (Template != null)
            {
                if (dict.ContainsKey(TemplateKey))
                    throw new Exception("A template mapping info already exists");

                var templateDict = new Dictionary<string, object>();
                DictionarySerializer.Serialize(templateDict, Template);

                metaDict.Add(TemplateKey, templateDict);
            }

            dict.Add(MetadataKey, metaDict);
        }

        public class TemplateDesc
        {
            [DictProperty("owner")]
            public string Owner { get; set; }

            [DictProperty("source_name")]
            public string SourceName { get; set; }
        }

        public class CreatorDesc
        {
            [DictProperty("owner")]
            public string Owner { get; set; }

            [DictProperty("source_hash")]
            public string SourceHash { get; set; }
        }
    }
}
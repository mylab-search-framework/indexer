using System;
using System.Collections.Generic;

namespace MyLab.Search.Indexer.Tools
{
    class ServiceMetadata
    {
        public const string MetaKey = "mylab_indexer_data";
        public const string MyCreator = "mylab-indexer";

        public const string HistoryKey = "history";
        public const string VerKey = "ver";
        public const string CreatorKey = "creator";

        public string Creator { get; set; }
        public string Ver { get; set; }
        public HistoryItem[] History { get; set; }
        public bool IsMyCreator() => Creator == MyCreator;

        public void Save(IDictionary<string, object> metadata)
        {
            if (metadata.ContainsKey(MetaKey))
            {
                metadata[MetaKey] = ToDictionary();
            }
            else
            {
                metadata.Add(MetaKey, metadata);
            }
        }

        public IDictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();

            if(Creator != null) { dict.Add(CreatorKey, Creator); }
            if(Ver != default) { dict.Add(VerKey, Ver); }

            if (History != null)
            {
                var historyDict = new Dictionary<string, object>();
                foreach (var historyItem in History)
                    historyItem.Save(historyDict);

                dict.Add(HistoryKey, historyDict);
            }

            return dict;
        }

        public static ServiceMetadata Extract(IDictionary<string, object> metadata)
        {
            if (metadata == null ||
                !metadata.TryGetValue(MetaKey, out var dataObj) ||
                dataObj is not IDictionary<string, object> dataDict)
            {
                return null;
            }

            dataDict.TryGetValue(CreatorKey, out var creator);
            dataDict.TryGetValue(VerKey, out var ver);

            HistoryItem[] history = null;

            if (dataDict.TryGetValue(HistoryKey, out var historyObj) &&
                historyObj is IDictionary<string, object> historyDict)
            {
                history = HistoryItem.Extract(historyDict);
            }

            return new ServiceMetadata
            {
                Creator = creator as string,
                Ver = ver as string,
                History = history is { Length: >0 } ? history : null
            };
        }

        

        public class HistoryItem
        {
            public const string ActorKey = "actor";
            public const string ActorVerKey = "actor_ver";
            public const string ComponentVerKey = "component_ver";

            public string Actor { get; set; }
            public string ActorVer { get; set; }
            public DateTime ActDt { get; set; }
            public string ComponentVer { get; set; }

            public static HistoryItem[] Extract(IDictionary<string, object> dict)
            {
                if (dict == null) return null;

                var items = new List<HistoryItem>();

                foreach (var pair in dict)
                {
                    if(pair.Value is not IDictionary<string, object> valDict)
                        continue;

                    if(!DateTime.TryParse(pair.Key, out var actDt))
                        continue;

                    var itm = Read(valDict);
                    itm.ActDt = actDt;

                    items.Add(itm);
                }

                return items.ToArray();
            }

            public static HistoryItem Read(IDictionary<string, object> dict)
            {
                if(dict == null) return null;

                dict.TryGetValue(ActorKey, out var actor);
                dict.TryGetValue(ActorVerKey, out var actorVer);
                dict.TryGetValue(ComponentVerKey, out var componentVer);

                return new HistoryItem
                {
                    Actor = actor as string,
                    ActorVer = actorVer as string,
                    ComponentVer = componentVer as string
                };
            }

            public IDictionary<string, object> ToDictionary()
            {
                var newDict = new Dictionary<string, object>();

                if (Actor != null) newDict.Add(ActorKey, Actor);
                if (ActorVer != null) newDict.Add(ActorVerKey, ActorVer);
                if (ComponentVer != null) newDict.Add(ComponentVerKey, ComponentVer);

                return newDict;
            }

            public void Save(IDictionary<string, object> destDict)
            {
                var dtStr = ActDt.ToString("s");

                var newDict = ToDictionary();

                if (destDict.ContainsKey(dtStr))
                {
                    destDict[dtStr] = newDict;
                }
                else
                {
                    destDict.Add(dtStr, newDict);
                }
            }
        } 
    }
}

using System.Collections.Generic;
using Elasticsearch.Net;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;

namespace MyLab.Search.Indexer.Tools
{
    public class IndexEntitySerializer : ConnectionSettingsAwareSerializerBase
    {
        public IndexEntitySerializer(
            IElasticsearchSerializer builtinSerializer, 
            IConnectionSettingsValues connectionSettings) 
            : base(builtinSerializer, connectionSettings)
        {
        }
    }
}

using System.ComponentModel.DataAnnotations;
using System.Linq;
using MyLab.Search.Indexer.Services;
using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Models
{
    static class IndexingMqMessageExtensions
    {
        public static void Validate(this IndexingMqMessage msg)
        {
            if (string.IsNullOrEmpty(msg.IndexId))
                throw new ValidationException("'indexId' must be specified");

            if (msg.Put != null)
            {
                if (!msg.Put.All(HasIdProperty))
                    throw new ValidationException("Put entity must have an `id` property");
            }

            if (msg.Patch != null)
            {
                if (!msg.Patch.All(HasIdProperty))
                    throw new ValidationException("Patch entity must have an `id` property");
            }
        }

        public static IndexingRequest ExtractIndexingRequest(this IndexingMqMessage msg)
        {
            return new IndexingRequest
            {
                PostList = msg.Post.Select(JObjectToEntity).ToArray(),
                PutList = msg.Put.Select(JObjectToEntity).ToArray(),
                PatchList = msg.Patch.Select(JObjectToEntity).ToArray(),
                IndexId = msg.IndexId,
                DeleteList = msg.Delete
            };
        }

        static IndexingRequestEntity JObjectToEntity(JObject json)
        {
            var res = new IndexingRequestEntity
            {
                Entity = json
            };

            var idValueProp = json.Property("id");

            if (idValueProp is
                    {
                        HasValues: true, 
                        Value:
                        {
                            Type: not JTokenType.Null
                        }
                    }
                )
            {
                res.Id = idValueProp.Value.ToString();
            }

            return res;
        }

        static bool HasIdProperty(JObject arg)
        {
            return arg.Property("id") != null;
        }
    }
}

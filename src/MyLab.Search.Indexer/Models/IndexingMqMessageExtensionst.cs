using System.ComponentModel.DataAnnotations;
using System.Linq;
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

        static bool HasIdProperty(JObject arg)
        {
            return arg.Property("id") != null;
        }
    }
}

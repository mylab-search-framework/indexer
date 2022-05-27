using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Models
{
    static class IndexingRequestExtensions
    {
        public static void Validate(this IndexingRequest req)
        {
            if (string.IsNullOrEmpty(req.IndexId))
                throw new ValidationException("'indexId' must be specified");

            if (req.Put != null)
            {
                if (!req.Put.All(HasIdProperty))
                    throw new ValidationException("Put entity must have an `id` property");
            }

            if (req.Patch != null)
            {
                if (!req.Patch.All(HasIdProperty))
                    throw new ValidationException("Patch entity must have an `id` property");
            }
        }

        static bool HasIdProperty(JObject arg)
        {
            return arg.Property("id") != null;
        }
    }
}

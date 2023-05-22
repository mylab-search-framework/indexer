using System.ComponentModel.DataAnnotations;
using LinqToDB.Common;
using MyLab.Search.Indexer.Tools;
using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Models
{
    static class IndexingMqMessageExtensions
    {
        public static void Validate(this IndexingMqMessage msg)
        {
            if (string.IsNullOrEmpty(msg.IndexId))
                throw new ValidationException("'indexId' must be specified");
            
            CheckList(msg.Put);
            CheckList(msg.Patch);

            void CheckList(JObject[] list)
            {
                if (!list.IsNullOrEmpty())
                {
                    foreach (var obj in list)
                    {
                        obj.CheckIdProperty();
                    }
                }
            }
        }

        public static InputIndexingRequest ExtractIndexingRequest(this IndexingMqMessage msg)
        {
            return new InputIndexingRequest
            {
                PutList = msg.Put,
                PatchList = msg.Patch,
                IndexId = msg.IndexId,
                DeleteList = msg.Delete,
                KickList = msg.Kick
            };
        }
    }
}

using System.ComponentModel.DataAnnotations;
using MyLab.Log;
using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Tools
{
    public static class JObjectExtensions
    {
        public static void CheckIdProperty(this JObject json)
        {
            if (json.Property("id") == null)
                throw new ValidationException("Document id not found")
                    .AndFactIs("json", json);
        }

        public static string GetIdProperty(this JObject json)
        {
            var idProp = json.Property("id");
            return idProp == null || idProp.Value.Type != JTokenType.Null
                ? idProp?.Value.ToString()
                : null;
        }
    }
}

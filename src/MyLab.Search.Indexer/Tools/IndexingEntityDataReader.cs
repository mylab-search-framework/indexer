using System;
using System.Data;
using System.Linq;
using MyLab.Log;
using MyLab.Search.Indexer.Models;
using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Tools
{
    static class IndexingEntityDataReader
    {
        private static readonly string[] DoubleCases = new[] { "decimal", "double", "float", "single", "real" };

        public static IndexingEntity Read(IDataReader reader)
        {
            JObject jsonEntity = new JObject();
            string id = null;

            for (var index = 0; index < reader.FieldCount; index++)
            {
                var name = reader.GetName(index);
                var typeName = reader.GetDataTypeName(index);

                jsonEntity.Add(name, ConvertValue(index, typeName));

                if (name.ToLower() == "id")
                {
                    id = reader.GetString(index);
                }
            }

            return new IndexingEntity
            {
                Id = id,
                Entity = jsonEntity
            };

            JToken ConvertValue(int index, string typeName)
            {
                var sc = StringComparison.InvariantCultureIgnoreCase;

                if (typeName.Contains("int", sc))
                    return JToken.FromObject(reader.GetInt64(index));

                if (DoubleCases.Any(c => c.Equals(typeName, sc)))
                    return JToken.FromObject(reader.GetDouble(index));

                if (typeName.Contains("bool", sc) || typeName.Equals("bit", sc))
                    return JToken.FromObject(reader.GetBoolean(index));

                if (typeName.Contains("date", sc))
                    return JToken.FromObject(reader.GetDateTime(index));

                if (typeName.Contains("char", sc))
                    return JToken.FromObject(reader.GetString(index));

                throw new NotSupportedException("DB field type is not supported")
                    .AndFactIs("db-type", typeName);
            }
        }
    }
}
using System;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using MyLab.Search.Indexer.Options;

namespace MyLab.Search.Indexer.Tools
{
    class KickQuery
    {
        public string Query { get; }

        public DataParameter[] Parameters { get; }

        public KickQuery(string query, DataParameter[] parameters)
        {
            Query = query;
            Parameters = parameters;
        }

        public static KickQuery Build(string queryPattern, string[] ids, IdPropertyType idPropertyType)
        {
            if (ids.IsNullOrEmpty())
                throw new InvalidOperationException("Kick id list is empty or null");

            DataParameter[] parameters;
            DataType idType;
            string query;

            switch (idPropertyType)
            {
                case IdPropertyType.String:
                    idType = DataType.Text;
                    break;
                case IdPropertyType.Int:
                    idType = DataType.Int64;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(idPropertyType), idPropertyType, null);
            }

            if (ids.Length == 1)
            {
                parameters = new[] { new DataParameter("id", ids[0], idType) };
                query = queryPattern;
            }
            else
            {
                parameters = ids
                    .Select((id, i) => new DataParameter("id" + i, id, idType))
                    .ToArray();

                var pIds = parameters.Select(p => "@" + p.Name);

                query = queryPattern.Replace("@id", string.Join(',', pIds));
            }

            return new KickQuery(query, parameters);
        }
    }
}

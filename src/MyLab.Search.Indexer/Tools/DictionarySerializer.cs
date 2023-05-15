using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static LinqToDB.Reflection.Methods.LinqToDB.Insert;
using DateTime = System.DateTime;

namespace MyLab.Search.Indexer.Tools
{
    static class DictionarySerializer
    {
        public static void Serialize(IDictionary<string, object> dict, object obj)
        {
            if (dict == null) throw new ArgumentNullException(nameof(dict));

            if(obj == null) return;

            var props = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var prop in props)
            {
                var pt = prop.PropertyType;

                object pVal = prop.GetValue(obj);
                object resVal;

                if (pVal == null) continue;

                if (pt.IsPrimitive || pt == typeof(string)) 
                    resVal = pVal.ToString();
                else if (pt == typeof(Guid)) 
                    resVal = ((Guid)pVal).ToString("N");
                else if (pt == typeof(DateTime)) 
                    resVal = ((DateTime)pVal).ToString("O");
                else
                {
                    var nestedDict = new Dictionary<string, object>();
                    Serialize(nestedDict, pVal);

                    resVal = nestedDict;
                }

                var dictPropAttr = (DictPropertyAttribute)Attribute.GetCustomAttribute(prop, typeof(DictPropertyAttribute));

                var propName = dictPropAttr?.Name ?? prop.Name;
                
                dict.Add(propName, resVal);
            }
        }

        public static T Deserialize<T>(IReadOnlyDictionary<string, object> dict) where T : class, new()
        {
            return Deserialize<T>((IDictionary<string, object>)new Dictionary<string, object>(dict));
        }

        public static T Deserialize<T>(IDictionary<string, object> dict) where T : class, new()
        {
            return (T)Deserialize(typeof(T), dict);
        }

        public static T Deserialize<T>(Dictionary<string, object> dict) where T : class, new()
        {
            return (T)Deserialize(typeof(T), dict);
        }

        static object Deserialize(Type resultType, IDictionary<string, object> dict)
        {
            if (resultType == null) throw new ArgumentNullException(nameof(resultType));
            if (dict == null) throw new ArgumentNullException(nameof(dict));

            var propsDict = resultType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(
                    p => Attribute.GetCustomAttribute(p, typeof(DictPropertyAttribute)) is DictPropertyAttribute attr ? attr.Name : p.Name,
                    p => p
                );

            var res = Activator.CreateInstance(resultType);

            foreach (var pair in dict)
            {
                if (pair.Value == null) continue;
                if (!propsDict.TryGetValue(pair.Key, out var prop)) continue;

                object newPropVal = null;

                if (pair.Value is string stringValue)
                {
                    if (prop.PropertyType == typeof(string)) newPropVal = stringValue;
                    else if (prop.PropertyType == typeof(Guid)) newPropVal = Guid.Parse(stringValue);
                    else if (prop.PropertyType == typeof(DateTime)) newPropVal = DateTime.Parse(stringValue);
                    else if (prop.PropertyType.IsPrimitive) newPropVal = Convert.ChangeType(stringValue, prop.PropertyType);
                    else throw new DictionarySerializationException(
                        $@"Unsupported conversion from string to '{prop.PropertyType.Name}'");
                }
                else
                {
                    if (pair.Value is IDictionary<string, object> dictVal)
                    {
                        newPropVal = Deserialize(prop.PropertyType, dictVal);
                    }
                    else if (pair.Value is IReadOnlyDictionary<string, object> roDictVal)
                    {
                        newPropVal = Deserialize(prop.PropertyType, new Dictionary<string, object>(roDictVal));
                    }
                    else newPropVal = prop;
                }

                prop.SetValue(res, newPropVal);
            }

            return res;
        }
    }

    class DictionarySerializationException : Exception
    {
        public DictionarySerializationException(string message) :base(message)
        {
            
        }
    }

    class DictPropertyAttribute : Attribute
    {
        public string Name { get; }

        public DictPropertyAttribute(string name)
        {
            Name = name;
        }
    }
}

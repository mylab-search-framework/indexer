using System;
using System.Linq.Expressions;
using System.Reflection;
using MyLab.Search.EsAdapter;

namespace MyLab.Search.Indexer.Tools
{
    static class OptionsValidatorTools
    {
        public static void CheckId(JobOptions options)
        {
            ThrowNotDefined(options, o => o.IdProperty);
        }

        public static void CheckEs(ElasticsearchOptions options)
        {
            ThrowNotDefined(options, o => o.DefaultIndex);
            ThrowNotDefined(options, o => o.Url);
        }

        public static void ThrowNotDefined<TOpt, TParam>(TOpt target, Expression<Func<TOpt, TParam>> propertyProvider)
        {
            var propExpr = (MemberExpression) propertyProvider.Body;
            var prop = (PropertyInfo) propExpr.Member;

            if(prop.GetValue(target) == (object) default(TParam))
                throw new InvalidOperationException($"Configuration parameter'{typeof(TOpt).Name}.{prop.Name}' required and not defined");
        }
    }
}
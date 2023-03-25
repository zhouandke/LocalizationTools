using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Localization2
{
    public class Lts : ILts
    {
        public static readonly Lts Default = new Lts();

        private readonly Dictionary<Type, TypeInfo> typeCache = new Dictionary<Type, TypeInfo>();
        private readonly DirectToStringBuilder directToStringBuilder;
        private LocalizationBuilderBase[] builders;

        public Lts()
        {
            directToStringBuilder = new DirectToStringBuilder(this);
            builders = new LocalizationBuilderBase[]
            {
                new CustomToStringBuilder(this),
                directToStringBuilder,
                new EnumerableToStringBuilder(this),
                new EnumToStringBuilder(this),
                new PropertyToStringBuilder(this),
            }
            .OrderBy(o => o.MatchOrder)
            .ThenBy(o => o.GetType().Name)
            .ToArray();
        }

        public bool IgnoreNullProperty { get; set; } = false;

        public object DefaultState { get; set; }

        public void AddLocalizationBuilder(LocalizationBuilderBase builder)
        {
            builders = builders.Concat(new[] { builder })
                .OrderBy(o => o.MatchOrder)
                .ThenBy(o => o.GetType().Name)
                .ToArray();
        }

        public void AddDirectToStringType(Type type)
        {
            directToStringBuilder.AddDirectToStringType(type);
        }

        public TypeInfo GetTypeInfo(Type type)
        {
            if (!typeCache.TryGetValue(type, out var typeInfo))
            {
                lock (typeCache)
                {
                    if (!typeCache.TryGetValue(type, out typeInfo))
                    {
                        typeInfo = new TypeInfo
                        {
                            Type = type
                        };
                        typeCache[type] = typeInfo;

                        foreach (var builder in builders)
                        {
                            if (builder.IsMatch(type))
                            {
                                builder.SetLocalizationer(typeInfo);
                            }
                        }
                    }
                }
            }
            return typeInfo;
        }

        public void SetCustomLocalization<TType, TImplement>()
            where TImplement : ILocalizationToString
        {
            var implementType = typeof(TImplement);
            var errorMsg = Help.CheckCustomLocalizationType(implementType);
            if (errorMsg != null)
            {
                throw new Exception(errorMsg);
            }
            var typeInfo = GetTypeInfo(typeof(TType));
            typeInfo.Localizationer = (ILocalizationToString)implementType.GetConstructor(new Type[0]).Invoke(new Type[0]);
        }

        public void SetCustomLocalization<TType, TImplement>(Expression<Func<TType, object>> exp)
            where TImplement : ILocalizationToString
        {
            var implementType = typeof(TImplement);
            var errorMsg = Help.CheckCustomLocalizationType(implementType);
            if (errorMsg != null)
            {
                throw new Exception(errorMsg);
            }

            var memberExpression = (exp.Body as UnaryExpression)?.Operand as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException($"{nameof(exp)} must like o=>o.name");
            }

            var typeInfo = GetTypeInfo(typeof(TType));
            var typePropertyInfo = typeInfo.TypePropertyInfos.FirstOrDefault(o => o.PropertyInfo.Name == memberExpression.Member.Name);
            if (typePropertyInfo == null)
            {
                throw new Exception($"{typeof(TType).Name} do not has property {memberExpression.Member.Name}");
            }

            typePropertyInfo.Localizationer = new Lazy<ILocalizationToString>(() => (ILocalizationToString)implementType.GetConstructor(new Type[0]).Invoke(new Type[0]));
        }

        public string Localization(object obj, Dictionary<string, string> customPropertyValues = null)
        {
            var context = new LocalizationStringContext(DefaultState, IgnoreNullProperty, customPropertyValues);
            return Localization(obj, context);
        }

        public string Localization(object obj, object state, Dictionary<string, string> customPropertyValues = null, string[] ignorePathes = null)
        {
            var context = new LocalizationStringContext(state ?? DefaultState, IgnoreNullProperty, customPropertyValues, ignorePathes);
            return Localization(obj, context);
        }

        public string Localization(object obj, LocalizationStringContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.Root = this;

            if (obj == null)
            {
                return null;
            }
            var typeInfo = GetTypeInfo(obj.GetType());
            return typeInfo.Localizationer.ToLocalization(obj, context, "", null, "");
        }
    }
}

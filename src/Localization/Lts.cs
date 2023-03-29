using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Localization2
{
    /// <summary>
    /// LocalizationToString
    /// </summary>
    public class Lts : ILts
    {
        public static readonly Lts Default = new Lts();

        private readonly Dictionary<Type, TypeInfo> typeCache = new Dictionary<Type, TypeInfo>();
        private readonly DirectToStringBuilder directToStringBuilder;
        private LocalizationBuilderBase[] builders = new LocalizationBuilderBase[0];

        public Lts()
        {
            directToStringBuilder = new DirectToStringBuilder(this);
            var newbuilders = new LocalizationBuilderBase[]
             {
                new CustomToStringBuilder(this),
                directToStringBuilder,
                new EnumerableToStringBuilder(this),
                new EnumToStringBuilder(this),
                new PropertyToStringBuilder(this),
             };
            AddLocalizationBuilder(newbuilders);
        }

        /// <summary>
        /// 是否忽略 null 的属性
        /// </summary>
        public bool IgnoreNullProperty { get; set; } = false;

        /// <summary>
        /// 用户传入的任何对象, 通常是 服务容器、ORM;
        /// </summary>
        public object DefaultState { get; set; }

        /// <summary>
        /// 增加 LocalizationBuilder
        /// </summary>
        /// <param name="builder"></param>
        public void AddLocalizationBuilder(IEnumerable<LocalizationBuilderBase> newbuilders)
        {
            builders = builders.Concat(newbuilders)
                .OrderBy(o => o.MatchOrder)
                .ThenBy(o => o.GetType().Name)
                .ToArray();
        }

        /// <summary>
        /// 增加 直接ToString() 的类型, 比如 int、long 等
        /// </summary>
        /// <param name="type"></param>
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
                                break;
                            }
                        }
                    }
                }
            }
            return typeInfo;
        }

        /// <summary>
        /// 为 Type 指定 ILocalizationToString
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <typeparam name="TImplement"></typeparam>
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
            typeInfo.Localizationer = (ILocalizationToString)implementType.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<Type>());
        }

        /// <summary>
        /// 为 Type的属性 指定 ILocalizationToString
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <typeparam name="TImplement"></typeparam>
        /// <param name="propertySelector"></param>
        public void SetCustomLocalization<TType, TImplement>(Expression<Func<TType, object>> propertySelector)
            where TImplement : ILocalizationToString
        {
            var implementType = typeof(TImplement);
            var errorMsg = Help.CheckCustomLocalizationType(implementType);
            if (errorMsg != null)
            {
                throw new Exception(errorMsg);
            }

            var memberExpression = (propertySelector.Body as UnaryExpression)?.Operand as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException($"{nameof(propertySelector)} must like o=>o.name");
            }

            var typeInfo = GetTypeInfo(typeof(TType));
            var typePropertyInfo = typeInfo.TypePropertyInfos.FirstOrDefault(o => o.PropertyInfo.Name == memberExpression.Member.Name);
            if (typePropertyInfo == null)
            {
                throw new Exception($"{typeof(TType).Name} do not has property {memberExpression.Member.Name}");
            }

            typePropertyInfo.Localizationer = new Lazy<ILocalizationToString>(() => (ILocalizationToString)implementType.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<Type>()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="customPropertyValues">用户传入的自定义值, Key 是属性路径(比如 Children[0].CityId ), value 是结果字符串</param>
        /// <returns></returns>
        public string Localization(object obj, Dictionary<string, string> customPropertyValues = null)
        {
            var context = new LocalizationStringContext(DefaultState, IgnoreNullProperty, customPropertyValues);
            return Localization(obj, context);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="state">用户传入的任何对象, 通常是 服务容器、ORM;</param>
        /// <param name="customPropertyValues">用户传入的自定义值, Key 是属性路径(比如 Children[0].CityId ), value 是结果字符串</param>
        /// <param name="ignorePaths">需要忽略的属性路径, 比如 Children.CityId, 数组不需要加下标 [0] </param>
        /// <returns></returns>
        public string Localization(object obj, object state, Dictionary<string, string> customPropertyValues = null, string[] ignorePaths = null)
        {
            var context = new LocalizationStringContext(state ?? DefaultState, IgnoreNullProperty, customPropertyValues, ignorePaths);
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
            return typeInfo.Localizationer.Localization(obj, context, "", null, "");
        }
    }
}

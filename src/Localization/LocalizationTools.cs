using Namotion.Reflection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Localization
{
    public static class LocalizationTools
    {
        // 缓存反射的结果
        private static readonly Dictionary<Type, TypeInfo> typePropertyCache = new Dictionary<Type, TypeInfo>(4096);//先分 4096 个容量吧
        private static readonly Dictionary<Type, Dictionary<object, string>> enumCache = new Dictionary<Type, Dictionary<object, string>>(4096);

        /// <summary>
        /// key value 之间的分隔符
        /// </summary>
        public static string KeyValueSeparator { get; set; } = ":";

        /// <summary>
        /// 需要双引号包裹的类型
        /// </summary>
        public static readonly HashSet<Type> WithQuotationTypes = new HashSet<Type>()
        {
            typeof(Char),
            typeof(String),

            typeof(Guid),

            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
        };

        public static readonly HashSet<Type> NumberTypes = new HashSet<Type>()
        {
            typeof(IntPtr),
            typeof(UIntPtr),

            typeof(Int16),
            typeof(Byte),
            typeof(SByte),
            typeof(UInt16),
            typeof(Int32),
            typeof(UInt32),
            typeof(Int64),
            typeof(UInt64),
            typeof(Single),
            typeof(Double),

            typeof(decimal),
        };


        private class TypeInfo
        {
            public Type Type { get; internal set; }

            public string ClassName { get; internal set; }

            public string DispalyName { get; internal set; }

            public TypePropertyInfo[] TypePropertyInfos { get; internal set; }

            public JsonType JsonType { get; internal set; }
        }


        private class TypePropertyInfo
        {
            public PropertyInfo PropertyInfo { get; internal set; }

            public string PropertyName { get; internal set; }

            public string DispalyName { get; internal set; }

            public ReplacePair[] ReplacePairs { get; internal set; }
        }


        private enum JsonType
        {
            ObjectType = 0,
            BoolType = 1,
            NumberType = 2,
            WithQuotationType = 3,
            EnumerableType = 4,

            EnumType = 10,
        }

        private static TypeInfo GetTypeInfo(Type type)
        {
            if (!typePropertyCache.TryGetValue(type, out var typeInfo))
            {
                lock (typePropertyCache)
                {
                    if (!typePropertyCache.TryGetValue(type, out typeInfo))
                    {
                        var typeSummary = type.GetXmlDocsSummary();
                        typeInfo = new TypeInfo()
                        {
                            Type = type,
                            ClassName = type.Name,
                            DispalyName = FindFirstNotEmptyName(type.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName, typeSummary, type.Name),
                            JsonType = GetJsonType(type)
                        };

                        var typePropertyInfoList = new List<TypePropertyInfo>();
                        if (typeInfo.JsonType == JsonType.ObjectType)
                        {
                            var contextualProperties = type.GetContextualProperties();
                            foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty))
                            {
                                var displayNameAttribute = propertyInfo.GetCustomAttribute<DisplayNameAttribute>();
                                var contextualPropertyInfo = contextualProperties.FirstOrDefault(o => o.Name == propertyInfo.Name);
                                var toStringReplacePairAttributes = propertyInfo.GetCustomAttributes<ToStringReplacePairAttribute>();

                                var dispalyName = FindFirstNotEmptyName(propertyInfo.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName, contextualPropertyInfo?.GetXmlDocsSummary(), propertyInfo.Name);
                                TypePropertyInfo typePropertyInfo = new TypePropertyInfo()
                                {
                                    PropertyName = propertyInfo.Name,
                                    DispalyName = dispalyName.Replace('\"', '\''),
                                    PropertyInfo = propertyInfo,
                                    ReplacePairs = toStringReplacePairAttributes.Count() > 0 ? toStringReplacePairAttributes?.SelectMany(o => o.ReplacePairs)?.ToArray() : null,
                                };
                                typePropertyInfoList.Add(typePropertyInfo);
                            }
                        }
                        typeInfo.TypePropertyInfos = typePropertyInfoList.ToArray();
                        typePropertyCache[type] = typeInfo;
                    }
                }
            }
            return typeInfo;
        }

        private static string FindFirstNotEmptyName(params string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                var name = names[i]?.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    return name;
                }
            }
            return "";
        }


        private static JsonType GetJsonType(Type type)
        {
            if (type == typeof(Boolean) || (IsNullable(type) && type.GetGenericArguments()[0] == typeof(Boolean)))
            {
                return JsonType.BoolType;
            }
            else if (NumberTypes.Contains(type) || (IsNullable(type) && NumberTypes.Contains(type.GetGenericArguments()[0])))
            {
                return JsonType.NumberType;
            }
            else if (WithQuotationTypes.Contains(type) || (IsNullable(type) && WithQuotationTypes.Contains(type.GetGenericArguments()[0])))
            {
                return JsonType.WithQuotationType;
            }
            else if (type.IsEnum || (IsNullable(type) && type.GetGenericArguments()[0].IsEnum))
            {
                return JsonType.EnumType;
            }
            else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            {
                return JsonType.EnumerableType;
            }
            else
            {
                return JsonType.ObjectType;
            }
        }

        private static bool IsNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// 获取枚举值的别名, 优先从 summary 中获取, 其次从 EnumAliasAttribute 中获取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetEnumAlias<T>(T enumValue)
            where T : struct
        {
            var type = typeof(T);
            return GetEnumAlias(type, enumValue);
        }

        /// <summary>
        /// 获取枚举值的别名, 优先从 summary 中获取, 其次从 EnumAliasAttribute 中获取
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetEnumAlias(Type enumType, object enumValue)
        {
            Dictionary<object, string> enumValueDict;
            if (!enumCache.TryGetValue(enumType, out enumValueDict))
            {
                lock (enumCache)
                {
                    if (!enumCache.TryGetValue(enumType, out enumValueDict))
                    {
                        var orginalType = enumType;
                        if (!enumType.IsEnum)
                        {
                            if (enumType.GetGenericTypeDefinition() != typeof(Nullable<>))
                            {
                                throw new ArgumentException("Only support enum and Nullable<enum>");
                            }
                            enumType = enumType.GetGenericArguments()[0];
                        }
                        enumValueDict = new Dictionary<object, string>();
                        var values = Enum.GetValues(enumType);
                        var contextualFieldInfos = enumType.GetContextualFields();
                        foreach (var value in values)
                        {
                            var contextualFieldInfo = contextualFieldInfos.FirstOrDefault(o => o.Name == value.ToString());
                            var aliasNameAttribute = enumType.GetField(value.ToString()).GetCustomAttribute<EnumAliasAttribute>();
                            enumValueDict[value] = FindFirstNotEmptyName(aliasNameAttribute?.AliasName, contextualFieldInfo?.GetXmlDocsSummary(), value.ToString());
                        }
                        enumCache[orginalType] = enumValueDict;
                    }
                }
            }
            if (enumValue == null)
            {
                return null;
            }
            if (!enumValueDict.TryGetValue(enumValue, out string result))
            {
                result = enumValue?.ToString();
            }
            return result;
        }

        /// <summary>
        /// 获取 class 的展示名称
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetDisplayName<T>()
        {
            return GetTypeInfo(typeof(T)).DispalyName;
        }

        /// <summary>
        /// 获取属性的展示名称
        /// </summary>
        /// <typeparam name="T">属于的class</typeparam>
        /// <param name="selector">字段或属性的选择器</param>
        /// <returns></returns>
        public static string GetDisplayName<T>(Expression<Func<T, object>> selector)
        {
            var typePropertyInfos = GetTypeInfo(typeof(T)).TypePropertyInfos;
            string memberName;
            if (selector.Body is UnaryExpression unaryExpression)
            {
                memberName = (unaryExpression.Operand as MemberExpression).Member.Name;
            }
            else if (selector.Body is MemberExpression memberExpression)
            {
                memberName = memberExpression.Member.Name;
            }
            else
            {
                throw new Exception($"{nameof(GetDisplayName)} not support");
            }
            var typePropertyInfo = typePropertyInfos.First(o => o.PropertyName == memberName);
            return typePropertyInfo.DispalyName;
        }


        public static string ToString(object obj, params string[] ignorePropertyNames)
        {
            return ToString<object>(obj, null, ignorePropertyNames);
        }

        public static string ToString<T>(object obj, T customPropertyValues, params string[] ignorePropertyNames)
            where T : class
        {
            var customPropertyValuesDictionary = GetCustomPropertyValues(customPropertyValues);
            return ToString(obj, customPropertyValuesDictionary, ignorePropertyNames);
        }

        public static string ToString(object obj, Dictionary<string, object> customPropertyValues, params string[] ignorePropertyNames)
        {
            return ToString(obj, ignorePropertyNames, customPropertyValues, 0);
        }

        public static string ToStringInclude(object obj, IEnumerable<string> includePropertyNames)
        {
            return ToStringInclude<object>(obj, includePropertyNames, null);
        }

        public static string ToStringInclude<T>(object obj, IEnumerable<string> includePropertyNames, T customPropertyValues)
            where T : class
        {
            var customPropertyValuesDictionary = GetCustomPropertyValues(customPropertyValues);
            return ToStringInclude(obj, includePropertyNames, customPropertyValuesDictionary);
        }

        public static string ToStringInclude(object obj, IEnumerable<string> includePropertyNames, Dictionary<string, object> customPropertyValues)
        {
            if (obj == null)
            {
                return ToJsonFormatString(null, null);
            }
            var typePropertyInfos = GetTypeInfo(obj.GetType()).TypePropertyInfos;
            var ignorePropertyNames = typePropertyInfos.Select(o => o.PropertyName).Except(includePropertyNames).ToArray();

            return ToString(obj, ignorePropertyNames, customPropertyValues, 0);
        }

        private static string ToString(object obj, IEnumerable<string> ignorePropertyNames, Dictionary<string, object> customPropertyValues, int currentDepth)
        {
            if (obj == null)
            {
                return ToJsonFormatString(null, null);
            }
            var typeInfo = GetTypeInfo(obj.GetType());

            if (typeInfo.JsonType == JsonType.BoolType || typeInfo.JsonType == JsonType.NumberType || typeInfo.JsonType == JsonType.WithQuotationType || typeInfo.JsonType == JsonType.EnumType)
            {
                return ToJsonFormatString(typeInfo, obj);
            }

            var sb = new StringBuilder(256); //估计256字能覆盖大多数情况了吧
            if (typeInfo.JsonType == JsonType.EnumerableType)
            {
                sb.Append("[");
                var enumerator = ((System.Collections.IEnumerable)obj).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    sb.Append(ToString(enumerator.Current, null, null, currentDepth + 1));
                    sb.Append(',');
                }
                StringBuilderTrimEnd(sb);
                sb.Append("]");
            }
            else
            {
                sb.Append('{');
                for (int i = 0; i < typeInfo.TypePropertyInfos.Length; i++)
                {
                    var typePropertyInfo = typeInfo.TypePropertyInfos[i];
                    if (ignorePropertyNames?.Contains(typePropertyInfo.PropertyName) == true)
                    {
                        continue;
                    }
                    string valueString = GetPropertyStringValue(obj, typePropertyInfo, customPropertyValues, currentDepth);
                    sb.AppendFormat("\"{0}\"{1}{2},", typePropertyInfo.DispalyName, KeyValueSeparator, valueString);
                }
                StringBuilderTrimEnd(sb);
                sb.Append('}');
            }
            return sb.ToString();
        }

        private static string GetPropertyStringValue(object obj, TypePropertyInfo typePropertyInfo, Dictionary<string, object> customPropertyValues, int currentDepth)
        {
            object value;
            if (currentDepth == 0 && customPropertyValues != null && customPropertyValues.ContainsKey(typePropertyInfo.PropertyName))
            {
                value = customPropertyValues[typePropertyInfo.PropertyName];
            }
            else
            {
                value = typePropertyInfo.PropertyInfo.GetValue(obj);
            }
            value = GetReplaceValue(value, typePropertyInfo.ReplacePairs);
            string stringValue = ToString(value, null, null, currentDepth + 1);
            return stringValue;
        }

        private static Dictionary<string, object> GetCustomPropertyValues<T>(T customOjbect)
        {
            if (customOjbect == null)
            {
                return null;
            }
            var customTypeInfo = GetTypeInfo(customOjbect.GetType());
            if (customTypeInfo.TypePropertyInfos.Length == 0)
            {
                return null;
            }
            Dictionary<string, object> customPropertyValuesDictionary = new Dictionary<string, object>(customTypeInfo.TypePropertyInfos.Length);
            foreach (var typePropertyInfo in customTypeInfo.TypePropertyInfos)
            {
                customPropertyValuesDictionary[typePropertyInfo.PropertyName] = typePropertyInfo.PropertyInfo.GetValue(customOjbect);
            }
            return customPropertyValuesDictionary;
        }

        private static object GetReplaceValue(object obj, ReplacePair[] replacePairs)
        {
            if (replacePairs != null)
            {
                var matchReplacePair = replacePairs.FirstOrDefault(o => object.Equals(o.Orginal, obj));
                if (matchReplacePair != null)
                {
                    obj = matchReplacePair.Replace;
                }
            }
            return obj;
        }

        private static string ToJsonFormatString(TypeInfo typeInfo, object obj)
        {
            if (obj == null)
            {
                return "null";
            }
            else if (typeInfo.JsonType == JsonType.BoolType)
            {
                return (bool)obj ? "true" : "false";
            }
            else if (typeInfo.JsonType == JsonType.NumberType)
            {
                return obj.ToString();
            }
            else if (typeInfo.JsonType == JsonType.WithQuotationType)
            {
                return $"\"{obj.ToString().Replace('\"', '\'')}\"";
            }
            else if (typeInfo.JsonType == JsonType.EnumType)
            {
                return $"\"{GetEnumAlias(typeInfo.Type, obj)}\"";
            }
            else
            {
                throw new Exception("not suppoert");
            }
        }




        public static CompareResult Compare<T>(T fromObject, T toObject, IEnumerable<string> ignorePropertyNames = null)
        {
            if (fromObject == null || toObject == null)
            {
                throw new ArgumentNullException("fromObject and toObject can't be null");
            }

            var compareResult = new CompareResult();

            var typePropertyInfos = GetTypeInfo(typeof(T)).TypePropertyInfos;
            for (int i = 0; i < typePropertyInfos.Length; i++)
            {
                ref var typePropertyInfo = ref typePropertyInfos[i];
                if (ignorePropertyNames?.Contains(typePropertyInfo.PropertyName) == true)
                {
                    continue;
                }
                var stringOfFrom = GetPropertyStringValue(fromObject, typePropertyInfo, null, 0);
                var stringOfTo = GetPropertyStringValue(toObject, typePropertyInfo, null, 0);

                if (stringOfFrom != stringOfTo)
                {
                    compareResult.DifferentProperties.Add(new DifferentProperty(typePropertyInfo.PropertyName, typePropertyInfo.DispalyName, stringOfFrom, stringOfTo));
                }
            }
            return compareResult;
        }

        public static CompareResult CompareInclude<T>(T fromObject, T toObject, IEnumerable<string> includePropertyNames)
        {
            var typePropertyInfos = GetTypeInfo(typeof(T)).TypePropertyInfos;
            var ignoreProperties = typePropertyInfos.Select(o => o.PropertyName).Except(includePropertyNames).ToArray();
            return Compare(fromObject, toObject, ignoreProperties);
        }


        public static void StringBuilderTrimEnd(StringBuilder sb, char trimChar = ',')
        {
            if (sb != null && sb.Length > 0)
            {
                var sbLength = sb.Length;
                for (int i = sbLength - 1; i >= 0; i--)
                {
                    if (sb[i] != trimChar)
                    {
                        break;
                    }
                    sb.Remove(sb.Length - 1, 1);
                }
            }
        }
    }


    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class EnumAliasAttribute : Attribute
    {
        public string AliasName { get; }

        public EnumAliasAttribute(string aliasName)
        {
            AliasName = aliasName;
        }
    }


    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class ToStringReplacePairAttribute : Attribute
    {
        public ReplacePair[] ReplacePairs { get; }

        public ToStringReplacePairAttribute(object orginal, object replace)
        {
            ReplacePairs = new ReplacePair[] { new ReplacePair(orginal, replace) };
        }

        public ToStringReplacePairAttribute(object orginal1, object replace1, object orginal2, object replace2)
        {
            ReplacePairs = new ReplacePair[] { new ReplacePair(orginal1, replace1), new ReplacePair(orginal2, replace2) };
        }

        public ToStringReplacePairAttribute(object orginal1, object replace1, object orginal2, object replace2, object orginal3, object replace3)
        {
            ReplacePairs = new ReplacePair[] { new ReplacePair(orginal1, replace1), new ReplacePair(orginal2, replace2), new ReplacePair(orginal3, replace3) };
        }

        public ToStringReplacePairAttribute(object orginal1, object replace1, object orginal2, object replace2, object orginal3, object replace3, object orginal4, object replace4)
        {
            ReplacePairs = new ReplacePair[] { new ReplacePair(orginal1, replace1), new ReplacePair(orginal2, replace2), new ReplacePair(orginal3, replace3), new ReplacePair(orginal4, replace4) };
        }
    }


    public class ReplacePair
    {
        public ReplacePair(object orginal, object replace)
        {
            Orginal = orginal;
            Replace = replace;
        }

        public object Orginal { get; }

        public object Replace { get; }
    }


    public class CompareResult
    {
        internal CompareResult()
        { }

        public bool HasDifference { get { return DifferentProperties.Count > 0; } }

        public List<DifferentProperty> DifferentProperties { get; } = new List<DifferentProperty>();

        public bool IsPropertyDifferent(string propertyName)
        {
            return DifferentProperties.Any(o => o.PropertyName == propertyName);
        }

        public bool UpdateDifferentProperty(string propertyName, object newFromValue, object newToValue)
        {
            var differentProperty = DifferentProperties.FirstOrDefault(o => o.PropertyName == propertyName);
            if (differentProperty == null)
            {
                return false;
            }

            // 如果传入的两个值是相等的, 则不更新
            var from = LocalizationTools.ToString(newFromValue);
            var to = LocalizationTools.ToString(newToValue);
            if (from == to)
            {
                return false;
            }

            differentProperty.From = from;
            differentProperty.To = to;
            return true;
        }

        public void AddNewDifferentProperty(string dispalyName, object newFromValue, object newToValue, string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(dispalyName))
            {
                throw new ArgumentException($"{ nameof(dispalyName)} can't be empty", nameof(dispalyName));
            }
            dispalyName = dispalyName.Trim().Replace('"', ',');
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                propertyName = dispalyName;
            }
            else
            {
                propertyName = propertyName.Trim();
            }

            if (DifferentProperties.Any(o => o.PropertyName == propertyName))
            {
                throw new ArgumentException($"DifferentProperties has exist {propertyName}, if you need update, please use UpdateDifferentProperty() method", nameof(propertyName));
            }

            var from = LocalizationTools.ToString(newFromValue);
            var to = LocalizationTools.ToString(newToValue);
            if (from == to)
            {
                throw new ArgumentException($"newFromValue is same to newToValue");
            }

            var differentProperty = new DifferentProperty(propertyName, dispalyName, from, to);
            DifferentProperties.Add(differentProperty);
        }

        public string GetDifferenceMsg(IEnumerable<string> ignorePropertyNames = null)
        {
            var sb = new StringBuilder(256);
            sb.Append('{');
            foreach (var differentProperty in DifferentProperties)
            {
                if (ignorePropertyNames?.Contains(differentProperty.PropertyName) == true)
                {
                    continue;
                }
                sb.Append($"\"{differentProperty.DispalyName}\":{{");
                sb.Append($"\"从\":{differentProperty.From ?? "null"},");
                sb.Append($"\"变成\":{differentProperty.To ?? "null"}}},");
            }
            LocalizationTools.StringBuilderTrimEnd(sb);
            sb.Append('}');
            return sb.ToString();
        }

        public override string ToString()
        {
            return GetDifferenceMsg();
        }
    }


    public class DifferentProperty
    {
        public DifferentProperty(string propertyName, string dispalyName, string from, string to)
        {
            PropertyName = propertyName;
            DispalyName = dispalyName;
            From = from;
            To = to;
        }

        public string PropertyName { get; internal set; }

        public string DispalyName { get; internal set; }

        public string From { get; internal set; }

        public string To { get; internal set; }
    }
}

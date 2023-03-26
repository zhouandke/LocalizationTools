using Namotion.Reflection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Localization2
{
    public class PropertyToStringBuilder : LocalizationBuilderBase
    {
        public PropertyToStringBuilder(ILts lts) : base(lts)
        { }

        public override double MatchOrder => 400;

        public override bool IsMatch(Type type)
        {
            return true;
        }

        public override void SetLocalizationer(TypeInfo typeInfo)
        {
            var contextualPropertyInfos = typeInfo.Type.GetContextualProperties();
            var typePropertyInfoList = new List<TypePropertyInfo>();
            foreach (var propertyInfo in typeInfo.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty))
            {
                if (propertyInfo.GetCustomAttribute<LtsIgnoreAttribute>(true) != null)
                {
                    continue;
                }

                var displayNameAttribute = propertyInfo.GetCustomAttribute<DisplayNameAttribute>();
                var contextualPropertyInfo = contextualPropertyInfos.FirstOrDefault(o => o.Name == propertyInfo.Name);
                var displayName = Help.FindFirstNotEmptyName(displayNameAttribute?.DisplayName, contextualPropertyInfo?.GetXmlDocsSummary(), propertyInfo.Name);

                var replacePairs = propertyInfo.GetCustomAttributes<LtsReplaceAttribute>(true).SelectMany(o => o.ReplacePairs).ToArray();

                var typePropertyInfo = new TypePropertyInfo
                {
                    PropertyInfo = propertyInfo,
                    DisplayName = displayName,
                    ReplacePairs = replacePairs
                };

                ILocalizationToString customlocalization = null;
                var customLocalizationOnProperty = propertyInfo.GetCustomAttribute<LtsCustomLocalizationAttribute>();
                if (customLocalizationOnProperty != null)
                {
                    var errorMsg = Help.CheckCustomLocalizationType(customLocalizationOnProperty.CustomLocalizationType);
                    if (errorMsg != null)
                    {
                        throw new Exception(errorMsg);
                    }
                    customlocalization = (ILocalizationToString)customLocalizationOnProperty.CustomLocalizationType.GetConstructor(new Type[0]).Invoke(new Type[0]);
                }
                if (customlocalization != null)
                {
                    typePropertyInfo.Localizationer = new Lazy<ILocalizationToString>(() => customlocalization);
                }
                else
                {
                    typePropertyInfo.Localizationer = new Lazy<ILocalizationToString>(() => lts.GetTypeInfo(propertyInfo.PropertyType).Localizationer);
                }

                typePropertyInfoList.Add(typePropertyInfo);
            }

            typeInfo.TypePropertyInfos = typePropertyInfoList.ToArray();
            var propertyToString = new PropertyToString();
            propertyToString.SetPropertyInfos(typeInfo.TypePropertyInfos);
            typeInfo.Localizationer = propertyToString;
        }
    }


    public class PropertyToString : ILocalizationToString
    {
        public TypePropertyInfo[] TypePropertyInfos { get; private set; }

        public void SetPropertyInfos(IEnumerable<TypePropertyInfo> typePropertyInfos)
        {
            TypePropertyInfos = typePropertyInfos?.ToArray() ?? Array.Empty<TypePropertyInfo>();
        }

        public string ToLocalization(object orginalValue, LocalizationStringContext context, string pathForReplaceValue, ReplacePair[] replacePairs, string pathForIgnore)
        {
            var replacePair = replacePairs?.FirstOrDefault(o => object.Equals(o.Orginal, orginalValue));
            if (replacePair != null)
            {
                return Help.FormatStringValue(replacePair.Replace);
            }

            if (orginalValue == null)
            {
                return null;
            }

            var kvpList = new List<KeyValuePair<string, string>>(TypePropertyInfos.Length);
            foreach (var typePropertyInfo in TypePropertyInfos)
            {
                var ignorePath = Help.Combine(pathForIgnore, typePropertyInfo.PropertyInfo.Name);
                if (context.IgnorePaths?.Contains(ignorePath) == true)
                {
                    continue;
                }

                var propertyPath = Help.Combine(pathForReplaceValue, typePropertyInfo.PropertyInfo.Name);
                string stringValue = null;
                if (context.CustomPropertyValues?.TryGetValue(propertyPath, out stringValue) == true)
                {
                    if (stringValue == null && context.IgnoreNullProperty)
                    {
                        continue;
                    }
                    // 如果是 null, 要输出字符串 "null"; ReplacePair.Replace 没有包含双引号
                    stringValue = (stringValue == null) ? "null" : $"\"{stringValue}\"";
                    kvpList.Add(new KeyValuePair<string, string>(typePropertyInfo.DisplayName, stringValue));
                    continue;
                }

                stringValue = typePropertyInfo.Localizationer.Value.ToLocalization(typePropertyInfo.PropertyInfo.GetValue(orginalValue), context, propertyPath, typePropertyInfo.ReplacePairs, ignorePath);
                if (stringValue == null && context.IgnoreNullProperty)
                {
                    continue;
                }

                // 如果是 null, 要输出字符串 "null"; ToLocalization 返回的结果已经包含双引号了
                stringValue = (stringValue == null) ? "null" : stringValue;
                kvpList.Add(new KeyValuePair<string, string>(typePropertyInfo.DisplayName, stringValue));
            }
            var str = string.Join(",", kvpList.Select(o => $"\"{o.Key}\": {o.Value}"));
            return "{" + str + "}";
        }
    }
}

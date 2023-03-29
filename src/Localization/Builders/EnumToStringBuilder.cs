using Namotion.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Localization2
{
    public class EnumToStringBuilder : LocalizationBuilderBase
    {
        private readonly Dictionary<Type, Dictionary<object, string>> enumCache = new Dictionary<Type, Dictionary<object, string>>();

        public EnumToStringBuilder(ILts lts) : base(lts)
        { }

        public override double MatchOrder => 200;

        public override bool IsMatch(Type type)
        {
            return type.IsEnum || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable) && type.GetGenericArguments()[0].IsEnum);
        }

        public override void SetLocalizationer(TypeInfo typeInfo)
        {
            var enumType = typeInfo.Type.IsEnum ? typeInfo.Type : typeInfo.Type.GetGenericArguments()[0];
            var enumValueMap = GetEnumValueMap(enumType);
            var enumToString = new EnumToString();
            enumToString.SetEnumValueMap(enumValueMap);
            typeInfo.Localizationer = enumToString;
        }

        private Dictionary<object, string> GetEnumValueMap(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new Exception($"{enumType.Name} is not a enum");
            }

            if (!enumCache.TryGetValue(enumType, out var enumValueMap))
            {
                lock (enumCache)
                {
                    if (!enumCache.TryGetValue(enumType, out enumValueMap))
                    {
                        enumValueMap = new Dictionary<object, string>();
                        var values = Enum.GetValues(enumType);
                        var contextualFieldInfos = enumType.GetContextualFields();
                        foreach (var value in values)
                        {
                            var aliasAttribute = enumType.GetField(value.ToString()).GetCustomAttribute<LtsEnumAliasAttribute>();
                            var contextualFieldInfo = contextualFieldInfos.FirstOrDefault(o => o.Name == value.ToString());
                            enumValueMap[value] = Help.FindFirstNotEmptyName(aliasAttribute?.Alias, contextualFieldInfo?.GetXmlDocsSummary(), value.ToString());
                        }
                    }
                    enumCache[enumType] = enumValueMap;
                }
            }
            return enumValueMap;
        }
    }


    public class EnumToString : ILocalizationToString
    {
        public IReadOnlyDictionary<object, string> EnumValues { get; private set; }

        public void SetEnumValueMap(IReadOnlyDictionary<object, string> enumValues)
        {
            EnumValues = enumValues;
        }

        public string Localization(object orginalValue, LocalizationStringContext context, string pathForReplaceValue, ReplacePair[] replacePairs, string pathForIgnore)
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

            if (EnumValues.TryGetValue(orginalValue, out var stringValue))
            {
                return Help.FormatStringValue(stringValue);
            }

            return Help.FormatStringValue(orginalValue.ToString());
        }
    }
}

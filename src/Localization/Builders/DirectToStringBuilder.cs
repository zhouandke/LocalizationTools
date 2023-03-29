using System;
using System.Collections.Generic;
using System.Linq;

namespace Localization2
{
    public class DirectToStringBuilder : LocalizationBuilderBase
    {
        private readonly HashSet<Type> directToStringTypes = new HashSet<Type>()
        {
            typeof(object),

            typeof(IntPtr),
            typeof(UIntPtr),
            typeof(IntPtr?),
            typeof(UIntPtr?),

            typeof(sbyte),
            typeof(byte),
            typeof(Int16),
            typeof(UInt16),
            typeof(Int32),
            typeof(UInt32),
            typeof(Int64),
            typeof(UInt64),
            typeof(char),
            typeof(sbyte?),
            typeof(byte?),
            typeof(Int16?),
            typeof(UInt16?),
            typeof(Int32?),
            typeof(UInt32?),
            typeof(Int64?),
            typeof(UInt64?),
            typeof(char?),

            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(float?),
            typeof(double?),
            typeof(decimal?),

            typeof(bool),
            typeof(bool?),

            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(DateTime?),
            typeof(DateTimeOffset?),
            typeof(TimeSpan?),

            typeof(string),
        };

        public DirectToStringBuilder(ILts lts) : base(lts)
        { }

        public void AddDirectToStringType(Type type)
        {
            directToStringTypes.Add(type);
        }

        public override double MatchOrder => 100;

        public override bool IsMatch(Type type)
        {
            return directToStringTypes.Contains(type);
        }

        public override void SetLocalizationer(TypeInfo typeInfo)
        {
            typeInfo.Localizationer = new DirectToString();
        }
    }


    public class DirectToString : ILocalizationToString
    {
        public string Localization(object orginalValue, LocalizationStringContext context, string pathForReplaceValue, ReplacePair[] replacePairs, string pathForIgnore)
        {
            string stringValue;
            var replacePair = replacePairs?.FirstOrDefault(o => object.Equals(o.Orginal, orginalValue));
            if (replacePair != null)
            {
                stringValue = replacePair.Replace;
            }
            else
            {
                stringValue = orginalValue?.ToString();
            }

            return Help.FormatStringValue(stringValue);
        }
    }
}

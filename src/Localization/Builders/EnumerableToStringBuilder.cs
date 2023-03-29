using System;
using System.Collections.Generic;

namespace Localization2
{
    public class EnumerableToStringBuilder : LocalizationBuilderBase
    {
        public EnumerableToStringBuilder(ILts lts) : base(lts)
        {}

        public override double MatchOrder => 300;

        public override bool IsMatch(Type type)
        {
            return typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string);
        }

        public override void SetLocalizationer(TypeInfo typeInfo)
        {
            typeInfo.Localizationer = new EnumerableToString();
        }
    }


    public class EnumerableToString : ILocalizationToString
    {
        public string Localization(object orginalValue, LocalizationStringContext context, string pathForReplaceValue, ReplacePair[] replacePairs, string pathForIgnore)
        {
            var enumerable = orginalValue as System.Collections.IEnumerable;
            if (enumerable == null)
            {
                return null;
            }

            var stringList = new List<string>();
            var index = 0;
            foreach (var item in enumerable)
            {
                if (item != null)
                {
                    var typeInfo = context.Root.GetTypeInfo(item.GetType());
                    stringList.Add(typeInfo.Localizationer.Localization(item, context, $"{pathForReplaceValue}[{index}]", replacePairs, pathForIgnore));
                }
                else
                {
                    stringList.Add(null);
                }
                index++;
            }

            return $"[{string.Join(",", stringList)}]";
        }
    }
}

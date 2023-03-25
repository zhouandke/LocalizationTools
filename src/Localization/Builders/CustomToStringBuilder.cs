using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Localization2
{
    public class CustomToStringBuilder : LocalizationBuilderBase
    {
        public CustomToStringBuilder(ILts lts) : base(lts)
        { }

        public override double MatchOrder => 0;

        public override bool IsMatch(Type type)
        {
            return type.GetCustomAttribute<LtsCustomLocalizationAttribute>() != null;
        }

        public override void SetLocalizationer(TypeInfo typeInfo)
        {
            var customLocalization = typeInfo.Type.GetCustomAttribute<LtsCustomLocalizationAttribute>();
            var errorMsg = Help.CheckCustomLocalizationType(customLocalization.CustomLocalizationType);
            if (errorMsg != null)
            {
                throw new Exception(errorMsg);
            }
            typeInfo.Localizationer = (ILocalizationToString)customLocalization.CustomLocalizationType.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<Type>());
        }
    }
}

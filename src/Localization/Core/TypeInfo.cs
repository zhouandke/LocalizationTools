using System;

namespace Localization2
{
    public class TypeInfo
    {
        public Type Type { get; set; }

        public ILocalizationToString Localizationer { get; set; }

        public TypePropertyInfo[] TypePropertyInfos { get; set; }
    }
}

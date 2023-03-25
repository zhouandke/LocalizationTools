using System;
using System.Reflection;

namespace Localization2
{
    public class TypePropertyInfo
    {
        private ReplacePair[] replacePairs;

        public PropertyInfo PropertyInfo { get; internal set; }

        public string DisplayName { get; internal set; }

        public ReplacePair[] ReplacePairs
        {
            get { return replacePairs; }
            internal set { replacePairs = value?.Length == 0 ? null : value; }
        }

        public Lazy<ILocalizationToString> Localizationer { get; internal set; }
    }
}

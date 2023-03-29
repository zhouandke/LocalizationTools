using System;
using System.Reflection;

namespace Localization2
{
    public class TypePropertyInfo
    {
        private ReplacePair[] replacePairs;

        public PropertyInfo PropertyInfo { get; set; }

        public string DisplayName { get; set; }

        public ReplacePair[] ReplacePairs
        {
            get { return replacePairs; }
            set { replacePairs = value?.Length == 0 ? null : value; }
        }

        public Lazy<ILocalizationToString> Localizationer { get; set; }
    }
}

using System.Collections.Generic;

namespace Localization2
{
    public class LocalizationStringContext
    {
        public LocalizationStringContext(object state, bool ignoreNullProperty, Dictionary<string, string> customPropertyValues = null, string[] ignorePathes = null)
        {
            State = state;
            IgnoreNullProperty = ignoreNullProperty;
            CustomPropertyValues = customPropertyValues;
            IgnorePathes = ignorePathes?.Length == 0 ? null : ignorePathes;
        }

        public ILts Root { get; internal set; }

        public object State { get; internal set; }

        public bool IgnoreNullProperty { get; }

        public Dictionary<string, string> CustomPropertyValues { get; }

        public string[] IgnorePathes { get; }
    }
}

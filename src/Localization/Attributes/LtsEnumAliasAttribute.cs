using System;

namespace Localization2
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class LtsEnumAliasAttribute : Attribute
    {
        public string Alias { get; }

        public LtsEnumAliasAttribute(string alias)
        {
            Alias = alias;
        }
    }
}

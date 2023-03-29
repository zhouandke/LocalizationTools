using System;

namespace Localization2
{
    /// <summary>
    /// DisplayNameAttribute 不能用于字段, 所以单独定义了一个 Attribute
    /// </summary>
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

using System;

namespace Localization2
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class LtsCustomLocalizationAttribute : Attribute
    {
        public Type CustomLocalizationType { get; }

        public LtsCustomLocalizationAttribute(Type customLocalization)
        {
            CustomLocalizationType = customLocalization;
        }
    }
}

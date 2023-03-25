using System;

namespace Localization2
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class LtsIgnoreAttribute : Attribute
    { }
}

using System;

namespace Localization2
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class LtsReplaceAttribute : Attribute
    {
        public ReplacePair[] ReplacePairs { get; set; }

        public LtsReplaceAttribute(object orginal, string replace)
        {
            ReplacePairs = new[] { new ReplacePair(orginal, replace) };
        }

        public LtsReplaceAttribute(object orginal0, string replace0, object orginal1, string replace1)
        {
            ReplacePairs = new[] { new ReplacePair(orginal0, replace0), new ReplacePair(orginal1, replace1) };
        }

        public LtsReplaceAttribute(object orginal0, string replace0, object orginal1, string replace1, object orginal2, string replace2)
        {
            ReplacePairs = new[] { new ReplacePair(orginal0, replace0), new ReplacePair(orginal1, replace1), new ReplacePair(orginal2, replace2) };
        }

        public LtsReplaceAttribute(object orginal0, string replace0, object orginal1, string replace1, object orginal2, string replace2, object orginal3, string replace3)
        {
            ReplacePairs = new[] { new ReplacePair(orginal0, replace0), new ReplacePair(orginal1, replace1), new ReplacePair(orginal2, replace2), new ReplacePair(orginal3, replace3) };
        }
    }
}

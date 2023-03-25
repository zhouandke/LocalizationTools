using System;
using System.Linq;

namespace Localization2
{
    public static class Help
    {
        public static string CheckCustomLocalizationType(Type customLocalizationType)
        {
            if (!customLocalizationType.GetInterfaces().Contains(typeof(ILocalizationToString)))
            {
                return $"{customLocalizationType.Name} must be ILocalizationToString";
            }

            if (customLocalizationType.GetConstructor(new Type[0]) == null)
            {
                return $"{customLocalizationType.Name} must has a parameterless Constructor";
            }

            return null;
        }

        public static string FormatStringValue(string stringValue)
        {
            return stringValue == null ? null : $"\"{stringValue}\"";
        }

        public static string Combine(string path, string subPathName)
        {
            return string.IsNullOrWhiteSpace(path) ? subPathName : $"{path}.{subPathName}";
        }

        public static string FindFirstNotEmptyName(params string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                var name = names[i]?.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    return name;
                }
            }

            return "";
        }
    }
}

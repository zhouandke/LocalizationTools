using System;

namespace Localization2
{
    public interface ILts
    {
        TypeInfo GetTypeInfo(Type type);

        string Localization(object obj, LocalizationStringContext context);
    }
}

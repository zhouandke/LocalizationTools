namespace Localization2
{
    public interface ILocalizationToString
    {
        string ToLocalization(object orginalValue, LocalizationStringContext context, string pathForReplaceValue, ReplacePair[] replacePairs, string pathForIgnore);
    }
}

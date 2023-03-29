namespace Localization2
{
    public interface ILocalizationToString
    {
        /// <summary>
        /// 必须提供无参构造函数
        /// </summary>
        /// <param name="orginalValue"></param>
        /// <param name="context"></param>
        /// <param name="pathForReplaceValue">用于替换值的属性路径, 比如 Spouse.CityId; 数组会加下标, 比如 Children[0].CityId </param>
        /// <param name="replacePairs">在属性上添加的 LtsReplaceAttribute, 一般情况优先匹配这个; 当然你自己写自定义的实现, 你也可以忽略这些</param>
        /// <param name="pathForIgnore">用于忽略值的属性路径, 比如 Children.CityId, 数组不加下标</param>
        /// <returns>可以返回 null; 数组返回 [] 形式; 其他的返回值都用双引号 "" 包裹</returns>
        string Localization(object orginalValue, LocalizationStringContext context, string pathForReplaceValue, ReplacePair[] replacePairs, string pathForIgnore);
    }
}

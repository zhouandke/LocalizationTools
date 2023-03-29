using System.Collections.Generic;

namespace Localization2
{
    public class LocalizationStringContext
    {
        public LocalizationStringContext(object state, bool ignoreNullProperty, Dictionary<string, string> customPropertyValues = null, string[] ignorePaths = null)
        {
            State = state;
            IgnoreNullProperty = ignoreNullProperty;
            CustomPropertyValues = customPropertyValues;
            IgnorePaths = ignorePaths?.Length == 0 ? null : ignorePaths;
        }

        public ILts Root { get; internal set; }

        /// <summary>
        /// 用户传入的任何对象, 通常是 服务容器、ORM;
        /// 可能为 null
        /// </summary>
        public object State { get; internal set; }

        /// <summary>
        /// 是否忽略 null 的属性
        /// </summary>
        public bool IgnoreNullProperty { get; }

        /// <summary>
        /// 用户传入的自定义值, Key 是属性路径(比如 Children[0].CityId ), value 是结果字符串;
        /// 可能为 null
        /// </summary>
        public Dictionary<string, string> CustomPropertyValues { get; }

        /// <summary>
        /// 需要忽略的属性路径, 比如 Children.CityId, 数组不需要加下标 [0];
        /// 可能为 null
        /// </summary>
        public string[] IgnorePaths { get; }
    }
}

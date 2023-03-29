# LocalizationTools
Localization 使用中文注释来代替 属性名称, 生成Json, 并提供一个简单的比较工具

LocalizationTools 第一个版本写得比较匆忙, 当时很多东西没想清楚, 所以对一些复杂对象支持不好。
这个工具已被一些朋友使用, 但一直没人反馈需要改进的地方, 所以这次我就以所在项目的需求对这个工具进行了升级, 总体思路:
1. 生成的字符串更加接近 Json, 如果对象很多属性的话, 格式化后的 Json 更直观;
2. 更好的支持扩展;
3. 新旧两个对象比较, 参考 JsonPath 的思路, 数组可以进行更精细化的比较;

**V1版本没有被删除**, 用户可以继续使用;  
现在的V2版本, 由于思路变化有些变化, 出现了 **break change**, 所以使用了新的命名空间 Localization2;

已使用了V1版本的项目, 并且模型比较简单, 可以继续使用V1版本([V1版本说明](README.V1.md)); 其他情况都建议使用V2版本.

####特性介绍:
1. 简单的值替换, 可以使用 LtsReplaceAttribute;
2. 可以使用 LtsCustomLocalizationAttribute 为类型或属性指定 简单的自定义转换工具, 所谓的简单是指: 不依赖容器或数据库等;
3. 可以使用 SetCustomLocalization() 方法为类型或属性指定 自定义转换工具, 同时将 容器或ORM传入 DefaultState, 可以查询数据库来替换显示值, 参考示例中的 PersonCityLocalization;
4. 可以加入自定义的 LocalizationBuilder 为多个类型指定 自定义转换工具.

<br/>
使用示例, 请务必在 Visual Studio 的项目编辑界面 --> 生成 --> 输出, 勾选 XML文档文件

``` csharp
using Localization2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SampleConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var p1 = new Person
            {
                Id = 1,
                Name = "王大锤",
                Birthday = DateTime.Parse("2020-01-01"),
                CityId = 10,
                Sex = SexType.Man,
                Password = "12345657689",
                IsAvailable = true,
                Hobby = new List<string> { "吃饭", "拍视频" },
                Spouse = new Person() { Id = 2, CityId = 21, Name = "老婆", Sex = SexType.Woman },
                Children = new List<Person>()
                {
                    new Person { Id=10, Name="王震天", CityId=10, Sex= SexType.Man },
                    new Person { Id=11, Name="王晴雨", CityId=10, Sex= SexType.Woman }
                }
            };

            Console.WriteLine("********** Localization(): 简单 **********");
            var str = Lts.Default.Localization(p1);
            Console.WriteLine(str);
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("********** Localization(): 复杂 **********");
            // 为 Person.Birthday 设置自定义转换程序
            Lts.Default.SetCustomLocalization<Person, PersonCityLocalization>(p => p.CityId);
            var customPropertyValues = new Dictionary<string, string>
            {
                { "Password",  "******"},
                { "Spouse.Password",  "******"},
                { "Children[0].Password",  "******"},  // 数组使用下标指定某个元素
                { "Children[1].Password",  "******"},
            };
            var ignorePaths = new[]
            {
                "Birthday",
                "Spouse.Birthday",
                "Children.Birthday"  // 忽略数组元素的某个属性, 不需要加下标 [0]
            };
            str = Lts.Default.Localization(p1, null, customPropertyValues, ignorePaths);
            Console.WriteLine(str);
            Console.WriteLine();
            Console.WriteLine();




            Console.WriteLine("********* Compare(string fromJson, string toJson): 忽略项在 Localization() 就已经忽略了 *********");
            var p2 = JsonConvert.DeserializeObject<Person>(JsonConvert.SerializeObject(p1));
            p2.Name = "王小锤";
            p2.CityId = 21;
            p2.Password = "1111111";
            p2.Spouse.Password = "1111111";
            p2.Children[1].Password = "111111";
            p2.Children[1].Name = "王下雨";
            var str2 = Lts.Default.Localization(p2, null, customPropertyValues, ignorePaths);
            var diffItems = new JsonCompare(ArrayDiffMode.Entire).Compare(str, str2);
            var compareResultJson = JsonConvert.SerializeObject(diffItems);
            Console.WriteLine(compareResultJson);
            Console.WriteLine();
            Console.WriteLine();


            Console.WriteLine("********* Compare<T>(T from, T to, params string[] ignorePaths): 带有忽略项 *********");
            ignorePaths = new[]
            {
                "Birthday",
                "Spouse.Birthday",
                "Children.Birthday",
                "Password",
                "Spouse.Password",
                "Children.Password",
            };
            diffItems = new JsonCompare(ArrayDiffMode.EvertyItem).Compare(p1, p2, ignorePaths);
            // 由于不能传入 customPropertyValues, 只能手动修改差异的值 
            //var diffItem = diffItems.FirstOrDefault(o => o.Path == "所在城市");
            //if (diffItem != null)
            //{
            //    ((Newtonsoft.Json.Linq.JValue)diffItem.From).Value = "北京";
            //    ((Newtonsoft.Json.Linq.JValue)diffItem.To).Value = "上海";
            //}
            compareResultJson = JsonConvert.SerializeObject(diffItems);
            Console.WriteLine(compareResultJson);
        }
    }

    /// <summary>
    /// 人类
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [DisplayName("名字")]
        public string Name { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime Birthday { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public SexType Sex { get; set; }

        /// <summary>
        /// 所在城市
        /// </summary>
        public int CityId { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [LtsReplace(null, "未设置", true, "是", false, "否")]
        public bool? IsAvailable { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 爱好
        /// </summary>
        public List<string> Hobby { get; set; } = new List<string>();

        /// <summary>
        /// 配偶
        /// </summary>
        [LtsReplace(null, "没有配偶")]
        public Person Spouse { get; set; }

        /// <summary>
        /// 子女
        /// </summary>
        public List<Person> Children { get; set; } = new List<Person>();
    }

    /// <summary>
    /// 性别
    /// </summary>
    public enum SexType
    {
        Unknown = 0,

        [LtsEnumAlias("男性")]
        Man = 1,
        /// <summary>
        /// 女性
        /// </summary>
        Woman = 2,
        /// <summary>
        /// 人妖
        /// </summary>
        //[EnumAlias("人妖")]
        Ladyman = 3,
    }

    /// <summary>
    /// 模拟从数据库读出 City Name
    /// </summary>
    public class PersonCityLocalization : ILocalizationToString
    {
        public string Localization(object orginalValue, LocalizationStringContext context, string pathForReplaceValue, ReplacePair[] replacePairs, string pathForIgnore)
        {
            // 忽略 replacePairs, 全部从数据库读取

            var orm = context.State; // 假设传入state 是 orm
            string stringValue;
            if (object.Equals(orginalValue, 10))
            {
                stringValue = "北京";
            }
            else if (object.Equals(orginalValue, 21))
            {
                stringValue = "上海";
            }
            else
            {
                stringValue = $"未知城市Id={orginalValue}";
            }
            return Help.FormatStringValue(stringValue);
        }
    }
}

```
<br/>
格式化后的结果
``` csharp
{
  "Id": "1",
  "名字": "王大锤",
  "性别": "男性",
  "所在城市": "北京",
  "是否启用": "是",
  "密码": "******",
  "爱好": [ "吃饭", "拍视频" ],
  "配偶": {
    "Id": "2",
    "名字": "老婆",
    "性别": "女性",
    "所在城市": "上海",
    "是否启用": "未设置",
    "密码": "******",
    "爱好": [],
    "配偶": "没有配偶",
    "子女": []
  },
  "子女": [
    {
      "Id": "10",
      "名字": "王震天",
      "性别": "男性",
      "所在城市": "北京",
      "是否启用": "未设置",
      "密码": "******",
      "爱好": [],
      "配偶": "没有配偶",
      "子女": []
    },
    {
      "Id": "11",
      "名字": "王晴雨",
      "性别": "女性",
      "所在城市": "北京",
      "是否启用": "未设置",
      "密码": "******",
      "爱好": [],
      "配偶": "没有配偶",
      "子女": []
    }
  ]
}
```
<br/>
格式化后的比较结果
``` js
[
  {
    "属性": "名字",
    "操作": "修改",
    "原值": "王大锤",
    "新值": "王小锤"
  },
  {
    "属性": "所在城市",
    "操作": "修改",
    "原值": "北京",
    "新值": "上海"
  },
  {
    "属性": "子女[1].名字",
    "操作": "修改",
    "原值": "王晴雨",
    "新值": "王下雨"
  }
]
```

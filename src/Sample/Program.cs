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

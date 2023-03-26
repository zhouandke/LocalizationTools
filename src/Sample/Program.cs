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
                Spouse = new Person() { Id = 2, CityId = 21, Name = "老婆", Sex = SexType.Woman },
                Children = new List<Person>()
                    {
                        new Person { Id=10, Name="王震天", CityId=10, Sex= SexType.Man },
                        new Person { Id=11, Name="王晴雨", CityId=10, Sex= SexType.Woman }
                    }

            };
            p1.Hobby.Add("吃饭");
            p1.Hobby.Add("拍视频");

            Console.WriteLine("********** Localization(): 简单 **********");
            var str = Lts.Default.Localization(p1);
            Console.WriteLine(str);
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("**** Localization():使用 替换 和 忽略项 ***");
            // 为 Person.Birthday 设置自定义转换程序
            Lts.Default.SetCustomLocalization<Person, PersonBirthdayLocalization>(p => p.Birthday);
            // 假设 10 代表北京, 21 代表成都
            var customPropertyValues = new Dictionary<string, string>
            {
                { "CityId",  "北京"},
                { "Spouse.CityId",  "上海"},
                { "Children[0].CityId",  "北京"},  // 数组使用下标指定某个元素
                { "Children[1].CityId",  "北京"},
            };
            var ignorePaths = new[]
            {
                "Password",
                "Spouse.Password",
                "Children.Password"  // 忽略数组元素的某个属性, 不需要加下标 [0]
            };
            str = Lts.Default.Localization(p1, null, customPropertyValues, ignorePaths);
            Console.WriteLine(str);
            Console.WriteLine();
            Console.WriteLine();

            //Console.WriteLine("********* Compare(): 带有忽略项 *********");
            var p2 = JsonConvert.DeserializeObject<Person>(JsonConvert.SerializeObject(p1));
            p2.Name = "王小锤";
            p2.Password = "1111111";
            p2.Spouse.Password = "1111111";
            p2.Children[1].Password = "111111";
            p2.Children[1].Name = "王下雨";

            var jsonCompare = new JsonCompare();
            var diffItems = jsonCompare.Compare(p1, p2, ignorePaths);
            var compareResultJson = JsonConvert.SerializeObject(diffItems);
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
        /// 名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 出生
        /// </summary>
        [DisplayName("出生日期")]
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
    /// Person.Birthday 没有设置有效值的话, 显示 未设置
    /// </summary>
    public class PersonBirthdayLocalization : ILocalizationToString
    {
        public string ToLocalization(object orginalValue, LocalizationStringContext context, string pathForReplaceValue, ReplacePair[] replacePairs, string pathForIgnore)
        {
            var replacePair = replacePairs?.FirstOrDefault(o => object.Equals(o.Orginal, orginalValue));
            if (replacePair != null)
            {
                return Help.FormatStringValue(replacePair.Replace);
            }

            string stringValue;
            if (orginalValue == null || object.Equals(orginalValue, DateTime.MinValue))
            {
                stringValue = "未设置";
            }
            else
            {
                stringValue = orginalValue.ToString();
            }

            return Help.FormatStringValue(stringValue);
        }
    }
}

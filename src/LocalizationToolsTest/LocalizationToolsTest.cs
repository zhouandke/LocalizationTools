using Localization2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LocalizationToolsTest
{
    /// <summary>
    /// LocalizationTools 测试
    /// </summary>
    [TestClass]
    public class LocalizationToolsTest
    {
        /// <summary>
        /// ToString 测试
        /// </summary>
        [TestMethod]
        public void LocalizationTest()
        {
            string expected, actual;

            var person = new Person
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

            // 为 Person.Birthday 设置自定义转换程序
            Lts.Default.SetCustomLocalization<Person, PersonCityLocalization>(p => p.CityId);
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
            var json = Lts.Default.Localization(person, null, customPropertyValues, ignorePaths);
            var jtoken = JToken.Parse(json);

            expected = person.Birthday.ToString();
            actual = jtoken.SelectToken("出生日期")?.ToString();
            Assert.AreEqual(expected, actual);

            expected = "北京";
            actual = jtoken.SelectToken("所在城市")?.ToString();
            Assert.AreEqual(expected, actual);

            expected = "男性";
            actual = jtoken.SelectToken("性别")?.ToString();
            Assert.AreEqual(expected, actual);

            actual = jtoken.SelectToken("密码")?.ToString();
            Assert.IsNull(actual);

            expected = "是";
            actual = jtoken.SelectToken("是否启用")?.ToString();
            Assert.AreEqual(expected, actual);

            expected = JsonConvert.SerializeObject(person.Hobby);
            actual = jtoken.SelectToken("爱好")?.ToString()?.Replace("\r\n", "")?.Replace(" ", "");
            Assert.AreEqual(expected, actual);

            expected = "上海";
            actual = jtoken.SelectToken("配偶.所在城市")?.ToString();
            Assert.AreEqual(expected, actual);

            actual = jtoken.SelectToken("配偶.密码")?.ToString();
            Assert.IsNull(actual);

            actual = jtoken.SelectToken("配偶.密码")?.ToString();
            Assert.IsNull(actual);

            expected = "未设置";
            actual = jtoken.SelectToken("配偶.是否启用")?.ToString();
            Assert.AreEqual(expected, actual);

            var expectedChildrenCount = 2;
            var actualChildren = jtoken.SelectTokens("子女.[*]");
            Assert.AreEqual(expectedChildrenCount, actualChildren?.Count());

            expected = "北京";
            actual = jtoken.SelectToken("子女[1].所在城市")?.ToString();
            Assert.AreEqual(expected, actual);

            actual = jtoken.SelectToken("子女[1].密码")?.ToString();
            Assert.IsNull(actual);


            var ignoreNullLts = new Lts() { IgnoreNullProperty = true };
            customPropertyValues = new Dictionary<string, string>
            {
                { "CityId",  null},
            };
            json = ignoreNullLts.Localization(person, null, customPropertyValues: customPropertyValues);
            jtoken = JToken.Parse(json);

            actual = jtoken.SelectToken("所在城市")?.ToString();
            Assert.IsNull(actual);

            actual = jtoken.SelectToken("配偶.爱好")?.ToString();
            Assert.IsNull(actual);
        }

        /// <summary>
        /// Compare 测试
        /// </summary>
        [TestMethod]
        public void CompareTest()
        {
            Lts.Default.SetCustomLocalization<Person, PersonCityLocalization>(p => p.CityId);

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
            var customPropertyValues1 = new Dictionary<string, string>
            {
                { "Password",  "******"},
                { "Spouse.Password",  "******"},
                { "Children[0].Password",  "******"},  // 数组使用下标指定某个元素
                { "Children[1].Password",  "******"},
            };
            var ignorePaths1 = new[]
            {
                "Birthday",
                "Spouse.Birthday",
                "Children.Birthday"  // 忽略数组元素的某个属性, 不需要加下标 [0]
            };
            var json1 = Lts.Default.Localization(p1, null, customPropertyValues1, ignorePaths1);

            var p2 = JsonConvert.DeserializeObject<Person>(JsonConvert.SerializeObject(p1));
            p2.Name = "王小锤";
            p2.CityId = 21;
            p2.Birthday = DateTime.MinValue;
            p2.Spouse.Sex = SexType.Ladyman;
            p2.Children[1].Name = "王下雨";
            p2.Children.Add(new Person { Id = 12, Name = "王倩云", CityId = 10, Sex = SexType.Woman });

            var customPropertyValues2 = new Dictionary<string, string>
            {
                { "Password",  "******"},
                { "Spouse.Password",  "******"},
                { "Children[0].Password",  "******"},  // 数组使用下标指定某个元素
                { "Children[1].Password",  "******"},
            };
            var ignorePaths2 = new[]
            {
                "Spouse.Birthday",
                "Children.Birthday"  // 忽略数组元素的某个属性, 不需要加下标 [0]
            };
            var json2 = Lts.Default.Localization(p2, null, customPropertyValues2, ignorePaths2);

            DiffItem[] diffItems;
            DiffItem diffItem;

            // 比较 json, 数组是默认整体比较
            diffItems = new JsonCompare(ArrayDiffMode.Entire).Compare(json1, json2);
            diffItem = diffItems.FirstOrDefault(o => o.Path == "名字");
            Assert.AreEqual("修改", diffItem?.Operation);
            Assert.AreEqual(p1.Name, diffItem?.From.ToString());
            Assert.AreEqual(p2.Name, diffItem?.To.ToString());

            diffItem = diffItems.FirstOrDefault(o => o.Path == "出生日期");
            Assert.AreEqual("修改", diffItem?.Operation);
            Assert.IsNull(diffItem.From);
            Assert.AreEqual(DateTime.MinValue.ToString(), diffItem?.To.ToString());

            diffItem = diffItems.FirstOrDefault(o => o.Path == "所在城市");
            Assert.AreEqual("修改", diffItem?.Operation);
            Assert.AreEqual("北京", diffItem?.From.ToString());
            Assert.AreEqual("上海", diffItem?.To.ToString());

            diffItem = diffItems.FirstOrDefault(o => o.Path == "配偶.性别");
            Assert.AreEqual("修改", diffItem?.Operation);
            Assert.AreEqual("女性", diffItem?.From.ToString());
            Assert.AreEqual("人妖", diffItem?.To.ToString());

            Assert.AreEqual(5, diffItems.Length);

            diffItem = diffItems.FirstOrDefault(o => o.Path == "子女");
            Assert.IsNotNull(diffItem);

            // 比较 json, 详细比较数组里的每一项
            diffItems = new JsonCompare(ArrayDiffMode.EvertyItem).Compare(json1, json2);
            Assert.AreEqual(6, diffItems.Length);

            diffItem = diffItems.FirstOrDefault(o => o.Path == "子女[1].名字");
            Assert.AreEqual("修改", diffItem?.Operation);
            Assert.AreEqual("王晴雨", diffItem?.From.ToString());
            Assert.AreEqual("王下雨", diffItem?.To.ToString());

            diffItem = diffItems.FirstOrDefault(o => o.Path == "子女[2]");
            Assert.AreEqual("增加", diffItem?.Operation);


            // 直接比较 两个对象
            diffItems = new JsonCompare(ArrayDiffMode.Entire).Compare(p1, p2);
            diffItem = diffItems.FirstOrDefault(o => o.Path == "名字");
            Assert.AreEqual("修改", diffItem?.Operation);
            Assert.AreEqual(p1.Name, diffItem?.From.ToString());
            Assert.AreEqual(p2.Name, diffItem?.To.ToString());

            diffItem = diffItems.FirstOrDefault(o => o.Path == "出生日期");
            Assert.AreEqual("修改", diffItem?.Operation);
            Assert.AreEqual(p1.Birthday.ToString(), diffItem?.From.ToString());
            Assert.AreEqual(DateTime.MinValue.ToString(), diffItem?.To.ToString());

            diffItem = diffItems.FirstOrDefault(o => o.Path == "所在城市");
            Assert.AreEqual("修改", diffItem?.Operation);
            Assert.AreEqual("北京", diffItem?.From.ToString());
            Assert.AreEqual("上海", diffItem?.To.ToString());

            diffItem = diffItems.FirstOrDefault(o => o.Path == "配偶.性别");
            Assert.AreEqual("修改", diffItem?.Operation);
            Assert.AreEqual("女性", diffItem?.From.ToString());
            Assert.AreEqual("人妖", diffItem?.To.ToString());

            Assert.AreEqual(5, diffItems.Length);
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
        public List<string> Hobby { get; set; }

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

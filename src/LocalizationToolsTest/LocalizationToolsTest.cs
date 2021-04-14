using Localization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public void ToStringTest()
        {
            string actual, expected;
            var person = new Person()
            {
                Id = 1,
                Name = "王大锤",
                Birthdate = DateTime.Parse("2001-02-01"),
                CityId = 10,
                Password = "123456789",
                Sex = SexType.Man,
                Height = 170,
                CanLogin = true,
                GraduateDate = null
            };

            LocalizationTools.KeyValueSeparator = "=";
            actual = LocalizationTools.ToString(person);
            expected = "{\"唯一性标识\"=1,\"名字\"=\"王大锤\",\"Birthdate\"=\"2001/2/1 0:00:00\",\"所在城市\"=10,\"性别\"=\"男性\",\"身高\"=170,\"密码\"=\"123456789\",\"是否可以登录\"=true,\"毕业时间\"=null}";
            Assert.AreEqual(expected, actual);

            LocalizationTools.KeyValueSeparator = ":";
            actual = LocalizationTools.ToString(person);
            expected = "{\"唯一性标识\":1,\"名字\":\"王大锤\",\"Birthdate\":\"2001/2/1 0:00:00\",\"所在城市\":10,\"性别\":\"男性\",\"身高\":170,\"密码\":\"123456789\",\"是否可以登录\":true,\"毕业时间\":null}";
            Assert.AreEqual(expected, actual);

            actual = LocalizationTools.ToString(person, nameof(Person.Password));
            expected = "{\"唯一性标识\":1,\"名字\":\"王大锤\",\"Birthdate\":\"2001/2/1 0:00:00\",\"所在城市\":10,\"性别\":\"男性\",\"身高\":170,\"是否可以登录\":true,\"毕业时间\":null}";
            Assert.AreEqual(expected, actual);

            actual = LocalizationTools.ToString(person, new { CityId = "北京" }, nameof(Person.Password));
            expected = "{\"唯一性标识\":1,\"名字\":\"王大锤\",\"Birthdate\":\"2001/2/1 0:00:00\",\"所在城市\":\"北京\",\"性别\":\"男性\",\"身高\":170,\"是否可以登录\":true,\"毕业时间\":null}";
            Assert.AreEqual(expected, actual);
            actual = LocalizationTools.ToString(person, new Dictionary<string, object>() { { "CityId", "北京" } }, nameof(Person.Password));
            Assert.AreEqual(expected, actual);




            actual = LocalizationTools.ToStringInclude(person, new[]
                {
                    nameof(Person.Name),
                    nameof(Person.Birthdate),
                    nameof(Person.CityId),
                    nameof(Person.Sex),
                });
            expected = "{\"名字\":\"王大锤\",\"Birthdate\":\"2001/2/1 0:00:00\",\"所在城市\":10,\"性别\":\"男性\"}";
            Assert.AreEqual(expected, actual);

            actual = LocalizationTools.ToStringInclude(person, new[]
                {
                    nameof(Person.Name),
                    nameof(Person.Birthdate),
                    nameof(Person.CityId),
                    nameof(Person.Sex),
                }, new { CityId = "北京" });
            expected = "{\"名字\":\"王大锤\",\"Birthdate\":\"2001/2/1 0:00:00\",\"所在城市\":\"北京\",\"性别\":\"男性\"}";
            Assert.AreEqual(expected, actual);
            actual = LocalizationTools.ToStringInclude(person, new[]
                {
                    nameof(Person.Name),
                    nameof(Person.Birthdate),
                    nameof(Person.CityId),
                    nameof(Person.Sex),
                }, new Dictionary<string, object>() { { "CityId", "北京" } });
            Assert.AreEqual(expected, actual);


            var sexTypes = new[] { SexType.Man, SexType.Woman, SexType.Ladyman };
            actual = LocalizationTools.ToString(sexTypes);
            expected = "[\"男性\",\"女性\",\"Ladyman\"]";
            Assert.AreEqual(expected, actual);


            var employe = new Employe()
            {
                Id = 1,
                Name = "王大锤",
                Sex = SexType.Man,
                CanLogin = true,
                Partner = new Employe() { Id = 2, Name = "渣渣" },
            };

            actual = LocalizationTools.ToString(employe);
            expected = "{\"唯一性标识\":1,\"名字\":\"王大锤\",\"性别\":\"男性\",\"可否登录\":\"可以登录\",\"搭档\":{\"唯一性标识\":2,\"名字\":\"渣渣\",\"性别\":\"男性\",\"可否登录\":\"未配置\",\"搭档\":\"没有搭档\"}}";
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Compare 测试
        /// </summary>
        [TestMethod]
        public void CompareTest()
        {
            string actual, expected;
            var e1 = new Employe()
            {
                Id = 1,
                Name = "王大锤",
                Sex = SexType.Man,
                CanLogin = true,
                Partner = new Employe() { Id = 2, Name = "渣渣" },
            };

            var e2 = new Employe()
            {
                Id = 2,
                Name = "王小锤",
                Sex = SexType.Ladyman,
                CanLogin = true,
                Partner = null,
            };

            var compareResult = LocalizationTools.Compare(e1, e2, new[] { nameof(Employe.Id) });

            Assert.IsTrue(compareResult.HasDifference);
            Assert.AreEqual(compareResult.DifferentProperties.Count, 3);
            Assert.IsTrue(compareResult.IsPropertyDifferent(nameof(Employe.Name)));
            Assert.IsTrue(compareResult.IsPropertyDifferent(nameof(Employe.Sex)));
            Assert.IsFalse(compareResult.IsPropertyDifferent(nameof(Employe.CanLogin)));
            Assert.IsTrue(compareResult.IsPropertyDifferent(nameof(Employe.Partner)));

            Assert.AreEqual("\"王大锤\"", compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Name)).From);
            Assert.AreEqual("\"王小锤\"", compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Name)).To);

            Assert.AreEqual("\"男性\"", compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Sex)).From);
            Assert.AreEqual("\"Ladyman\"", compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Sex)).To);

            expected = "{\"唯一性标识\":2,\"名字\":\"渣渣\",\"性别\":\"男性\",\"可否登录\":\"未配置\",\"搭档\":\"没有搭档\"}";
            actual = compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Partner)).From;
            Assert.AreEqual(expected, actual);
            Assert.AreEqual("\"没有搭档\"", compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Partner)).To);

            expected = "{\"名字\":{\"从\":\"王大锤\",\"变成\":\"王小锤\"},\"性别\":{\"从\":\"男性\",\"变成\":\"Ladyman\"},\"搭档\":{\"从\":{\"唯一性标识\":2,\"名字\":\"渣渣\",\"性别\":\"男性\",\"可否登录\":\"未配置\",\"搭档\":\"没有搭档\"},\"变成\":\"没有搭档\"}}";
            actual = compareResult.GetDifferenceMsg();
            Assert.AreEqual(expected, actual);

            expected = "{}";
            actual = compareResult.GetDifferenceMsg(new[] { nameof(Employe.Name), nameof(Employe.Sex), nameof(Employe.Partner) });
            Assert.AreEqual(expected, actual);


            var updateDifferentPropertyResult = compareResult.UpdateDifferentProperty(nameof(Employe.Name), "王大锤", "王大锤");
            Assert.AreEqual(updateDifferentPropertyResult, false);
            // 没有跟新成功, to值不应该改变
            actual = compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Name)).To;
            Assert.AreEqual(actual, "\"王小锤\"");

            updateDifferentPropertyResult = compareResult.UpdateDifferentProperty(nameof(Employe.Name), "王大锤", new { 姓 = "王", 名 = "大锤" });
            Assert.AreEqual("\"王大锤\"", compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Name)).From);
            expected = "{\"姓\":\"王\",\"名\":\"大锤\"}";
            actual = compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Name)).To;
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(updateDifferentPropertyResult, true);


            expected = "{\"名字\":{\"从\":\"王大锤\",\"变成\":{\"姓\":\"王\",\"名\":\"大锤\"}},\"性别\":{\"从\":\"男性\",\"变成\":\"Ladyman\"},\"搭档\":{\"从\":{\"唯一性标识\":2,\"名字\":\"渣渣\",\"性别\":\"男性\",\"可否登录\":\"未配置\",\"搭档\":\"没有搭档\"},\"变成\":\"没有搭档\"}}";
            actual = compareResult.GetDifferenceMsg();
            Assert.AreEqual(expected, actual);


            Assert.ThrowsException<ArgumentException>(() =>
            {
                compareResult.AddNewDifferentProperty("名字", "111", "111", nameof(Employe.Name));
            });
            Assert.ThrowsException<ArgumentException>(() =>
            {
                compareResult.AddNewDifferentProperty("名字", "111", "111", "NotExistPropertyName");
            });
            compareResult.AddNewDifferentProperty("地址", "北街", "胡同1号", "Address");
            Assert.AreEqual(compareResult.IsPropertyDifferent("Address"), true);


            compareResult = LocalizationTools.CompareInclude(e1, e2, new[] { nameof(Employe.Name) });
            Assert.IsTrue(compareResult.HasDifference);
            Assert.AreEqual(compareResult.DifferentProperties.Count, 1);
            Assert.IsTrue(compareResult.IsPropertyDifferent(nameof(Employe.Name)));
            Assert.IsFalse(compareResult.IsPropertyDifferent(nameof(Employe.Sex)));
            Assert.IsFalse(compareResult.IsPropertyDifferent(nameof(Employe.CanLogin)));
            Assert.IsFalse(compareResult.IsPropertyDifferent(nameof(Employe.Partner)));

            Assert.AreEqual("\"王大锤\"", compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Name)).From);
            Assert.AreEqual("\"王小锤\"", compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Name)).To);

            expected = "{\"名字\":{\"从\":\"王大锤\",\"变成\":\"王小锤\"}}";
            actual = compareResult.GetDifferenceMsg();
            Assert.AreEqual(expected, actual);


            compareResult.DifferentProperties.Clear();
            Assert.AreEqual(compareResult.HasDifference, false);


            var o1 = new Order() { Id = 1, OrderNo = "1111", Amount = 999M };
            var o2 = new Order() { Id = 1, OrderNo = "1111", Amount = 999.00M };
            compareResult = LocalizationTools.Compare(o1, o2);
            Assert.AreEqual(false, compareResult.HasDifference);
        }
    }

    /// <summary>
    /// 自然人
    /// </summary>
    public class Person
    {
        /// <summary>
        ///  唯一性标识
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 名字, 在 summary 里可能会被加入各种其他说明, 所以用 [DisplayName] 来替换
        /// </summary>
        [DisplayName("名字")]
        public string Name { get; set; }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public DateTime Birthdate { get; set; } // 测试没有任何 别名 的情况
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

        /// <summary>
        /// 所在城市
        /// </summary>
        public int CityId { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public SexType Sex { get; set; }

        /// <summary>
        /// 身高
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 是否可以登录
        /// </summary>
        public bool CanLogin { get; set; }

        /// <summary>
        /// 毕业时间
        /// </summary>
        public DateTimeOffset? GraduateDate { get; set; }
    }

    /// <summary>
    /// 性别
    /// </summary>
    public enum SexType
    {
        /// <summary>
        /// 男性
        /// </summary>
        Man = 0,

        /// <summary>
        /// 女性, balabalabalabalabalabalabalabalabala
        /// </summary>
        [EnumAlias("女性")]
        Woman = 2,

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        Ladyman = 3,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
    }

    /// <summary>
    /// 员工
    /// </summary>
    public class Employe
    {
        /// <summary>
        ///  唯一性标识
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public SexType Sex { get; set; }

        /// <summary>
        /// 可否登录
        /// </summary>
        [ToStringReplacePair(null, "未配置", true, "可以登录", false, "不能登录")]
        public bool? CanLogin { get; set; }

        /// <summary>
        /// 搭档
        /// </summary>
        [ToStringReplacePair(null, "没有搭档")]
        public Employe Partner { get; set; }
    }

    public class Order
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 订单金额
        /// </summary>
        public decimal Amount { get; set; }
    }
}

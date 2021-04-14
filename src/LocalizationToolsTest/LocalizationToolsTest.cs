using Localization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LocalizationToolsTest
{
    /// <summary>
    /// LocalizationTools ����
    /// </summary>
    [TestClass]
    public class LocalizationToolsTest
    {
        /// <summary>
        /// ToString ����
        /// </summary>
        [TestMethod]
        public void ToStringTest()
        {
            string actual, expected;
            var person = new Person()
            {
                Id = 1,
                Name = "����",
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
            expected = "{\"Ψһ�Ա�ʶ\"=1,\"����\"=\"����\",\"Birthdate\"=\"2001/2/1 0:00:00\",\"���ڳ���\"=10,\"�Ա�\"=\"����\",\"���\"=170,\"����\"=\"123456789\",\"�Ƿ���Ե�¼\"=true,\"��ҵʱ��\"=null}";
            Assert.AreEqual(expected, actual);

            LocalizationTools.KeyValueSeparator = ":";
            actual = LocalizationTools.ToString(person);
            expected = "{\"Ψһ�Ա�ʶ\":1,\"����\":\"����\",\"Birthdate\":\"2001/2/1 0:00:00\",\"���ڳ���\":10,\"�Ա�\":\"����\",\"���\":170,\"����\":\"123456789\",\"�Ƿ���Ե�¼\":true,\"��ҵʱ��\":null}";
            Assert.AreEqual(expected, actual);

            actual = LocalizationTools.ToString(person, nameof(Person.Password));
            expected = "{\"Ψһ�Ա�ʶ\":1,\"����\":\"����\",\"Birthdate\":\"2001/2/1 0:00:00\",\"���ڳ���\":10,\"�Ա�\":\"����\",\"���\":170,\"�Ƿ���Ե�¼\":true,\"��ҵʱ��\":null}";
            Assert.AreEqual(expected, actual);

            actual = LocalizationTools.ToString(person, new { CityId = "����" }, nameof(Person.Password));
            expected = "{\"Ψһ�Ա�ʶ\":1,\"����\":\"����\",\"Birthdate\":\"2001/2/1 0:00:00\",\"���ڳ���\":\"����\",\"�Ա�\":\"����\",\"���\":170,\"�Ƿ���Ե�¼\":true,\"��ҵʱ��\":null}";
            Assert.AreEqual(expected, actual);
            actual = LocalizationTools.ToString(person, new Dictionary<string, object>() { { "CityId", "����" } }, nameof(Person.Password));
            Assert.AreEqual(expected, actual);




            actual = LocalizationTools.ToStringInclude(person, new[]
                {
                    nameof(Person.Name),
                    nameof(Person.Birthdate),
                    nameof(Person.CityId),
                    nameof(Person.Sex),
                });
            expected = "{\"����\":\"����\",\"Birthdate\":\"2001/2/1 0:00:00\",\"���ڳ���\":10,\"�Ա�\":\"����\"}";
            Assert.AreEqual(expected, actual);

            actual = LocalizationTools.ToStringInclude(person, new[]
                {
                    nameof(Person.Name),
                    nameof(Person.Birthdate),
                    nameof(Person.CityId),
                    nameof(Person.Sex),
                }, new { CityId = "����" });
            expected = "{\"����\":\"����\",\"Birthdate\":\"2001/2/1 0:00:00\",\"���ڳ���\":\"����\",\"�Ա�\":\"����\"}";
            Assert.AreEqual(expected, actual);
            actual = LocalizationTools.ToStringInclude(person, new[]
                {
                    nameof(Person.Name),
                    nameof(Person.Birthdate),
                    nameof(Person.CityId),
                    nameof(Person.Sex),
                }, new Dictionary<string, object>() { { "CityId", "����" } });
            Assert.AreEqual(expected, actual);


            var sexTypes = new[] { SexType.Man, SexType.Woman, SexType.Ladyman };
            actual = LocalizationTools.ToString(sexTypes);
            expected = "[\"����\",\"Ů��\",\"Ladyman\"]";
            Assert.AreEqual(expected, actual);


            var employe = new Employe()
            {
                Id = 1,
                Name = "����",
                Sex = SexType.Man,
                CanLogin = true,
                Partner = new Employe() { Id = 2, Name = "����" },
            };

            actual = LocalizationTools.ToString(employe);
            expected = "{\"Ψһ�Ա�ʶ\":1,\"����\":\"����\",\"�Ա�\":\"����\",\"�ɷ��¼\":\"���Ե�¼\",\"�\":{\"Ψһ�Ա�ʶ\":2,\"����\":\"����\",\"�Ա�\":\"����\",\"�ɷ��¼\":\"δ����\",\"�\":\"û�д\"}}";
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Compare ����
        /// </summary>
        [TestMethod]
        public void CompareTest()
        {
            string actual, expected;
            var e1 = new Employe()
            {
                Id = 1,
                Name = "����",
                Sex = SexType.Man,
                CanLogin = true,
                Partner = new Employe() { Id = 2, Name = "����" },
            };

            var e2 = new Employe()
            {
                Id = 2,
                Name = "��С��",
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

            Assert.AreEqual("\"����\"", compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Name)).From);
            Assert.AreEqual("\"��С��\"", compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Name)).To);

            Assert.AreEqual("\"����\"", compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Sex)).From);
            Assert.AreEqual("\"Ladyman\"", compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Sex)).To);

            expected = "{\"Ψһ�Ա�ʶ\":2,\"����\":\"����\",\"�Ա�\":\"����\",\"�ɷ��¼\":\"δ����\",\"�\":\"û�д\"}";
            actual = compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Partner)).From;
            Assert.AreEqual(expected, actual);
            Assert.AreEqual("\"û�д\"", compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Partner)).To);

            expected = "{\"����\":{\"��\":\"����\",\"���\":\"��С��\"},\"�Ա�\":{\"��\":\"����\",\"���\":\"Ladyman\"},\"�\":{\"��\":{\"Ψһ�Ա�ʶ\":2,\"����\":\"����\",\"�Ա�\":\"����\",\"�ɷ��¼\":\"δ����\",\"�\":\"û�д\"},\"���\":\"û�д\"}}";
            actual = compareResult.GetDifferenceMsg();
            Assert.AreEqual(expected, actual);

            expected = "{}";
            actual = compareResult.GetDifferenceMsg(new[] { nameof(Employe.Name), nameof(Employe.Sex), nameof(Employe.Partner) });
            Assert.AreEqual(expected, actual);


            var updateDifferentPropertyResult = compareResult.UpdateDifferentProperty(nameof(Employe.Name), "����", "����");
            Assert.AreEqual(updateDifferentPropertyResult, false);
            // û�и��³ɹ�, toֵ��Ӧ�øı�
            actual = compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Name)).To;
            Assert.AreEqual(actual, "\"��С��\"");

            updateDifferentPropertyResult = compareResult.UpdateDifferentProperty(nameof(Employe.Name), "����", new { �� = "��", �� = "��" });
            Assert.AreEqual("\"����\"", compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Name)).From);
            expected = "{\"��\":\"��\",\"��\":\"��\"}";
            actual = compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Name)).To;
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(updateDifferentPropertyResult, true);


            expected = "{\"����\":{\"��\":\"����\",\"���\":{\"��\":\"��\",\"��\":\"��\"}},\"�Ա�\":{\"��\":\"����\",\"���\":\"Ladyman\"},\"�\":{\"��\":{\"Ψһ�Ա�ʶ\":2,\"����\":\"����\",\"�Ա�\":\"����\",\"�ɷ��¼\":\"δ����\",\"�\":\"û�д\"},\"���\":\"û�д\"}}";
            actual = compareResult.GetDifferenceMsg();
            Assert.AreEqual(expected, actual);


            Assert.ThrowsException<ArgumentException>(() =>
            {
                compareResult.AddNewDifferentProperty("����", "111", "111", nameof(Employe.Name));
            });
            Assert.ThrowsException<ArgumentException>(() =>
            {
                compareResult.AddNewDifferentProperty("����", "111", "111", "NotExistPropertyName");
            });
            compareResult.AddNewDifferentProperty("��ַ", "����", "��ͬ1��", "Address");
            Assert.AreEqual(compareResult.IsPropertyDifferent("Address"), true);


            compareResult = LocalizationTools.CompareInclude(e1, e2, new[] { nameof(Employe.Name) });
            Assert.IsTrue(compareResult.HasDifference);
            Assert.AreEqual(compareResult.DifferentProperties.Count, 1);
            Assert.IsTrue(compareResult.IsPropertyDifferent(nameof(Employe.Name)));
            Assert.IsFalse(compareResult.IsPropertyDifferent(nameof(Employe.Sex)));
            Assert.IsFalse(compareResult.IsPropertyDifferent(nameof(Employe.CanLogin)));
            Assert.IsFalse(compareResult.IsPropertyDifferent(nameof(Employe.Partner)));

            Assert.AreEqual("\"����\"", compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Name)).From);
            Assert.AreEqual("\"��С��\"", compareResult.DifferentProperties.Single(o => o.PropertyName == nameof(Employe.Name)).To);

            expected = "{\"����\":{\"��\":\"����\",\"���\":\"��С��\"}}";
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
    /// ��Ȼ��
    /// </summary>
    public class Person
    {
        /// <summary>
        ///  Ψһ�Ա�ʶ
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ����, �� summary ����ܻᱻ�����������˵��, ������ [DisplayName] ���滻
        /// </summary>
        [DisplayName("����")]
        public string Name { get; set; }

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
        public DateTime Birthdate { get; set; } // ����û���κ� ���� �����
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��

        /// <summary>
        /// ���ڳ���
        /// </summary>
        public int CityId { get; set; }

        /// <summary>
        /// �Ա�
        /// </summary>
        public SexType Sex { get; set; }

        /// <summary>
        /// ���
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// �Ƿ���Ե�¼
        /// </summary>
        public bool CanLogin { get; set; }

        /// <summary>
        /// ��ҵʱ��
        /// </summary>
        public DateTimeOffset? GraduateDate { get; set; }
    }

    /// <summary>
    /// �Ա�
    /// </summary>
    public enum SexType
    {
        /// <summary>
        /// ����
        /// </summary>
        Man = 0,

        /// <summary>
        /// Ů��, balabalabalabalabalabalabalabalabala
        /// </summary>
        [EnumAlias("Ů��")]
        Woman = 2,

#pragma warning disable CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
        Ladyman = 3,
#pragma warning restore CS1591 // ȱ�ٶԹ����ɼ����ͻ��Ա�� XML ע��
    }

    /// <summary>
    /// Ա��
    /// </summary>
    public class Employe
    {
        /// <summary>
        ///  Ψһ�Ա�ʶ
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// �Ա�
        /// </summary>
        public SexType Sex { get; set; }

        /// <summary>
        /// �ɷ��¼
        /// </summary>
        [ToStringReplacePair(null, "δ����", true, "���Ե�¼", false, "���ܵ�¼")]
        public bool? CanLogin { get; set; }

        /// <summary>
        /// �
        /// </summary>
        [ToStringReplacePair(null, "û�д")]
        public Employe Partner { get; set; }
    }

    public class Order
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        public decimal Amount { get; set; }
    }
}

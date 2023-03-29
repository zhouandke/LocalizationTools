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
    /// LocalizationTools ����
    /// </summary>
    [TestClass]
    public class LocalizationToolsTest
    {
        /// <summary>
        /// ToString ����
        /// </summary>
        [TestMethod]
        public void LocalizationTest()
        {
            string expected, actual;

            var person = new Person
            {
                Id = 1,
                Name = "����",
                Birthday = DateTime.Parse("2020-01-01"),
                CityId = 10,
                Sex = SexType.Man,
                Password = "12345657689",
                IsAvailable = true,
                Hobby = new List<string> { "�Է�", "����Ƶ" },
                Spouse = new Person() { Id = 2, CityId = 21, Name = "����", Sex = SexType.Woman },
                Children = new List<Person>()
                {
                    new Person { Id=10, Name="������", CityId=10, Sex= SexType.Man },
                    new Person { Id=11, Name="������", CityId=10, Sex= SexType.Woman }
                }
            };

            // Ϊ Person.Birthday �����Զ���ת������
            Lts.Default.SetCustomLocalization<Person, PersonCityLocalization>(p => p.CityId);
            // ���� 10 ������, 21 ����ɶ�
            var customPropertyValues = new Dictionary<string, string>
            {
                { "CityId",  "����"},
                { "Spouse.CityId",  "�Ϻ�"},
                { "Children[0].CityId",  "����"},  // ����ʹ���±�ָ��ĳ��Ԫ��
                { "Children[1].CityId",  "����"},
            };
            var ignorePaths = new[]
            {
                "Password",
                "Spouse.Password",
                "Children.Password"  // ��������Ԫ�ص�ĳ������, ����Ҫ���±� [0]
            };
            var json = Lts.Default.Localization(person, null, customPropertyValues, ignorePaths);
            var jtoken = JToken.Parse(json);

            expected = person.Birthday.ToString();
            actual = jtoken.SelectToken("��������")?.ToString();
            Assert.AreEqual(expected, actual);

            expected = "����";
            actual = jtoken.SelectToken("���ڳ���")?.ToString();
            Assert.AreEqual(expected, actual);

            expected = "����";
            actual = jtoken.SelectToken("�Ա�")?.ToString();
            Assert.AreEqual(expected, actual);

            actual = jtoken.SelectToken("����")?.ToString();
            Assert.IsNull(actual);

            expected = "��";
            actual = jtoken.SelectToken("�Ƿ�����")?.ToString();
            Assert.AreEqual(expected, actual);

            expected = JsonConvert.SerializeObject(person.Hobby);
            actual = jtoken.SelectToken("����")?.ToString()?.Replace("\r\n", "")?.Replace(" ", "");
            Assert.AreEqual(expected, actual);

            expected = "�Ϻ�";
            actual = jtoken.SelectToken("��ż.���ڳ���")?.ToString();
            Assert.AreEqual(expected, actual);

            actual = jtoken.SelectToken("��ż.����")?.ToString();
            Assert.IsNull(actual);

            actual = jtoken.SelectToken("��ż.����")?.ToString();
            Assert.IsNull(actual);

            expected = "δ����";
            actual = jtoken.SelectToken("��ż.�Ƿ�����")?.ToString();
            Assert.AreEqual(expected, actual);

            var expectedChildrenCount = 2;
            var actualChildren = jtoken.SelectTokens("��Ů.[*]");
            Assert.AreEqual(expectedChildrenCount, actualChildren?.Count());

            expected = "����";
            actual = jtoken.SelectToken("��Ů[1].���ڳ���")?.ToString();
            Assert.AreEqual(expected, actual);

            actual = jtoken.SelectToken("��Ů[1].����")?.ToString();
            Assert.IsNull(actual);


            var ignoreNullLts = new Lts() { IgnoreNullProperty = true };
            customPropertyValues = new Dictionary<string, string>
            {
                { "CityId",  null},
            };
            json = ignoreNullLts.Localization(person, null, customPropertyValues: customPropertyValues);
            jtoken = JToken.Parse(json);

            actual = jtoken.SelectToken("���ڳ���")?.ToString();
            Assert.IsNull(actual);

            actual = jtoken.SelectToken("��ż.����")?.ToString();
            Assert.IsNull(actual);
        }

        /// <summary>
        /// Compare ����
        /// </summary>
        [TestMethod]
        public void CompareTest()
        {
            Lts.Default.SetCustomLocalization<Person, PersonCityLocalization>(p => p.CityId);

            var p1 = new Person
            {
                Id = 1,
                Name = "����",
                Birthday = DateTime.Parse("2020-01-01"),
                CityId = 10,
                Sex = SexType.Man,
                Password = "12345657689",
                IsAvailable = true,
                Hobby = new List<string> { "�Է�", "����Ƶ" },
                Spouse = new Person() { Id = 2, CityId = 21, Name = "����", Sex = SexType.Woman },
                Children = new List<Person>()
                {
                    new Person { Id=10, Name="������", CityId=10, Sex= SexType.Man },
                    new Person { Id=11, Name="������", CityId=10, Sex= SexType.Woman }
                }
            };
            var customPropertyValues1 = new Dictionary<string, string>
            {
                { "Password",  "******"},
                { "Spouse.Password",  "******"},
                { "Children[0].Password",  "******"},  // ����ʹ���±�ָ��ĳ��Ԫ��
                { "Children[1].Password",  "******"},
            };
            var ignorePaths1 = new[]
            {
                "Birthday",
                "Spouse.Birthday",
                "Children.Birthday"  // ��������Ԫ�ص�ĳ������, ����Ҫ���±� [0]
            };
            var json1 = Lts.Default.Localization(p1, null, customPropertyValues1, ignorePaths1);

            var p2 = JsonConvert.DeserializeObject<Person>(JsonConvert.SerializeObject(p1));
            p2.Name = "��С��";
            p2.CityId = 21;
            p2.Birthday = DateTime.MinValue;
            p2.Spouse.Sex = SexType.Ladyman;
            p2.Children[1].Name = "������";
            p2.Children.Add(new Person { Id = 12, Name = "��ٻ��", CityId = 10, Sex = SexType.Woman });

            var customPropertyValues2 = new Dictionary<string, string>
            {
                { "Password",  "******"},
                { "Spouse.Password",  "******"},
                { "Children[0].Password",  "******"},  // ����ʹ���±�ָ��ĳ��Ԫ��
                { "Children[1].Password",  "******"},
            };
            var ignorePaths2 = new[]
            {
                "Spouse.Birthday",
                "Children.Birthday"  // ��������Ԫ�ص�ĳ������, ����Ҫ���±� [0]
            };
            var json2 = Lts.Default.Localization(p2, null, customPropertyValues2, ignorePaths2);

            DiffItem[] diffItems;
            DiffItem diffItem;

            // �Ƚ� json, ������Ĭ������Ƚ�
            diffItems = new JsonCompare(ArrayDiffMode.Entire).Compare(json1, json2);
            diffItem = diffItems.FirstOrDefault(o => o.Path == "����");
            Assert.AreEqual("�޸�", diffItem?.Operation);
            Assert.AreEqual(p1.Name, diffItem?.From.ToString());
            Assert.AreEqual(p2.Name, diffItem?.To.ToString());

            diffItem = diffItems.FirstOrDefault(o => o.Path == "��������");
            Assert.AreEqual("�޸�", diffItem?.Operation);
            Assert.IsNull(diffItem.From);
            Assert.AreEqual(DateTime.MinValue.ToString(), diffItem?.To.ToString());

            diffItem = diffItems.FirstOrDefault(o => o.Path == "���ڳ���");
            Assert.AreEqual("�޸�", diffItem?.Operation);
            Assert.AreEqual("����", diffItem?.From.ToString());
            Assert.AreEqual("�Ϻ�", diffItem?.To.ToString());

            diffItem = diffItems.FirstOrDefault(o => o.Path == "��ż.�Ա�");
            Assert.AreEqual("�޸�", diffItem?.Operation);
            Assert.AreEqual("Ů��", diffItem?.From.ToString());
            Assert.AreEqual("����", diffItem?.To.ToString());

            Assert.AreEqual(5, diffItems.Length);

            diffItem = diffItems.FirstOrDefault(o => o.Path == "��Ů");
            Assert.IsNotNull(diffItem);

            // �Ƚ� json, ��ϸ�Ƚ��������ÿһ��
            diffItems = new JsonCompare(ArrayDiffMode.EvertyItem).Compare(json1, json2);
            Assert.AreEqual(6, diffItems.Length);

            diffItem = diffItems.FirstOrDefault(o => o.Path == "��Ů[1].����");
            Assert.AreEqual("�޸�", diffItem?.Operation);
            Assert.AreEqual("������", diffItem?.From.ToString());
            Assert.AreEqual("������", diffItem?.To.ToString());

            diffItem = diffItems.FirstOrDefault(o => o.Path == "��Ů[2]");
            Assert.AreEqual("����", diffItem?.Operation);


            // ֱ�ӱȽ� ��������
            diffItems = new JsonCompare(ArrayDiffMode.Entire).Compare(p1, p2);
            diffItem = diffItems.FirstOrDefault(o => o.Path == "����");
            Assert.AreEqual("�޸�", diffItem?.Operation);
            Assert.AreEqual(p1.Name, diffItem?.From.ToString());
            Assert.AreEqual(p2.Name, diffItem?.To.ToString());

            diffItem = diffItems.FirstOrDefault(o => o.Path == "��������");
            Assert.AreEqual("�޸�", diffItem?.Operation);
            Assert.AreEqual(p1.Birthday.ToString(), diffItem?.From.ToString());
            Assert.AreEqual(DateTime.MinValue.ToString(), diffItem?.To.ToString());

            diffItem = diffItems.FirstOrDefault(o => o.Path == "���ڳ���");
            Assert.AreEqual("�޸�", diffItem?.Operation);
            Assert.AreEqual("����", diffItem?.From.ToString());
            Assert.AreEqual("�Ϻ�", diffItem?.To.ToString());

            diffItem = diffItems.FirstOrDefault(o => o.Path == "��ż.�Ա�");
            Assert.AreEqual("�޸�", diffItem?.Operation);
            Assert.AreEqual("Ů��", diffItem?.From.ToString());
            Assert.AreEqual("����", diffItem?.To.ToString());

            Assert.AreEqual(5, diffItems.Length);
        }
    }

    /// <summary>
    /// ����
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        [DisplayName("��������")]
        public DateTime Birthday { get; set; }

        /// <summary>
        /// �Ա�
        /// </summary>
        public SexType Sex { get; set; }

        /// <summary>
        /// ���ڳ���
        /// </summary>
        public int CityId { get; set; }

        /// <summary>
        /// �Ƿ�����
        /// </summary>
        [LtsReplace(null, "δ����", true, "��", false, "��")]
        public bool? IsAvailable { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public List<string> Hobby { get; set; }

        /// <summary>
        /// ��ż
        /// </summary>
        [LtsReplace(null, "û����ż")]
        public Person Spouse { get; set; }

        /// <summary>
        /// ��Ů
        /// </summary>
        public List<Person> Children { get; set; } = new List<Person>();
    }

    /// <summary>
    /// �Ա�
    /// </summary>
    public enum SexType
    {
        Unknown = 0,

        [LtsEnumAlias("����")]
        Man = 1,
        /// <summary>
        /// Ů��
        /// </summary>
        Woman = 2,
        /// <summary>
        /// ����
        /// </summary>
        //[EnumAlias("����")]
        Ladyman = 3,
    }


    /// <summary>
    /// ģ������ݿ���� City Name
    /// </summary>
    public class PersonCityLocalization : ILocalizationToString
    {
        public string Localization(object orginalValue, LocalizationStringContext context, string pathForReplaceValue, ReplacePair[] replacePairs, string pathForIgnore)
        {
            // ���� replacePairs, ȫ�������ݿ��ȡ

            var orm = context.State; // ���贫��state �� orm
            string stringValue;
            if (object.Equals(orginalValue, 10))
            {
                stringValue = "����";
            }
            else if (object.Equals(orginalValue, 21))
            {
                stringValue = "�Ϻ�";
            }
            else
            {
                stringValue = $"δ֪����Id={orginalValue}";
            }
            return Help.FormatStringValue(stringValue);
        }
    }
}

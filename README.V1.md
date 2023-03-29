# LocalizationTools
ToString() 使用中文注释来代替 属性名称, 并提供一个简单的比较工具

使用示例, 请务必在 Visual Studio 的项目编辑界面 --> 生成 --> 输出, 勾选 XML文档文件

``` csharp
using Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel;

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
                Spouse = new Person() { Id = 2, Name = "老婆", Sex = SexType.Woman }
            };
            p1.Hobby.Add("吃饭");
            p1.Hobby.Add("拍视频");

            Console.WriteLine("********** ToString(): 简单 **********");
            var str = LocalizationTools.ToString(p1);
            Console.WriteLine(str);
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("**** ToString():使用 替换 和 忽略项 ***");
            // 假设 10 代表北京
            str = LocalizationTools.ToString(p1, new { CityId = "北京" }, nameof(Person.Password));
            Console.WriteLine(str);
            Console.WriteLine();
            Console.WriteLine();

            var p2 = new Person
            {
                Id = 1,
                Name = "王小锤",
                Password = "987654321",
                CityId = 28,
            };
            Console.WriteLine("********* Compare(): 带有忽略项 *********");
            var compareResult = LocalizationTools.Compare(p1, p2, new[] { nameof(Person.Password) });
            Console.WriteLine(compareResult.GetDifferenceMsg());
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("********** Compare(): 使用替换 **********");
            // 假设 10 代表北京, 28 代表成都
            compareResult.UpdateDifferentProperty(nameof(Person.CityId), "北京", "成都");
            Console.WriteLine(compareResult.GetDifferenceMsg());
            Console.WriteLine();
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
        /// 出生日期
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
        [ToStringReplacePair(null, "未设置", true, "是", false, "否")]
        public bool? IsAvailable { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 配偶
        /// </summary>
        [ToStringReplacePair(null, "没有配偶")]
        public Person Spouse { get; set; }

        /// <summary>
        /// 爱好
        /// </summary>
        public List<string> Hobby { get; } = new List<string>();

    }

    /// <summary>
    /// 性别
    /// </summary>
    public enum SexType
    {
        /// <summary>
        /// 男性
        /// </summary>
        //[EnumAlias("男性")]
        Man = 0,
        /// <summary>
        /// 女性
        /// </summary>
        //[EnumAlias("女性")]
        Woman = 2,
        /// <summary>
        /// 人妖
        /// </summary>
        //[EnumAlias("人妖")]
        Ladyman = 3,
    }
}

```

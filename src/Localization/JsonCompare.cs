﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Localization2
{
    public enum ArrayDiffMode
    {
        Entire = 0,
        EvertyItem = 1
    }

    public class Operation
    {
        [JsonProperty(PropertyName = "属性")]
        public string Path { get; set; }

        [JsonProperty(PropertyName = "操作")]
        public string Op { get; set; }

        [JsonProperty(PropertyName = "原值", NullValueHandling = NullValueHandling.Ignore, Order = 10)]
        public virtual JToken From { get; set; }

        [JsonProperty(PropertyName = "新值", NullValueHandling = NullValueHandling.Ignore, Order = 11)]
        public virtual JToken To { get; set; }

        public static Operation Add(string path, JToken to)
        {
            return new Operation()
            {
                Path = path,
                Op = "新增",
                From = null,
                To = to
            };
        }

        public static Operation Remove(string path, JToken from)
        {
            return new Operation()
            {
                Path = path,
                Op = "删除",
                From = from,
                To = null
            };
        }

        public static Operation Replace(string path, JToken from, JToken to)
        {
            return new ReplaceOperation()
            {
                Path = path,
                Op = "修改",
                From = from,
                To = to
            };
        }

        private class ReplaceOperation : Operation
        {
            [JsonProperty(PropertyName = "原值", Order = 10)]
            public override JToken From { get; set; }

            [JsonProperty(PropertyName = "新值", Order = 11)]
            public override JToken To { get; set; }
        }
    }


    public class JsonCompare
    {
        private readonly ArrayDiffMode ArrayDiffMode;

        public JsonCompare(ArrayDiffMode rrayDiffMode = ArrayDiffMode.Entire)
        {
            ArrayDiffMode = rrayDiffMode;
        }

        public Operation[] Compare<T>(T from, T to, params string[] ignorePathes)
        {
            var fromJson = Lts.Default.Localization(from, null, ignorePathes: ignorePathes);
            var toJson = Lts.Default.Localization(to, null, ignorePathes: ignorePathes);
            return Compare(fromJson, toJson);
        }

        public Operation[] Compare(string fromJson, string toJson)
        {
            var from = JToken.Parse(fromJson);
            var to = JToken.Parse(toJson);
            return CompareInternal(from, to, "").ToArray();
        }

        public IEnumerable<Operation> CompareInternal(JToken from, JToken to, string path)
        {
            if (from.Type != to.Type)
            {
                yield return Operation.Replace(path, from, to);
                yield break;
            }

            if (from.Type == JTokenType.Array)
            {
                if (ArrayDiffMode == ArrayDiffMode.Entire)
                {
                    if (from.ToString() != to.ToString())
                    {
                        yield return Operation.Replace(path, from, to);
                    }
                }
                else
                {
                    foreach (var operation in ArrayCompare(from, to, path))
                        yield return operation;
                }
            }
            else if (from.Type == JTokenType.Object)
            {
                var fromProps = ((IDictionary<string, JToken>)from).OrderBy(o => o.Key);
                var toProps = ((IDictionary<string, JToken>)to).OrderBy(o => o.Key);

                foreach (var added in toProps.Except(fromProps, KeyValuePairEqualByKey.Instance))
                {
                    var newPath = Help.Combine(path, added.Key);
                    yield return Operation.Replace(newPath, null, added.Value);
                }

                foreach (var removed in fromProps.Except(toProps, KeyValuePairEqualByKey.Instance))
                {
                    var newPath = Help.Combine(path, removed.Key);
                    yield return Operation.Replace(newPath, removed.Value, null);
                }

                var matchedKeys = fromProps.Select(Kvp => Kvp.Key).Intersect(toProps.Select(kvp => kvp.Key));
                var matchedList = matchedKeys.Select(key => new { Key = key, From = from[key], To = to[key] }).ToList();

                foreach (var match in matchedList)
                {
                    var newPath = Help.Combine(path, match.Key);
                    foreach (var operation in CompareInternal(match.From, match.To, newPath))
                        yield return operation;
                }
            }
            else
            {
                if (from.ToString() == to.ToString())
                {
                    yield break;
                }
                else
                {
                    yield return Operation.Replace(path, from, to);
                }
            }
        }

        private IEnumerable<Operation> ArrayCompare(JToken from, JToken to, string path)
        {
            var fromArray = from.ToArray();
            var toArray = to.ToArray();
            var index = 0;
            while (index < fromArray.Length && index < toArray.Length)
            {
                foreach (var opeation in CompareInternal(fromArray[index], toArray[index], $"{path}[{index}]"))
                {
                    yield return opeation;
                }
                index++;
            }

            if (index < fromArray.Length)
            {
                while (index < fromArray.Length)
                {
                    yield return Operation.Remove($"{path}[{index}]", fromArray[index]);
                    index++;
                }
            }

            if (index < toArray.Length)
            {
                while (index < toArray.Length)
                {
                    yield return Operation.Add($"{path}[{index}]", toArray[index]);
                    index++;
                }
            }
        }


        private class KeyValuePairEqualByKey : IEqualityComparer<KeyValuePair<string, JToken>>
        {
            public static readonly KeyValuePairEqualByKey Instance = new KeyValuePairEqualByKey();
            public bool Equals(KeyValuePair<string, JToken> x, KeyValuePair<string, JToken> y)
            {
                return x.Key.Equals(y.Key);
            }

            public int GetHashCode(KeyValuePair<string, JToken> obj)
            {
                return obj.Key.GetHashCode();
            }
        }
    }
}

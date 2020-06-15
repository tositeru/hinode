using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// 
    /// <seealso cref="Hinode.Tests.CSharp.TestJsonSerializer"/>
    /// </summary>
    public class JsonSerializer : ISerializer
    {
        readonly char[] SPACE_CHARS = { ' ', '\t', '\r', '\n' };
        readonly Regex IS_KEYWORD_CHAR_REGEX = new Regex(@"\w", RegexOptions.IgnoreCase);
        readonly Regex IS_NUMBER_CHAR_REGEX = new Regex(@"[-\de\.]", RegexOptions.IgnoreCase);

        public JsonSerializer(IInstanceCreator instanceCreator = null)
            : base(instanceCreator)
        {
        }

        public string Serialize(object obj)
        {
            using (var writer = new StringWriter())
            {
                Serialize(writer, obj);
                return writer.ToString();
            }
        }

        public object Deserialize(string json, System.Type type)
        {
            using (var reader = new StringReader(json))
            {
                return Deserialize(reader, type);
            }
        }
        public T Deserialize<T>(string json)
        {
            return (T)Deserialize(json, typeof(T));
        }

        protected override void WriteTo(TextWriter stream, SerializationInfo srcInfo)
        {
            stream.Write('{');
            bool appendComma = false;
            foreach (var entry in srcInfo.GetEnumerable().Where(_e => _e.Value != null))
            {
                if (appendComma) stream.Write(",");
                stream.Write($"\"{entry.Name}\":");
                WriteValue(stream, entry.Value, entry.ObjectType);

                appendComma = true;
            }
            stream.Write('}');
        }

        void WriteValue(TextWriter stream, object value, System.Type type)
        {
            if (type.IsPrimitive || type.IsNumeric())
            {
                stream.Write($"{value}");
            }
            else if (type.Equals(typeof(string)))
            {
                stream.Write($"\"{value}\"");
            }
            else if (type.IsSubclassOf(typeof(System.Enum)))
            {
                stream.Write($"{(int)value}");
            }
            else if (type.IsArrayOrList())
            {
                Assert.IsTrue(value is IEnumerable);
                var enumerable = value as IEnumerable;

                stream.Write("[");
                bool doAppendCamma = false;
                foreach (var element in enumerable)
                {
                    if (doAppendCamma) stream.Write(",");
                    doAppendCamma = true;
                    WriteValue(stream, element, element.GetType());
                }
                stream.Write("]");
            }
            else
            {
                Serialize(stream, value);
            }
        }

        protected override void ReadTo(TextReader stream, SerializationInfo outInfo, IReadOnlyDictionary<string, System.Type> keyAndTypeDict)
        {
            if (!stream.SkipTo(SPACE_CHARS)) return;
            if (!stream.MoveTo('{')) return;
            if (!stream.SkipTo(SPACE_CHARS)) return;

            while (!stream.IsMatchPeek('}'))
            {
                // read key
                var key = ReadString(stream);
                Assert.IsTrue(stream.SkipTo(SPACE_CHARS));

                System.Type valueType;
                if (outInfo.ObjectType.Equals(DEFAULT_TYPE))
                {
                    valueType = DEFAULT_TYPE;
                }
                else
                {
                    valueType = (keyAndTypeDict != null && keyAndTypeDict.ContainsKey(key))
                        ? keyAndTypeDict[key]
                        : null;
                    if (valueType == null)
                    {
                        var f = outInfo.ObjectType.GetFieldInHierarchy(key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        Assert.IsNotNull(f, $"Don't found '{key}' field...");
                        valueType = f.FieldType;
                    }
                }

                // move to next of ':'
                Assert.IsTrue(SkipNextColon(stream));
                Assert.IsTrue(stream.SkipTo(SPACE_CHARS));

                //Debug.Log($"debug-- key={key}; next={(char)stream.Peek()},type={outInfo.FullTypeName}");
                // read value
                var value = ReadValue(stream, valueType);
                //Debug.Log($"debug-- key={key}, value={value}; next={(char)stream.Peek()},type={outInfo.FullTypeName}");

                outInfo.AddValue(key, value);
                Assert.IsTrue(stream.SkipTo(SPACE_CHARS));

                if (SkipNextCamma(stream))
                {
                    Assert.IsTrue(stream.SkipTo(SPACE_CHARS));
                }
            }
            stream.Read(); // skip '}' if not end of string
        }

        object ReadValue(TextReader stream, System.Type valueType)
        {
            if (valueType.IsEnum)
            {
                return ReadEnum(stream, valueType);
            }

            switch (stream.Peek())
            {
                case '{':// object
                    Assert.IsTrue(valueType.IsClass || !valueType.IsPrimitive || !valueType.IsEnum);
                    return Deserialize(stream, valueType);
                case '[':// array
                    Assert.IsTrue(valueType.IsArrayOrList(), $"Be not array this type({valueType.FullName})...");
                    return ReadArray(stream, valueType);
                case '"':
                case '\'': //string
                    Assert.AreEqual(typeof(string), valueType);
                    return ReadString(stream);
                case 't':
                case 'T':
                case 'f':
                case 'F'://bool
                    Assert.AreEqual(typeof(bool), valueType);
                    return ReadBool(stream);
                default: //number
                    var numberValue = ReadNumber(stream, valueType);
                    Assert.IsTrue(numberValue.GetType().IsNumeric());
                    return numberValue;
            }
        }

        Dictionary<System.Type, RefCache> _listTypeRefCache = new Dictionary<System.Type, RefCache>();
        RefCache GetListTypeCache(System.Type type)
        {
            if (_listTypeRefCache.ContainsKey(type)) return _listTypeRefCache[type];

            var baseListType = typeof(List<>);
            var listType = baseListType.MakeGenericType(new System.Type[] { type });
            var refCache = new RefCache(listType);
            refCache.FindAndCacheConstructor("ctor", true, new System.Type[] { });
            refCache.FindAndCacheMethod("Add", true, new System.Type[] { type });
            refCache.FindAndCacheMethod("ToArray", true, new System.Type[] { });
            _listTypeRefCache.Add(type, refCache);
            return refCache;
        }

        /// <summary>
        ///
        /// <seealso cref="TypeExtensions.GetArrayElementType(System.Type)"></seealso>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="arrayType"></param>
        /// <returns>List&lft;arrayType.GetArrayElementType() or object&rht;</returns>
        object ReadArray(TextReader stream, System.Type arrayType)
        {
            Assert.IsTrue(arrayType.IsArrayOrList(), $"arrayType({arrayType.FullName}) is not Array Type...");

            Assert.IsTrue(stream.IsMatchPeek('['));
            stream.Read();
            Assert.IsTrue(stream.SkipTo(SPACE_CHARS));

            var elementType = arrayType.GetArrayElementType();
            var refCache = GetListTypeCache(elementType);
            var addMethodInfo = refCache.GetCachedMethodInfo("Add");

            var tmpList = refCache.CreateInstanceWithCache("ctor", null);
            while (!stream.IsMatchPeek(']'))
            {
                var value = ReadValue(stream, elementType);
                addMethodInfo.Invoke(tmpList, new object[] { value });

                Assert.IsTrue(stream.SkipTo(SPACE_CHARS));
                if (SkipNextCamma(stream))
                {
                    Assert.IsTrue(stream.SkipTo(SPACE_CHARS));
                }
            }
            stream.Read(); // skip ']' if not end of string

            //instanceをarrayTypeへキャストする
            if (arrayType.IsArray)
            {
                var count = (tmpList as IList).Count;
                var arrayCtor = arrayType.GetConstructor(new System.Type[] { typeof(int) });
                var arrayInst = arrayCtor.Invoke(new object[] { count });

                var toArrayMethod = refCache.GetCachedMethodInfo("ToArray");
                var srcArray = (System.Array)toArrayMethod.Invoke(tmpList, new object[] { });
                System.Array.Copy(srcArray, (System.Array)arrayInst, count);
                return arrayInst;
            }
            else
            {
                //List<>の時
                return tmpList;
            }
        }

        string ReadString(TextReader stream)
        {
            Assert.IsTrue(stream.IsMatchPeek('"', '\''));
            var delimeter = (char)stream.Read();

            var str = "";
            while (!stream.IsMatchPeek(delimeter))
            {
                var ch = (char)stream.Read();
                str += ch;
                if ('\\' == ch)
                {
                    str += stream.Read();
                }
            }
            stream.Read();
            return str;
        }

        string ReadKeyword(TextReader stream, Regex usedRegex = null)
        {
            if (usedRegex == null) usedRegex = IS_KEYWORD_CHAR_REGEX;
            Assert.IsTrue(stream.IsMatchPeek(usedRegex));

            var str = "";
            while (stream.IsMatchPeek(usedRegex))
            {
                var ch = (char)stream.Read();
                str += ch;
            }
            return str;
        }

        bool ReadBool(TextReader stream)
        {
            switch (char.ToLower((char)stream.Peek()))
            {
                case 't':
                    var keyword = ReadKeyword(stream).ToLower();
                    return keyword == "true" || keyword == "t";
                case 'f':
                    keyword = ReadKeyword(stream).ToLower();
                    return !(keyword == "false" || keyword == "f");
                default:
                    Assert.IsFalse(true, $"parse error!! not be Boolean Keyword");
                    return false;
            }
        }

        object ReadNumber(TextReader stream, System.Type type = null)
        {
            var keyword = ReadKeyword(stream, IS_NUMBER_CHAR_REGEX);

            if (type == null)
            {
                if (int.TryParse(keyword, out var integer))
                {
                    return integer;
                }
                else if (double.TryParse(keyword, out var number))
                {
                    return number;
                }
                else
                {
                    Assert.IsFalse(true, $"keyword({keyword}) not be number string...");
                    return null;
                }
            }
            else
            {
                return type.ParseToNumber(keyword);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        object ReadEnum(TextReader stream, System.Type type)
        {
            Assert.IsTrue(type.IsEnum);
            if (stream.Peek() == '"')
            {
                var str = ReadString(stream);
                if (type.GetCustomAttribute<System.FlagsAttribute>() != null)
                {
                    var tokens = str.Replace(" ", "").Replace("　", "").Split(',', '|');
                    var v = 0;
                    foreach (var t in tokens)
                    {
                        v |= (int)System.Enum.Parse(type, t);
                    }
                    return System.Enum.ToObject(type, v);
                }
                else
                {
                    return System.Enum.Parse(type, str);
                }
            }
            else
            {
                //数値の時
                var num = ReadNumber(stream, type);
                Assert.IsNotNull(num);
                Assert.IsTrue(num.GetType().IsNumeric());
                return System.Enum.ToObject(type, num);
            }
        }

        bool SkipNextColon(TextReader stream)
        {
            if ((char)stream.Peek() == ':')
            {
                stream.Read();
                return true;
            }
            return false;
        }

        bool SkipNextCamma(TextReader stream)
        {
            if ((char)stream.Peek() == ',')
            {
                stream.Read();
                return true;
            }
            return false;
        }
    }
}

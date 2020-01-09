using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp
{
    public class TestJsonSerializer
    {
        [System.Serializable]
        class TestBasicClass
        {
#pragma warning disable CS0649
            public int v1 = 1;
            [SerializeField] string v2 = "hoge";
            public TestBasicSubClass sub = new TestBasicSubClass { v1 = -1, v2 = "v2" };
            public TestBasicSubClass2 sub2;
            public int[] arr = { 12, 34 };
            public List<string> list = new List<string>{ "first", "second" };
#pragma warning restore CS0649

            public void AssertAreSame(TestBasicClass other)
            {
                Assert.AreEqual(this.v1, other.v1);
                Assert.AreEqual(this.v2, other.v2);
                sub.AssertAreSame(other.sub);
                sub2.AssertAreSame(other.sub2);
                Assert.AreEqual(arr.Length, other.arr.Length);
                foreach (var pair in arr.Zip(other.arr, (_t, _o) => (_t, _o)))
                {
                    Assert.AreEqual(pair._t, pair._o);
                }
            }
        }

        [System.Serializable]
        class TestBasicSubClass
        {
#pragma warning disable CS0649
            public int v1 = 111;
            public string v2 = "msg";
#pragma warning restore CS0649
            public void AssertAreSame(TestBasicSubClass other)
            {
                Assert.AreEqual(this.v1, other.v1);
                Assert.AreEqual(this.v2, other.v2);
            }
        }

        class TestBasicSubClass2 : ISerializable
        {
#pragma warning disable CS0649
            public int v1 = 111;
            public string v2 = "msg";
#pragma warning restore CS0649
            public void AssertAreSame(TestBasicSubClass2 other)
            {
                Assert.AreEqual(this.v1, other.v1);
                Assert.AreEqual(this.v2, other.v2);
            }

            public TestBasicSubClass2() { }
            public TestBasicSubClass2(SerializationInfo info, StreamingContext context)
            {
                v1 = info.GetInt32("v1");
                v2 = info.GetString("v2");
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("v1", v1);
                info.AddValue("v2", v2);
            }
        }

        // A Test behaves as an ordinary method
        [Test, Description("TextWriter/TextReaderのテスト")]
        public void BasicPasses()
        {
            var src = new TestBasicClass()
            {
                v1 = 1,
                sub = new TestBasicSubClass { v1 = -1, v2 = "v2 in sub" },
                sub2 = new TestBasicSubClass2 { v1 = 111, v2 = "vvvidv" },
                arr = new int[] { -1, -2, -3 },
                list = new List<string> { "App", "Ora" },
            };

            var serializer = new JsonSerializer();
            var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, src);
            var json = stringWriter.ToString();

            //Debug.Log($"debug-- '{json}'");
            var reader = new StringReader(json);
            var dest = serializer.Deserialize(reader, typeof(TestBasicClass)) as TestBasicClass;

            src.AssertAreSame(dest);

            var stringWriter2 = new StringWriter();
            serializer.Serialize(stringWriter2, dest);
            Assert.AreEqual(json, stringWriter2.ToString());
        }

        [System.Serializable]
        class EnumPassesClass
        {
            public enum Enum { A, B, C }
            [System.Flags]
            public enum Flags { Apple=0x1, Orange=0x2, Grape=0x4 }

            public enum EmptyEnum { }

#pragma warning disable CS0649
            public Enum e = Enum.A;
            public Flags f = 0;
            public EmptyEnum emptyEnum;
#pragma warning restore CS0649
        }

        [Test]
        public void EnumPasses()
        {
            var src = new EnumPassesClass()
            {
                e = EnumPassesClass.Enum.B,
                f = EnumPassesClass.Flags.Apple | EnumPassesClass.Flags.Grape,
            };

            var serializer = new JsonSerializer();
            {
                var writer = new StringWriter();
                serializer.Serialize(writer, src);
                var json = writer.ToString();

                //Debug.Log($"json -> {json}");

                var reader = new StringReader(json);
                var dest = serializer.Deserialize(reader, typeof(EnumPassesClass)) as EnumPassesClass;

                Assert.AreEqual(src.e, dest.e);
                Assert.AreEqual(src.f, dest.f);

                var writer2 = new StringWriter();
                serializer.Serialize(writer2, dest);
                Assert.AreEqual(json, writer2.ToString());
            }

            {
                var strEnumJson = "{\"e\":\"C\",\"f\":\"Apple, 　Orange\",\"emptyEnum\":0}";
                var reader2 = new StringReader(strEnumJson);
                var dest2 = serializer.Deserialize(reader2, typeof(EnumPassesClass)) as EnumPassesClass;
                Assert.AreEqual(EnumPassesClass.Enum.C, dest2.e);
                Assert.AreEqual(EnumPassesClass.Flags.Apple| EnumPassesClass.Flags.Orange, dest2.f);
            }
        }
    }
}

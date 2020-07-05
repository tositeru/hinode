using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Hinode.Serialization;

namespace Hinode.Tests.CSharp.Serialization
{
    /// <summary>
    /// <seealso cref="ISerializer"/>
    /// </summary>
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

        /// <summary>
        /// <seealso cref="JsonSerializer.Serialize(object)"/>
        /// <seealso cref="JsonSerializer.Deserialize(string, System.Type)"/>
        /// </summary>
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

        /// <summary>
        /// <seealso cref="JsonSerializer.Serialize(object)"/>
        /// <seealso cref="JsonSerializer.Deserialize(string, System.Type)"/>
        /// </summary>
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

        [System.Serializable]
        class BoolPassesClass
        {
#pragma warning disable CS0649
            public bool b1 = true;
            public bool b2 = false;
#pragma warning restore CS0649
        }

        /// <summary>
        /// <seealso cref="JsonSerializer.Serialize(object)"/>
        /// <seealso cref="JsonSerializer.Deserialize(string, System.Type)"/>
        /// </summary>
        [Test]
        public void BoolPasses()
        {
            var src = new BoolPassesClass();

            var serializer = new JsonSerializer();
            {
                var writer = new StringWriter();
                serializer.Serialize(writer, src);
                var json = writer.ToString();

                Debug.Log($"json -> {json}");

                var reader = new StringReader(json);
                var dest = serializer.Deserialize(reader, typeof(BoolPassesClass)) as BoolPassesClass;

                Assert.AreEqual(src.b1, dest.b1);
                Assert.AreEqual(src.b2, dest.b2);

                var writer2 = new StringWriter();
                serializer.Serialize(writer2, dest);
                Assert.AreEqual(json, writer2.ToString());
            }
        }

        [System.Serializable]
        class InstanceCreatorPassesClass
        {
            public Vector2 vec2;
            public Vector2Int vec2Int;
            public Vector3 vec3;
            public Vector3Int vec3Int;
            public Vector4 vec4;
            public Quaternion quaternion;
        }

        /// <summary>
        /// <seealso cref="ISerializer.IInstanceCreator"/>
        /// <seealso cref="JsonSerializer.Serialize(object)"/>
        /// <seealso cref="JsonSerializer.Deserialize(string, System.Type)"/>
        /// </summary>
        [Test, Description("ISerializer#DefaultInstanceCreatorのテスト")]
        public void InstanceCreatorPasses()
        {
            var t = typeof(Vector2);
            foreach (var ctor in t.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                Debug.Log($"debug Vec2 ctor => {ctor.Name} params={ctor.GetParameters().Length}");
            }
            //TODO シリアライズに失敗したケースでエラーを出すようにする
            var inst = new InstanceCreatorPassesClass()
            {
                vec2 = new Vector2(1.234f, 5f),
                vec2Int = new Vector2Int(1, 6),
                vec3 = new Vector3(2.3f, 5f, 11f),
                vec3Int = new Vector3Int(3, 4, 5),
                vec4 = new Vector4(-1.234f, -5f, -7f, -9f),
                quaternion = new Quaternion(1f, 5f, 2f, 6f),
            };
            var serializer = new JsonSerializer();
            {
                var writer = new StringWriter();
                serializer.Serialize(writer, inst);
                var json = writer.ToString();

                Debug.Log($"json -> {json}");

                var reader = new StringReader(json);
                var dest = serializer.Deserialize(reader, typeof(InstanceCreatorPassesClass)) as InstanceCreatorPassesClass;

                Assert.AreEqual(inst.vec2.x, dest.vec2.x);
                Assert.AreEqual(inst.vec2.y, dest.vec2.y);
                Assert.AreEqual(inst.vec2Int.x, dest.vec2Int.x);
                Assert.AreEqual(inst.vec2Int.y, dest.vec2Int.y);
                Assert.AreEqual(inst.vec3.x, dest.vec3.x);
                Assert.AreEqual(inst.vec3.y, dest.vec3.y);
                Assert.AreEqual(inst.vec3.z, dest.vec3.z);
                Assert.AreEqual(inst.vec3Int.x, dest.vec3Int.x);
                Assert.AreEqual(inst.vec3Int.y, dest.vec3Int.y);
                Assert.AreEqual(inst.vec3Int.z, dest.vec3Int.z);
                Assert.AreEqual(inst.vec4.x, dest.vec4.x);
                Assert.AreEqual(inst.vec4.y, dest.vec4.y);
                Assert.AreEqual(inst.vec4.z, dest.vec4.z);
                Assert.AreEqual(inst.vec4.w, dest.vec4.w);
                Assert.AreEqual(inst.quaternion.x, dest.quaternion.x);
                Assert.AreEqual(inst.quaternion.y, dest.quaternion.y);
                Assert.AreEqual(inst.quaternion.z, dest.quaternion.z);
                Assert.AreEqual(inst.quaternion.w, dest.quaternion.w);

                var writer2 = new StringWriter();
                serializer.Serialize(writer2, dest);
                Assert.AreEqual(json, writer2.ToString());
            }
        }

        [System.Serializable]
        class NumericTypePassesClass
        {
            public sbyte _sbyte;
            public byte _byte;
            public short _short;
            public ushort _ushort;
            public int _int;
            public uint _uint;
            public long _long;
            public ulong _ulong;
            public System.Numerics.BigInteger _bigInt;
            public float _float;
            public double _double;
            public decimal _decimal;
        }

        /// <summary>
        /// 使用している数値型へ正確にDeserializeしているか確認するためのテスト
        /// <seealso cref="JsonSerializer.Serialize(object)"/>
        /// <seealso cref="JsonSerializer.Deserialize(string, System.Type)"/>
        /// </summary>
        [Test, Description("数値型の変換テスト")]
        public void NumericTypeDeserializePasses()
        {
            var src = new NumericTypePassesClass
            {
                _sbyte = -1,
                _byte = 1,
                _short = -2,
                _ushort = 2,
                _int = -3,
                _uint = 3,
                _long = -4,
                _ulong = 4,
                _bigInt = 100000,
                _float = 1.23f,
                _double = 2.345,
                _decimal = 1.2344567M,
            };

            var serializer = new JsonSerializer();
            {
                var writer = new StringWriter();
                serializer.Serialize(writer, src);
                var json = writer.ToString();

                Debug.Log($"json -> {json}");

                var reader = new StringReader(json);
                var dest = serializer.Deserialize(reader, typeof(NumericTypePassesClass)) as NumericTypePassesClass;

                Assert.AreEqual(src._sbyte, dest._sbyte);
                Assert.AreEqual(src._byte, dest._byte);
                Assert.AreEqual(src._int, dest._int);
                Assert.AreEqual(src._uint, dest._uint);
                Assert.AreEqual(src._long, dest._long);
                Assert.AreEqual(src._ulong, dest._ulong);
                Assert.AreEqual(src._bigInt, dest._bigInt);
                Assert.AreEqual(src._float, dest._float);
                Assert.AreEqual(src._double, dest._double);
                Assert.AreEqual(src._decimal, dest._decimal);

                var writer2 = new StringWriter();
                serializer.Serialize(writer2, dest);
                Assert.AreEqual(json, writer2.ToString());
            }

        }
    }
}

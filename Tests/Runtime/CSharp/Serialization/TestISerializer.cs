using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Hinode.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp.Serialization
{
    /// <summary>
    /// <seealso cref="ISerializer"/>
    /// </summary>
    public class TestISerializer
    {
        [System.Serializable]
        class TestClass
        {
            public int field = 0;
        }

        /// <summary>
        /// <seealso cref="ISerializerExtensions.Serialize(ISerializer, object)"/>
        /// </summary>
        [Test]
        public void SerializeExtensionPasses()
        {
            var serializer = new JsonSerializer();
            var inst = new TestClass();
            var json = serializer.Serialize(inst);
            Debug.Log($"debug -- json={json}");

            using (var stream = new StringWriter())
            {
                serializer.Serialize(stream, inst);
                Assert.AreEqual(json, stream.ToString());
            }
        }

        /// <summary>
        /// <seealso cref="ISerializerExtensions.Deserialize{T}(ISerializer, string)(ISerializer, object)"/>
        /// </summary>
        [Test]
        public void DeserializeExtensionWithTemplateArgumentsPasses()
        {
            var serializer = new JsonSerializer();
            var inst = new TestClass();
            var json = serializer.Serialize(inst);
            Debug.Log($"debug -- json={json}");

            var dest = serializer.Deserialize<TestClass>(json);

            using (var stream = new StringReader(json))
            {
                serializer.Deserialize(stream, typeof(TestClass));
                Assert.AreEqual(inst.field, dest.field);
            }
        }

        /// <summary>
        /// <seealso cref="ISerializerExtensions.Deserialize(ISerializer, string, System.Type)"/>
        /// </summary>
        [Test]
        public void DeserializeExtensionWithStringAndTypePasses()
        {
            var serializer = new JsonSerializer();
            var inst = new TestClass();
            var json = serializer.Serialize(inst);
            Debug.Log($"debug -- json={json}");

            var dest = serializer.Deserialize(json, typeof(TestClass))
                as TestClass;
            using (var stream = new StringReader(json))
            {
                var correct = serializer.Deserialize(stream, typeof(TestClass))
                    as TestClass;
                Assert.AreEqual(correct.field, dest.field);
            }
        }
    }
}

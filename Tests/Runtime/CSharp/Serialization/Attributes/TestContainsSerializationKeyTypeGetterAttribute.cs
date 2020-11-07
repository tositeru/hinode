using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Serialization;
using System.Linq;
using System.Reflection;

namespace Hinode.Tests.CSharp.Serialization
{
    /// <summary>
    /// <seealso cref="ContainsSerializationKeyTypeGetterAttribute"/>
    /// <seealso cref="SerializationKeyTypeGetterAttribute"/>
    /// </summary>
    public class TestContainsSerializationKeyTypeGetterAttribute
    {
        [ContainsSerializationKeyTypeGetter(typeof(TestClass))]
        class TestClass
        {
            static Dictionary<string, System.Type> _dict = new Dictionary<string, System.Type>()
            {
                { "int", typeof(int) },
                { "Apple", typeof(string) }
            };

            [SerializationKeyTypeGetter]
            static System.Type GetKeyType(string key)
            {
                return _dict.ContainsKey(key)
                    ? _dict[key]
                    : null;
            }
        }

        [ContainsSerializationKeyTypeGetter(typeof(TestSubClass))]
        class TestSubClass : TestClass
        {
            static Dictionary<string, System.Type> _dict = new Dictionary<string, System.Type>()
            {
                { "Orange", typeof(string) }
            };

            [SerializationKeyTypeGetter]
            static System.Type GetKeyType(string key)
            {
                return _dict.ContainsKey(key)
                    ? _dict[key]
                    : null;
            }
        }

        [Test]
        public void BasicUsagePasses()
        {
            var attr = typeof(TestClass).GetCustomAttributes(true)
                .OfType<ContainsSerializationKeyTypeGetterAttribute>()
                .First();

            var keyTypeGetter = attr.CreateKeyTypeGetter(typeof(TestClass));

            Assert.AreEqual(typeof(int), keyTypeGetter.Get("int"));
            Assert.AreEqual(typeof(string), keyTypeGetter.Get("Apple"));

            Assert.IsNull(keyTypeGetter.Get("__invalid"));
        }

        [Test]
        public void SubClassPasses()
        {
            var attr = typeof(TestSubClass).GetCustomAttributes(true)
                .OfType<ContainsSerializationKeyTypeGetterAttribute>()
                .First();

            var keyTypeGetter = attr.CreateKeyTypeGetter(typeof(TestSubClass));

            Assert.AreEqual(typeof(string), keyTypeGetter.Get("Orange"));
            Debug.Log($"Success to KeyTypeGetter in TestSubClass!");

            Assert.AreEqual(typeof(int), keyTypeGetter.Get("int"));
            Assert.AreEqual(typeof(string), keyTypeGetter.Get("Apple"));
            Debug.Log($"Success to KeyTypeGetter in TestClass!");

            Assert.IsNull(keyTypeGetter.Get("__invalid"));
            Debug.Log($"Success to Invalid Key!");
        }

        [ContainsSerializationKeyTypeGetter(typeof(TestClass))]
        class InvalidTestClass
        {
        }

        [Test]
        public void NotSerializationKeyTypeGetterFail()
        {
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                var attr = typeof(InvalidTestClass).GetCustomAttributes(true)
                    .OfType<ContainsSerializationKeyTypeGetterAttribute>()
                    .First();

                var keyTypeGetter = attr.CreateKeyTypeGetter(typeof(InvalidTestClass));
            });
        }

        [ContainsSerializationKeyTypeGetter(typeof(ContainsMultipleSerializationKeyTypeTestClass))]
        class ContainsMultipleSerializationKeyTypeTestClass
        {
            [SerializationKeyTypeGetter]
            static System.Type ValidMethod(string key) => null;
            [SerializationKeyTypeGetter]
            static System.Type ValidMethods(string key) => null;

        }

        [Test]
        public void ContainsMultipleSerializationKeyTypeGetterFail()
        {
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => {
                var attr = typeof(ContainsMultipleSerializationKeyTypeTestClass).GetCustomAttributes(true)
                    .OfType<ContainsSerializationKeyTypeGetterAttribute>()
                    .First();

                var keyTypeGetter = attr.CreateKeyTypeGetter(typeof(TestSubClass));
            });
        }

        class SerializationKeyTypeGetterTestClass
        {
            [SerializationKeyTypeGetter]
            static System.Type ValidMethod(string key)
            {
                return null;
            }

            [SerializationKeyTypeGetter]
            static void InvalidReturnMethod(string key)
            { }

            [SerializationKeyTypeGetter]
            static System.Type InvalidArgumentMethod(bool key)
            { return null; }

            [SerializationKeyTypeGetter]
            static System.Type InvalidArgumentMethod2()
            { return null; }

            [SerializationKeyTypeGetter]
            System.Type InvalidInstanceMethod(string key)
            { return null; }

        }

        [Test]
        public void IsValidSerializationKeyTypeGetterMethodPasses()
        {
            var testData = new (string methodName, bool isValid)[]
            {
                ("ValidMethod", true),
                ("InvalidReturnMethod", false),
                ("InvalidArgumentMethod", false),
                ("InvalidArgumentMethod2", false),
                ("InvalidInstanceMethod", false),
            };

            foreach(var data in testData)
            {
                var errorMessage = $"Fail... methodName={data.methodName}, isValid={data.isValid}";
                var methodInfo = typeof(SerializationKeyTypeGetterTestClass)
                    .GetMethod(data.methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                Assert.IsNotNull(methodInfo, errorMessage);

                Assert.AreEqual(data.isValid, SerializationKeyTypeGetterAttribute.IsValid(methodInfo), errorMessage);
            }
        }
    }
}

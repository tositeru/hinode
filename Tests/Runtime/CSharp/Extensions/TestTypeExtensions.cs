using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp
{
    public class TestTypeExtensions
    {
        class GetFieldInHierarchyBaseClass : ScriptableObject
        {
#pragma warning disable CS0649, CS0414
            int value = 1;
#pragma warning restore CS0649, CS0414
        }
        class GetFieldInHierarchySubClass : GetFieldInHierarchyBaseClass
        {
#pragma warning disable CS0649, CS0414
            public int apple = 10;
#pragma warning restore CS0649, CS0414
        }

        // A Test behaves as an ordinary method
        [Test]
        public void GetFieldInHierarchyPasses()
        {
            Assert.AreEqual(typeof(GetFieldInHierarchyBaseClass).GetField("value", BindingFlags.NonPublic),
                typeof(GetFieldInHierarchySubClass).GetFieldInHierarchy("value", BindingFlags.NonPublic));
            // Use the Assert class to test conditions
        }

        class InheritedList : List<int> {}

        [Test]
        public void IsArrayOrListPasses()
        {
            Assert.IsTrue(typeof(int[]).IsArrayOrList());
            Assert.IsTrue(typeof(System.Array).IsArrayOrList());
            Assert.IsTrue(typeof(ArrayList).IsArrayOrList());
            Assert.IsTrue(typeof(List<>).IsArrayOrList());
            Assert.IsTrue(typeof(List<int>).IsArrayOrList());
            Assert.IsTrue(typeof(InheritedList).IsArrayOrList());

            Assert.IsFalse(typeof(int).IsArrayOrList());
            Assert.IsFalse(typeof(System.Enum).IsArrayOrList());
            Assert.IsFalse(typeof(Dictionary<string, int>).IsArrayOrList());
        }

        [Test]
        public void GetArrayElementTypePasses()
        {
            Assert.AreEqual(typeof(int), typeof(int[]).GetArrayElementType());
            Assert.AreEqual(typeof(object), typeof(System.Array).GetArrayElementType());
            Assert.AreEqual(typeof(object), typeof(ArrayList).GetArrayElementType());
            Assert.AreEqual(typeof(List<>).GetGenericArguments()[0], typeof(List<>).GetArrayElementType());
            Assert.AreEqual(typeof(string), typeof(List<string>).GetArrayElementType());
            Assert.AreEqual(typeof(int), typeof(InheritedList).GetArrayElementType());

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => typeof(int).GetArrayElementType());
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => typeof(System.Enum).GetArrayElementType());
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => typeof(Dictionary<string, int>).GetArrayElementType());
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Hinode.Editors;
using System.Reflection;
using System.Linq;

namespace Hinode.Tests.Editors
{
    public class TestSerializedObjectExtensions : TestBase
    {
        [System.Serializable]
        class TestClass : ScriptableObject
        {
#pragma warning disable CS0649
            public int value;
            public TestSubClass sub = new TestSubClass(0);
#pragma warning restore CS0649
        }

        [System.Serializable]
        private class TestSubClass
        {
#pragma warning disable CS0649
            [SerializeField, Min(0)] float v;
#pragma warning restore CS0649
            public TestSubClass(float v) { this.v = v; }

            public override bool Equals(object obj)
            {
                if(obj is TestSubClass)
                {
                    var other = obj as TestSubClass;
                    return Mathf.Abs(this.v - other.v) < float.Epsilon;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return v.GetHashCode();
            }
        }

        [Test]
        public void CopyPasses()
        {
            var obj = ScriptableObject.CreateInstance<TestClass>();
            obj.value = 100;
            obj.sub = new TestSubClass(100);
            var SO = new SerializedObject(obj);

            var destObj = ScriptableObject.CreateInstance<TestClass>();
            var dest = new SerializedObject(destObj);
            SO.Copy(dest);

            Assert.AreEqual(100, destObj.value);
            Assert.AreEqual(100, dest.FindProperty("value").intValue);

            Assert.AreEqual(obj.sub, destObj.sub);
            Assert.AreEqual(SO.FindProperty("sub").FindPropertyRelative("v").floatValue,
                dest.FindProperty("sub").FindPropertyRelative("v").floatValue);
        }
    }
}

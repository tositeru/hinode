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
    }
}

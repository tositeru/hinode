using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Runtime.Extensions
{
    public class TestMonoBehaviourExtensions
    {
        class AssertObjectReferenceTest : MonoBehaviour
        {
#pragma warning disable CS0649
            public int valueType;
            [SerializeField] float valueType2;
            [SerializeField] string valueType3;
            [SerializeField] GameObject obj;
            public GameObject obj2;
            public SerializableObj serializableObj;
#pragma warning restore CS0649

            public GameObject Obj { get => obj; set => obj = value; }
        }

        [System.Serializable]
        struct SerializableObj
        {
#pragma warning disable CS0649
            public GameObject obj;
            [SerializeField] GameObject obj2;
#pragma warning restore CS0649

            public GameObject Obj2 { get => obj2; set => obj2 = value; }
        }

        [Test]
        public void AssertObjectReferencePasses()
        {
            var obj = new GameObject("obj");

            var test = obj.AddComponent<AssertObjectReferenceTest>();

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => test.AssertObjectReference());

            test.obj2 = obj;
            test.Obj = new GameObject("inst1");
            test.serializableObj = new SerializableObj();
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => test.AssertObjectReference(), "Don't Check fields in serializeableObj ...");

            test.serializableObj.obj = obj;
            test.serializableObj.Obj2 = new GameObject("inst2");
            Assert.DoesNotThrow(() => test.AssertObjectReference());
        }
    }
}

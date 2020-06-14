using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Extensions
{
    /// <summary>
    /// <seealso cref="MonoBehaviourExtensions"/>
    /// </summary>
    public class TestMonoBehaviourExtensions : TestBase
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

        /// <summary>
        /// <seealso cref="MonoBehaviourExtensions.AssertObjectReference(MonoBehaviour, HashSet{object})"/>
        /// </summary>
        [UnityTest]
        public IEnumerator AssertObjectReferencePasses()
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
            yield return null;
        }

        class SafeStartCoroutineMonoBehaviour : MonoBehaviour
        {
            public int Value { get; set; }
            public bool IsFinishCoroutine { get; set; }

            public IEnumerator TestEnumerator(int value)
            {
                IsFinishCoroutine = false;
                yield return null;
                yield return null;

                Value = value;
                IsFinishCoroutine = true;
            }
        }

        /// <summary>
        /// <seealso cref="MonoBehaviourExtensions.SafeStartCoroutine(MonoBehaviour, ref Coroutine, IEnumerator)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator SafeStartCoroutinePasses()
        {
            var obj = new GameObject("obj");
            var behaviour = obj.AddComponent<SafeStartCoroutineMonoBehaviour>();
            Coroutine coroutine = null;
            behaviour.SafeStartCoroutine(ref coroutine, behaviour.TestEnumerator(100));
            Assert.IsNotNull(coroutine);

            behaviour.SafeStartCoroutine(ref coroutine, behaviour.TestEnumerator(200)); // <- Stop Prev Coroutine

            yield return null;
            yield return null;
            yield return null;

            Assert.AreEqual(200, behaviour.Value);
        }
    }
}

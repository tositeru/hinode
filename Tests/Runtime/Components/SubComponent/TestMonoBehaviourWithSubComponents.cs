using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Components.SubComponent
{
    /// <summary>
    /// <seealso cref="MonoBehaviourWithSubComponents{T}"/>
    /// </summary>
    public class TestMonoBehaviourWithSubComponents : TestBase
    {
        class TestMonoBehaviour : MonoBehaviourWithSubComponents<TestMonoBehaviour>
        {
            public Sub1 publicSub = new Sub1();

            [SerializeField] Sub1 _sub1 = new Sub1();
            public Sub1 SerializeFieldSub { get => _sub1; }

            [System.Serializable]
            public class Sub1 : ISubComponent<TestMonoBehaviour>
            {
                public int CallCounter = 0;

                public TestMonoBehaviour RootComponent { get; set; }

                public void Destroy() { CallCounter++; }
                public void Init() { CallCounter++; }
                public void UpdateUI() { CallCounter++; }
            }
        }

        /// <summary>
        /// <seealso cref="MonoBehaviourWithSubComponents{T}.Awake()"/>
        /// <seealso cref="MonoBehaviourWithSubComponents{T}.Start()"/>
        /// <seealso cref="MonoBehaviourWithSubComponents{T}.OnDestroy()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator BasicUsagePasses()
        {
            var inst = new GameObject().AddComponent<TestMonoBehaviour>();

            {
                Assert.AreSame(inst, inst.publicSub.RootComponent);
                Assert.AreEqual(1, inst.publicSub.CallCounter);

                Assert.AreSame(inst, inst.SerializeFieldSub.RootComponent);
                Assert.AreEqual(1, inst.SerializeFieldSub.CallCounter);
            }
            Debug.Log("Success to MonoBehaviour#Awake()!!!");

            {
                inst.publicSub.CallCounter = 0;
                inst.SerializeFieldSub.CallCounter = 0;
                yield return null;

                Assert.AreEqual(1, inst.publicSub.CallCounter);
                Assert.AreEqual(1, inst.SerializeFieldSub.CallCounter);
            }
            Debug.Log("Success to MonoBehaviour#Start()!!!");

            {
                inst.publicSub.CallCounter = 0;
                inst.SerializeFieldSub.CallCounter = 0;
                var cachePublicSub = inst.publicSub;
                var cacheSerializeFieldSub = inst.SerializeFieldSub;
                Object.Destroy(inst);
                yield return null;

                Assert.AreEqual(1, cachePublicSub.CallCounter);
                Assert.AreEqual(1, cacheSerializeFieldSub.CallCounter);
            }
            Debug.Log("Success to MonoBehaviour#Destroy()!!!");
        }
    }
}

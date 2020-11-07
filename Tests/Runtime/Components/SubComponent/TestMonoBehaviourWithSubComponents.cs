using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using UnityEngine.UI;
using System.Linq;

namespace Hinode.Tests.Components.SubComponent
{
    /// <summary>
    /// test case
    /// ## Basic Usage
    /// ## BindCallbacks
    /// <seealso cref="MonoBehaviourWithSubComponents{T}"/>
    /// </summary>
    public class TestMonoBehaviourWithSubComponents : TestBase
    {
        const int Order_BasicUsage = 0;
        const int Order_BindCallbacks = Order_BasicUsage + 100;

        #region Basic Usage
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
        [UnityTest, Order(Order_BasicUsage), Description("")]
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
        #endregion

        #region BindCallbacks
        class BindCallbacksTest : MonoBehaviourWithSubComponents<BindCallbacksTest>
        {
            public const string LABEL1 = "1";

            public int OnEventCounter { get; set; }
            [BindCallback(typeof(CallbacksBehaviour), "UnityEvent", Labels = new string[] { LABEL1 })]
            public void OnEvent()
            {
                OnEventCounter++;
            }

            public class CallbacksBehaviour : MonoBehaviour
            {
                public UnityEvent UnityEvent { get; } = new UnityEvent();

                public LabelObject LabelObj { get; set; }
                private void Awake()
                {
                    LabelObj = gameObject.AddComponent<LabelObject>();
                    LabelObj.Labels.Add(LABEL1);
                }
            }
        }

        [UnityTest, Order(Order_BindCallbacks), Description("")]
        public IEnumerator BindCallbacks_Passes()
        {
            var inst = new GameObject().AddComponent<BindCallbacksTest>();

            {//Valid case
                var callbacks = new GameObject().AddComponent<BindCallbacksTest.CallbacksBehaviour>();

                inst.BindCallbacks(callbacks);

                callbacks.UnityEvent.Invoke();

                Assert.AreEqual(1, inst.OnEventCounter);
            }

            {//Invalid case
                var invalidCallbacks = new GameObject().AddComponent<BindCallbacksTest.CallbacksBehaviour>();
                invalidCallbacks.LabelObj.Labels.Remove(BindCallbacksTest.LABEL1);

                inst.BindCallbacks(invalidCallbacks);

                inst.OnEventCounter = 0;
                invalidCallbacks.UnityEvent.Invoke();
                Assert.AreEqual(0, inst.OnEventCounter);
            }

            yield break;
        }
        #endregion
    }
}

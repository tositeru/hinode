using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Components.SubComponent
{
    /// <summary>
    /// <seealso cref="SubComponentManager{T}"/>
    /// </summary>
    public class TestSubComponentManager : TestBase
    {
        const int ORDER_CONSTRUCTOR = 0;

        [System.AttributeUsage(System.AttributeTargets.Field, Inherited=false, AllowMultiple = false)]
        public sealed class TestFieldAttribute : System.Attribute
            , ISubComponentAttribute
        {
            static TestFieldAttribute()
            {
                SubComponentAttributeManager.AddInitMethod<TestFieldAttribute>(MethodInit);
                SubComponentAttributeManager.AddDestroyMethod<TestFieldAttribute>(MethodDestroy);
                SubComponentAttributeManager.AddUpdateUIMethod<TestFieldAttribute>(MethodUpdateUI);
            }

            public static List<object> RecievedInstAtInit = new List<object>();
            public static int CallInitCounter = 0;
            public static void MethodInit(object inst)
            {
                RecievedInstAtInit.Add(inst);
                CallInitCounter++;
            }

            public static List<object> RecievedInstAtDestroy = new List<object>();
            public static int CallDestroyCounter = 0;
            public static void MethodDestroy(object inst)
            {
                RecievedInstAtDestroy.Add(inst);
                CallDestroyCounter++;
            }

            public static List<object> RecievedInstAtUpdateUI = new List<object>();
            public static int CallUpdateUICounter = 0;
            public static void MethodUpdateUI(object inst)
            {
                RecievedInstAtUpdateUI.Add(inst);
                CallUpdateUICounter++;
            }

            public TestFieldAttribute()
            {}
        }
        class TestComponent : MonoBehaviour
        {
            public class Sub1 : ISubComponent<TestComponent>
            {
                public int CallCounter = 0;

                public TestComponent RootComponent { get; set; }

                public void Destroy() { CallCounter++; }
                public void Init() { CallCounter++; }
                public void UpdateUI() { CallCounter++; }
            }

            [TestField]
            public Sub1 publicField = new Sub1();

            [TestField] Sub1 serializedField = new Sub1();
            public Sub1 SerializedField { get => serializedField; }
        }

        class TestComponent2 : MonoBehaviour
            , ISubComponent<TestComponent2>
        {
            public class Sub1 : ISubComponent<TestComponent2>
            {
                public int CallCounter = 0;

                public TestComponent2 RootComponent { get; set; }

                public void Destroy() { CallCounter++; }
                public void Init() { CallCounter++; }
                public void UpdateUI() { CallCounter++; }
            }

            [TestField]
            public Sub1 publicField = new Sub1();

            public TestComponent2 RootComponent { get; set; }

            public void Destroy() {}
            public void Init() {}
            public void UpdateUI() {}
        }

        /// <summary>
        /// <seealso cref="SubComponentManager{T}.RootComponent"/>
        /// <seealso cref="SubComponentManager{T}.Init()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest, Order(ORDER_CONSTRUCTOR), Description("コンストラクタのテスト")]
        public IEnumerator Constructor_Passes()
        {
            var rootComponent = new GameObject().AddComponent<TestComponent>();

            var manager = new SubComponentManager<TestComponent>(rootComponent); // test point
            Assert.AreSame(rootComponent, manager.RootComponent);
            Assert.AreSame(rootComponent, rootComponent.publicField.RootComponent);
            Assert.AreSame(rootComponent, rootComponent.SerializedField.RootComponent);
            Assert.AreEqual(2, manager.SubComponentCount);
            yield break;
        }

        /// <summary>
        /// <seealso cref="SubComponentManager{T}.RootComponent"/>
        /// <seealso cref="SubComponentManager{T}.Init()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest, Order(ORDER_CONSTRUCTOR), Description("RootComponentがISubComponentを継承している時のテスト")]
        public IEnumerator Constructor_Passes2()
        {
            var rootComponent = new GameObject().AddComponent<TestComponent2>();

            var manager = new SubComponentManager<TestComponent2>(rootComponent); // test point
            Assert.AreSame(rootComponent, manager.RootComponent);
            Assert.IsNull(rootComponent.RootComponent);
            Assert.AreSame(rootComponent, rootComponent.publicField.RootComponent);
            Assert.AreEqual(2, manager.SubComponentCount);
            yield break;
        }

        /// <summary>
        /// <seealso cref="SubComponentManager{T}.RootComponent"/>
        /// <seealso cref="SubComponentManager{T}.Init()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest, Order(ORDER_CONSTRUCTOR + 1), Description("RootComponentにあるISubComponentがNullの時のテスト")]
        public IEnumerator Constructor_IgnoreNullSubComponent_Passes()
        {
            var rootComponent = new GameObject().AddComponent<TestComponent>();
            rootComponent.publicField = null;

            SubComponentManager<TestComponent> manager = null;
            Assert.DoesNotThrow(() => {
                manager = new SubComponentManager<TestComponent>(rootComponent); // test point
            });

            Assert.AreSame(rootComponent, manager.RootComponent);
            Assert.AreEqual(1, manager.SubComponentCount);
            yield break;
        }

        #region Init
        /// <summary>
        /// <seealso cref="SubComponentManager{T}.RootComponent"/>
        /// <seealso cref="SubComponentManager{T}.Init()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator InitPasses()
        {
            var rootComponent = new GameObject().AddComponent<TestComponent>();

            var manager = new SubComponentManager<TestComponent>(rootComponent); // test point

            manager.Init(); // test point

            Assert.AreEqual(1, rootComponent.publicField.CallCounter);
            Assert.AreEqual(1, rootComponent.SerializedField.CallCounter);
            yield break;
        }


        [UnityTest]
        public IEnumerator InvokeSubComponentAttributeMethodsInitPasses()
        {
            var rootComponent = new GameObject().AddComponent<TestComponent>();

            var manager = new SubComponentManager<TestComponent>(rootComponent);
            Assert.AreSame(rootComponent, manager.RootComponent);

            TestFieldAttribute.RecievedInstAtInit.Clear();
            TestFieldAttribute.CallInitCounter = 0;

            manager.Init(); // test point

            AssertionUtils.AssertEnumerableByUnordered(
                new object[] { rootComponent.publicField, rootComponent.SerializedField },
                TestFieldAttribute.RecievedInstAtInit,
                ""
            );
            Assert.AreEqual(2, TestFieldAttribute.CallInitCounter);
            yield break;
        }
        #endregion

        #region Destroy
        /// <summary>
        /// <seealso cref="SubComponentManager{T}.RootComponent"/>
        /// <seealso cref="SubComponentManager{T}.Destroy()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator DestroyPasses()
        {
            var rootComponent = new GameObject().AddComponent<TestComponent>();

            var manager = new SubComponentManager<TestComponent>(rootComponent);
            Assert.AreSame(rootComponent, manager.RootComponent);

            manager.Destroy(); // test point

            Assert.AreEqual(1, rootComponent.publicField.CallCounter);
            Assert.AreEqual(1, rootComponent.SerializedField.CallCounter);
            yield break;
        }


        [UnityTest]
        public IEnumerator InvokeSubComponentAttributeMethodsDestroyPasses()
        {
            var rootComponent = new GameObject().AddComponent<TestComponent>();

            var manager = new SubComponentManager<TestComponent>(rootComponent);
            Assert.AreSame(rootComponent, manager.RootComponent);

            TestFieldAttribute.RecievedInstAtDestroy.Clear();
            TestFieldAttribute.CallDestroyCounter = 0;

            manager.Destroy(); // test point

            AssertionUtils.AssertEnumerableByUnordered(
                new object[] { rootComponent.publicField, rootComponent.SerializedField },
                TestFieldAttribute.RecievedInstAtDestroy,
                ""
            );
            Assert.AreEqual(2, TestFieldAttribute.CallDestroyCounter);
            yield break;
        }
        #endregion

        #region UpdateUI
        /// <summary>
        /// <seealso cref="SubComponentManager{T}.RootComponent"/>
        /// <seealso cref="SubComponentManager{T}.UpdateUI()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator UpdateUIPasses()
        {
            var rootComponent = new GameObject().AddComponent<TestComponent>();

            var manager = new SubComponentManager<TestComponent>(rootComponent);
            Assert.AreSame(rootComponent, manager.RootComponent);

            manager.UpdateUI(); // test point

            Assert.AreEqual(1, rootComponent.publicField.CallCounter);
            Assert.AreEqual(1, rootComponent.SerializedField.CallCounter);
            yield break;
        }


        [UnityTest]
        public IEnumerator InvokeSubComponentAttributeMethodsUpdateUIPasses()
        {
            var rootComponent = new GameObject().AddComponent<TestComponent>();

            var manager = new SubComponentManager<TestComponent>(rootComponent);
            Assert.AreSame(rootComponent, manager.RootComponent);

            TestFieldAttribute.RecievedInstAtUpdateUI.Clear();
            TestFieldAttribute.CallUpdateUICounter = 0;

            manager.UpdateUI(); // test point

            AssertionUtils.AssertEnumerableByUnordered(
                new object[] { rootComponent.publicField, rootComponent.SerializedField },
                TestFieldAttribute.RecievedInstAtUpdateUI,
                ""
            );
            Assert.AreEqual(2, TestFieldAttribute.CallUpdateUICounter);
            yield break;
        }
        #endregion

    }
}

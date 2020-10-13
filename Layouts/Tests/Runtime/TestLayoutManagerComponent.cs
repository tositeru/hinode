using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Tests;
using System.Linq;

namespace Hinode.Layouts.Tests
{
    /// <summary>
    /// Test List
    /// ## Entry
    /// -- 登録
    /// -- LayoutTargetComponent#OnDestroyへのコールバックの設定
    /// -- LayoutTargetComponent#LayoutTarget#OnDisposedへのコールバックの設定
    /// ## Exit
    /// -- 解除
    /// -- LayoutTargetComponent#OnDestroyへのコールバックの解除
    /// -- LayoutTargetComponent#LayoutTarget#OnDisposedへのコールバックの解除
    /// ## CaluculateLayouts
    /// -- 登録されているLayoutTargetCompnentのLayout計算
    /// ## SingletonMonoBehaviour<>
    /// -- OnDestroyed
    /// <seealso cref="LayoutManagerComponent"/>
    /// </summary>
    public class TestLayoutManagerComponent
    {
        const int ORDER_ENTRY = 0;
        const int ORDER_EXIT = ORDER_ENTRY + 100;
        const int ORDER_CALUCULATE_LAYOUTS = ORDER_ENTRY + 100;
        const int ORDER_SINGLETON_MONOBEHAVIOUR = -100;

        [TearDown()]
        public void TearDown()
        {
            Object.DestroyImmediate(LayoutManagerComponent.Instance.gameObject);
        }

        LayoutTargetComponent CreateLayoutTargetComponent(string name = "__layoutTargetCom")
        {
            var obj = new GameObject(name);
            var inst = obj.AddComponent<LayoutTargetComponent>();
            return inst;
        }

        #region Entry
        /// <summary>
        /// <seealso cref="LayoutManagerComponent.Entry(LayoutTargetComponent)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest, Order(ORDER_ENTRY), Description("")]
        public IEnumerator Entry_Passes()
        {
            var manager = LayoutManagerComponent.Instance;

            var layoutTarget = CreateLayoutTargetComponent("__test");

            manager.Entry(layoutTarget);

            TestLayoutManager.AssertRootsInLayoutManagerGroup(manager.Manager
                , new ILayoutTarget[] {
                    layoutTarget.LayoutTarget
                }
            );
            var group = manager.Manager.Groups.First(_g => _g.Root == layoutTarget.LayoutTarget);
            TestLayoutManager.AssertLayoutManagerGroup(group, layoutTarget.LayoutTarget, group.Priority, null, null,
                new ILayoutTarget[] {
                    layoutTarget.LayoutTarget
                }
            );


            AssertionUtils.AssertEnumerableByUnordered(
                new LayoutTargetComponent[] {
                    layoutTarget
                }
                , manager.Targets
                , ""
            );
            yield return null;
        }

        /// <summary>
        /// <seealso cref="LayoutManagerComponent.Entry(LayoutTargetComponent)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest, Order(ORDER_ENTRY), Description("")]
        public IEnumerator Entry_SetLayoutTargetComponentOnDestroyedCallback_Passes()
        {
            var manager = LayoutManagerComponent.Instance;

            var layoutTarget = CreateLayoutTargetComponent("__test");

            var cacheLayoutTargetOnDestroyedCallbackCounter = layoutTarget.OnDestroyed.RegistedDelegateCount;
            manager.Entry(layoutTarget);

            Assert.IsTrue(cacheLayoutTargetOnDestroyedCallbackCounter < layoutTarget.OnDestroyed.RegistedDelegateCount);

            yield break;
        }

        /// <summary>
        /// <seealso cref="LayoutManagerComponent.Entry(LayoutTargetComponent)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest, Order(ORDER_ENTRY), Description("")]
        public IEnumerator Entry_SetLayoutTargetComponent_LayoutTargetOnDisposedCallback_Passes()
        {
            var manager = LayoutManagerComponent.Instance;

            var layoutTarget = CreateLayoutTargetComponent("__test");

            var cacheLayoutTargetOnDisposedCallbackCounter = layoutTarget.LayoutTarget.OnDisposed.RegistedDelegateCount;
            manager.Entry(layoutTarget);

            Assert.IsTrue(cacheLayoutTargetOnDisposedCallbackCounter < layoutTarget.LayoutTarget.OnDisposed.RegistedDelegateCount);
            yield break;
        }
        #endregion

        #region Exit
        /// <summary>
        /// <seealso cref="LayoutManagerComponent.Exit(LayoutTargetComponent)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest, Order(ORDER_EXIT), Description("")]
        public IEnumerator Exit_Passes()
        {
            var manager = LayoutManagerComponent.Instance;

            var layoutTarget = CreateLayoutTargetComponent("__test");

            manager.Entry(layoutTarget);


            manager.Exit(layoutTarget);
            TestLayoutManager.AssertRootsInLayoutManagerGroup(manager.Manager
                , new ILayoutTarget[] {}
            );
            AssertionUtils.AssertEnumerableByUnordered(
                new LayoutTargetComponent[] {
                }
                , manager.Targets
                , ""
            );
            yield return null;
        }

        /// <summary>
        /// <seealso cref="LayoutManagerComponent.Exit(LayoutTargetComponent)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest, Order(ORDER_EXIT), Description("")]
        public IEnumerator Exit_RemoveLayoutTargetComponentOnDestroyedCallback_Passes()
        {
            var manager = LayoutManagerComponent.Instance;

            var layoutTarget = CreateLayoutTargetComponent("__test");
            manager.Entry(layoutTarget);

            var cacheLayoutTargetOnDestroyedCallbackCounter = layoutTarget.OnDestroyed.RegistedDelegateCount;
            manager.Exit(layoutTarget);

            Assert.IsTrue(cacheLayoutTargetOnDestroyedCallbackCounter > layoutTarget.OnDestroyed.RegistedDelegateCount);

            yield break;
        }

        /// <summary>
        /// <seealso cref="LayoutManagerComponent.Exit(LayoutTargetComponent)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest, Order(ORDER_EXIT), Description("")]
        public IEnumerator Exit_RemoveLayoutTargetComponent_LayoutTargetOnDisposedCallback_Passes()
        {
            var manager = LayoutManagerComponent.Instance;

            var layoutTarget = CreateLayoutTargetComponent("__test");
            manager.Entry(layoutTarget);

            var cacheLayoutTargetOnDisposedCallbackCounter = layoutTarget.LayoutTarget.OnDisposed.RegistedDelegateCount;
            manager.Exit(layoutTarget);

            Assert.IsTrue(cacheLayoutTargetOnDisposedCallbackCounter > layoutTarget.LayoutTarget.OnDisposed.RegistedDelegateCount);
            yield break;
        }
        #endregion

        #region CaluculateLayouts
        /// <summary>
        /// <seealso cref="LayoutManagerComponent.CaluculateLayouts()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest, Order(ORDER_CALUCULATE_LAYOUTS), Description("")]
        public IEnumerator CaluculateLayouts_Passes()
        {
            var manager = LayoutManagerComponent.Instance;

            var layoutTarget = CreateLayoutTargetComponent("__test");
            manager.Entry(layoutTarget);

            var layout = new TestLayoutManager.LayoutCallbackCounter();
            layoutTarget.LayoutTarget.AddLayout(layout);

            manager.CaluculateLayouts();

            Assert.AreEqual(1, layout.CallUpdateLayoutCounter);
            yield break;
        }
        #endregion

        #region SingletonMonoBehaviour
        /// <summary>
        /// <seealso cref="LayoutManagerComponent.OnDestroyed(bool)()"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest, Order(ORDER_SINGLETON_MONOBEHAVIOUR), Description("")]
        public IEnumerator SingletonMonoBehaviour_OnDestroyed_Passes()
        {
            var manager = LayoutManagerComponent.Instance;

            var layoutTarget = CreateLayoutTargetComponent("__test");

            manager.Entry(layoutTarget);

            var cacheLayoutTargetOnDestroyedCallbackCounter = layoutTarget.OnDestroyed.RegistedDelegateCount;
            var cacheLayoutTargetOnDisposedCallbackCounter = layoutTarget.LayoutTarget.OnDisposed.RegistedDelegateCount;

            Object.Destroy(manager.gameObject);
            yield return null;

            //TestLayoutManager.AssertRootsInLayoutManagerGroup(manager.Manager
            //    , new ILayoutTarget[] { }
            //);
            //AssertionUtils.AssertEnumerableByUnordered(
            //    new LayoutTargetComponent[] {
            //    }
            //    , manager.Targets
            //    , ""
            //);
            Assert.IsTrue(cacheLayoutTargetOnDestroyedCallbackCounter > layoutTarget.OnDestroyed.RegistedDelegateCount);
            Assert.IsTrue(cacheLayoutTargetOnDisposedCallbackCounter > layoutTarget.LayoutTarget.OnDisposed.RegistedDelegateCount);
            yield return null;
        }
        #endregion

    }
}

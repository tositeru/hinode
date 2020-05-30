using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using System.Linq;

namespace Hinode.Tests.MVC.Views
{
    /// <summary>
	/// <seealso cref="MonoBehaviourViewObject"/>
	/// </summary>
    public class TestMonoBehaviourViewObject : TestBase
    {
        [UnityTest]
        public IEnumerator DestroyPasses()
        {
            var viewObj = MonoBehaviourViewObject.Create();
            var viewGameObj = viewObj.gameObject;
            viewObj.Destroy();
            yield return null;

            var scene = SceneManager.GetActiveScene();
            Assert.IsFalse(scene.GetGameObjectEnumerable().Contains(viewGameObj));
        }

        [UnityTest]
        public IEnumerator DestroyWhenBindPasses()
        {
            var viewObj = MonoBehaviourViewObject.Create();
            viewObj.UseModel = new Model();
            viewObj.UseBindInfo = new ModelViewBinder.BindInfo(typeof(MonoBehaviourViewObject));

            Assert.IsTrue(viewObj.DoBinding());

            int unbindCounter = 0;
            int destroyCounter = 0;
            viewObj.OnUnbinded.Add(_v => unbindCounter++);
            viewObj.OnDestroyed.Add(_v => destroyCounter++);

            viewObj.Destroy();

            Assert.IsNull(viewObj.UseModel);
            Assert.IsNull(viewObj.UseBindInfo);
            Assert.IsNull(viewObj.UseBinderInstance);
            Assert.AreEqual(1, unbindCounter);
            Assert.AreEqual(1, destroyCounter);
            Debug.Log($"Success to IViewObject#Destroy!");

            unbindCounter = 0;
            destroyCounter = 0;
            viewObj.Destroy();
            Assert.AreEqual(0, unbindCounter);
            Assert.AreEqual(0, destroyCounter);
            Debug.Log($"Success to IViewObject#Destroy When already destroy!");
            yield return null;
        }

        [Test]
        public void DestroyWhenBindNothingPasses()
        {
            var viewObj = MonoBehaviourViewObject.Create();

            Assert.IsFalse(viewObj.DoBinding());

            int unbindCounter = 0;
            int destroyCounter = 0;
            viewObj.OnUnbinded.Add(_v => unbindCounter++);
            viewObj.OnDestroyed.Add(_v => destroyCounter++);

            viewObj.Destroy();

            Assert.IsNull(viewObj.UseModel);
            Assert.IsNull(viewObj.UseBindInfo);
            Assert.IsNull(viewObj.UseBinderInstance);
            Assert.AreEqual(0, unbindCounter);
            Assert.AreEqual(1, destroyCounter);
        }

        [Test]
        public void BindingPasses()
        {
            {
                var viewObj = MonoBehaviourViewObject.Create();
                viewObj.UseModel = new Model();
                viewObj.UseBindInfo = new ModelViewBinder.BindInfo(typeof(MonoBehaviourViewObject));
                Assert.IsTrue(viewObj.DoBinding());
                Debug.Log($"Success to DoBinding()");

                int counter = 0;
                viewObj.OnUnbinded.Add(_v => counter++);
                viewObj.Unbind();

                Assert.IsNull(viewObj.UseModel);
                Assert.IsNull(viewObj.UseBindInfo);
                Assert.IsFalse(viewObj.DoBinding());
                Assert.AreEqual(1, counter);
                Debug.Log($"Success to Unbind()");

                counter = 0;
                viewObj.Unbind(); // <- when DoBinding() == false
                Assert.AreEqual(0, counter);
                Debug.Log($"Success to not Call OnUnbind() when Bind Nothing!");
            }

            {
                var viewObj = MonoBehaviourViewObject.Create();
                viewObj.UseModel = null;
                viewObj.UseBindInfo = new ModelViewBinder.BindInfo(typeof(MonoBehaviourViewObject));
                Assert.IsFalse(viewObj.DoBinding());
            }
            {
                var viewObj = MonoBehaviourViewObject.Create();
                viewObj.UseModel = new Model();
                viewObj.UseBindInfo = null;
                Assert.IsFalse(viewObj.DoBinding());
            }
            {
                var viewObj = MonoBehaviourViewObject.Create();
                viewObj.UseModel = null;
                viewObj.UseBindInfo = null;
                Assert.IsFalse(viewObj.DoBinding());
            }
            Debug.Log($"Success all Case (DoBinding() == false)!");
        }

    }
}

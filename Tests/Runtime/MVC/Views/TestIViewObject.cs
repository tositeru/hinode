using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC.Views
{
    /// <summary>
    /// <seealso cref="IViewObject"/>
    /// <seealso cref="EmptyViewObject"/>
    /// </summary>
    public class TestIViewObject
    {
        [Test]
        public void DestroyWhenBindPasses()
        {
            var viewObj = new EmptyViewObject()
            {
                UseModel = new Model(),
                UseBindInfo = new ModelViewBinder.BindInfo(typeof(EmptyViewObject)),
            };

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
        }

        [Test]
        public void DestroyWhenBindNothingPasses()
        {
            var viewObj = new EmptyViewObject();

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
                var viewObj = new EmptyViewObject()
                {
                    UseModel = new Model(),
                    UseBindInfo = new ModelViewBinder.BindInfo(typeof(EmptyViewObject)),
                };
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
                var viewObj = new EmptyViewObject()
                {
                    UseModel = null,
                    UseBindInfo = new ModelViewBinder.BindInfo(typeof(EmptyViewObject)),
                };
                Assert.IsFalse(viewObj.DoBinding());
            }
            {
                var viewObj = new EmptyViewObject()
                {
                    UseModel = new Model(),
                    UseBindInfo = null,
                };
                Assert.IsFalse(viewObj.DoBinding());
            }
            {
                var viewObj = new EmptyViewObject()
                {
                    UseModel = null,
                    UseBindInfo = null,
                };
                Assert.IsFalse(viewObj.DoBinding());
            }
            Debug.Log($"Success all Case (DoBinding() == false)!");
        }
    }
}

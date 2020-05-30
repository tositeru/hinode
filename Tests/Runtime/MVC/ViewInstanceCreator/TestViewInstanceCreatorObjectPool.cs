using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.MVC.ViewInstanceCreator
{
    /// <summary>
    /// <seealso cref="ViewInstanceCreatorObjectPool"/>
    /// </summary>
    public class TestViewInstanceCreatorObjectPool
    {
        class PoolingViewObj : EmptyViewObject
        {
            public static int newCounter = 0;

            public int DestroyCallCount { get; private set; } = 0;
            public int BindCallCount { get; private set; } = 0;
            public int UnbindCallCount { get; private set; } = 0;

            public PoolingViewObj()
            {
                newCounter++;
            }

            public void Reset()
            {
                DestroyCallCount = 0;
                BindCallCount = 0;
                UnbindCallCount = 0;
            }

            protected override void OnDestroy()
            {
                DestroyCallCount++;
            }

            protected override void OnBind(Model targetModel, ModelViewBinder.BindInfo bindInfo, ModelViewBinderInstanceMap binderInstanceMap)
            {
                BindCallCount++;
            }

            protected override void OnUnbind()
            {
                UnbindCallCount++;
            }
        }

        // A Test behaves as an ordinary method
        [Test]
        public void PopAndPushPasses()
        {
            var creator = new DefaultViewInstanceCreator(
                (typeof(PoolingViewObj), new EmptyModelViewParamBinder()),
                (typeof(EmptyViewObject), new EmptyModelViewParamBinder())
                );
            var objPool = new ViewInstanceCreatorObjectPool(creator);

            Assert.AreSame(creator, objPool.UseCreator);

            {//PoolingViewObj
                var poolingBindInfo = new ModelViewBinder.BindInfo(typeof(PoolingViewObj));

                PoolingViewObj.newCounter = 0;
                var viewObj = objPool.PopOrCreate(poolingBindInfo) as PoolingViewObj;
                Assert.IsTrue(viewObj is PoolingViewObj);
                Assert.AreEqual(0, viewObj.DestroyCallCount);
                Assert.AreEqual(0, viewObj.BindCallCount);
                Assert.AreEqual(0, viewObj.UnbindCallCount);
                Assert.AreEqual(1, PoolingViewObj.newCounter);
                objPool.Push(viewObj);

                PoolingViewObj.newCounter = 0;
                var popViewObj = objPool.PopOrCreate(poolingBindInfo) as PoolingViewObj;
                Assert.AreSame(viewObj, popViewObj);
                Assert.AreEqual(0, PoolingViewObj.newCounter);

                PoolingViewObj.newCounter = 0;
                var popViewObj2 = objPool.PopOrCreate(poolingBindInfo) as PoolingViewObj;
                Assert.AreNotSame(viewObj, popViewObj2);
                Assert.AreEqual(1, PoolingViewObj.newCounter);
            }
            Debug.Log("Success Basic Pop and Push");

            {//EmptyViewObject
                var emptyBindInfo = new ModelViewBinder.BindInfo(typeof(EmptyViewObject));

                var viewObj = objPool.PopOrCreate(emptyBindInfo) as EmptyViewObject;
                Assert.IsTrue(viewObj is EmptyViewObject);
                objPool.Push(viewObj);

                var popViewObj = objPool.PopOrCreate(emptyBindInfo) as EmptyViewObject;
                Assert.AreSame(viewObj, popViewObj);

                var popViewObj2 = objPool.PopOrCreate(emptyBindInfo) as EmptyViewObject;
                Assert.AreNotSame(viewObj, popViewObj2);
            }
        }

        [Test]
        public void DisposePasses()
        {
            var creator = new DefaultViewInstanceCreator(
                (typeof(PoolingViewObj), new EmptyModelViewParamBinder()),
                (typeof(EmptyViewObject), new EmptyModelViewParamBinder())
                );
            var objPool = new ViewInstanceCreatorObjectPool(creator);

            Assert.AreSame(creator, objPool.UseCreator);

            var clearPoolingViewObj = new PoolingViewObj();
            objPool.Push(clearPoolingViewObj);

            var clearEmptyViewObj = new EmptyViewObject();
            objPool.Push(clearEmptyViewObj);


            objPool.Dispose();

            Assert.IsNull(objPool.UseCreator);
        }

        [Test]
        public void CreatorPasses()
        {
            var creator = new DefaultViewInstanceCreator(
                (typeof(PoolingViewObj), new EmptyModelViewParamBinder()),
                (typeof(EmptyViewObject), new EmptyModelViewParamBinder())
                );

            var cacheBindInfo = new ModelViewBinder.BindInfo(typeof(PoolingViewObj))
            {
                ViewObjectCreateType = ViewObjectCreateType.Cache
            };
            var viewObj = creator.CreateViewObj(cacheBindInfo);
            viewObj.UseModel = new Model();
            Assert.IsTrue(viewObj is PoolingViewObj);

            viewObj.Unbind();

            {
                PoolingViewObj.newCounter = 0;
                var viewObj2 = creator.CreateViewObj(cacheBindInfo);
                Assert.AreSame(viewObj, viewObj2, $"IViewObject#Unbindされた時に、Cacheされていません。");
                Assert.AreEqual(0, PoolingViewObj.newCounter);

                PoolingViewObj.newCounter = 0;
                var viewObj3 = creator.CreateViewObj(cacheBindInfo);
                Assert.AreNotSame(viewObj, viewObj3);
                Assert.AreEqual(1, PoolingViewObj.newCounter);
            }

            {
                PoolingViewObj.newCounter = 0;
                viewObj.Unbind();
                viewObj.Destroy();
                var viewObj2 = creator.CreateViewObj(cacheBindInfo);
                Assert.AreNotSame(viewObj, viewObj2, $"IViewObject#Destroyが呼び出された場合にCacheされていたら、Cacheから取り除くようにしてください");
                Assert.AreEqual(1, PoolingViewObj.newCounter);
            }
        }
    }
}

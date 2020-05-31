using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Tests;

namespace Hinode.MVC.Tests.ViewInstanceCreator
{
    /// <summary>
    /// <seealso cref="UnityViewInstanceCreatorObjectPool"/>
    /// </summary>
    public class TestUnityViewInstanceCreatorObjectPool
    {
        [SetUp]
        public void Setup()
        {
            Logger.PriorityLevel = Logger.Priority.Debug;
        }

        [UnityTest]
        public IEnumerator PopAndPushPasses()
        {
            var creator = new UnityViewInstanceCreator()
                .AddUnityViewObjects();
            var pool = new UnityViewInstanceCreatorObjectPool(creator);
            yield return null;

            var bindInfo = new ModelViewBinder.BindInfo(typeof(CubeViewObject));
            var cubeViewObj = pool.PopOrCreate(bindInfo) as CubeViewObject;

            Assert.IsNotNull(cubeViewObj);
            Assert.IsTrue(cubeViewObj.gameObject.activeSelf);
            Assert.IsNull(cubeViewObj.transform.parent);

            pool.Push(cubeViewObj);
            Assert.IsFalse(cubeViewObj.gameObject.activeSelf);
            Assert.AreSame(pool.PoolingObjParent, cubeViewObj.transform.parent);

            var viewObj = pool.PopOrCreate(bindInfo);
            Assert.AreSame(cubeViewObj, viewObj);
            Assert.IsTrue(cubeViewObj.gameObject.activeSelf);
            Assert.IsNull(cubeViewObj.transform.parent);
        }

        [UnityTest]
        public IEnumerator CreatorPasses()
        {
            yield return null;

            var creator = new UnityViewInstanceCreator()
                .AddUnityViewObjects();

            var bindInfo = new ModelViewBinder.BindInfo(typeof(CubeViewObject))
                .SetViewCreateType(ViewObjectCreateType.Cache);

            var cubeViewObj = creator.CreateViewObj(bindInfo) as CubeViewObject;
            {
                Assert.IsTrue(cubeViewObj is CubeViewObject);
                Assert.IsTrue(cubeViewObj.gameObject.activeSelf);
                Assert.IsNull(cubeViewObj.transform.parent);
                Assert.AreSame(bindInfo, cubeViewObj.UseBindInfo);
            }
            Debug.Log($"Success to IViewInstanceCreator#CreateViewObj!");

            {
                cubeViewObj.UseModel = new Model();
                Assert.IsTrue(cubeViewObj.DoBinding());

                cubeViewObj.Unbind();
                Assert.IsFalse(cubeViewObj.gameObject.activeSelf);
                Assert.AreSame(creator.PoolingViewObjParent, cubeViewObj.transform.parent);
                Assert.IsNull(cubeViewObj.UseModel);
                Assert.IsNull(cubeViewObj.UseBindInfo);
            }
            Debug.Log($"Success to IViewObject#Unbind!");

            {
                var cachedViewObj = creator.CreateViewObj(bindInfo) as CubeViewObject;
                Assert.AreSame(cubeViewObj, cachedViewObj);
                Assert.IsTrue(cachedViewObj.gameObject.activeSelf);
                Assert.IsNull(cachedViewObj.transform.parent);
                Assert.AreSame(bindInfo, cachedViewObj.UseBindInfo);
            }
            Debug.Log($"Success to IViewInstanceCreator#CreateViewObj(Cached)!");

            {
                cubeViewObj.UseModel = new Model();
                Assert.IsTrue(cubeViewObj.DoBinding());

                cubeViewObj.Unbind(); // <- cache in Pool!!

                cubeViewObj.Destroy();
                var newViewObj = creator.CreateViewObj(bindInfo);
                Assert.AreNotSame(cubeViewObj, newViewObj, $"IViewObject#Destroyが呼び出された場合にCacheされていたら、Cacheから取り除くようにしてください");
            }
            Debug.Log($"Success to IViewObject#Destroy!(Remove from ObjPool)");
        }

    }
}

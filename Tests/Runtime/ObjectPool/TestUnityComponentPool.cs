using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.ObjectPool
{
    /// <summary>
    /// <seealso cref="UnityComponentPool{T}"/>
    /// </summary>
    public class TestUnityComponentPool
    {
        class TestInstanceCreator : UnityComponentPool<Transform>.IInstanceCreater
        {
            public Transform Create()
            {
                return new GameObject().transform;
            }
        }

        [UnityTest]
        public IEnumerator BasicUsagePasses()
        {
            yield return null;

            var pool = new UnityComponentPool<Transform>(new TestInstanceCreator());
            Assert.AreEqual(0, pool.Count);

            var obj = pool.PopOrCreate();
            Assert.IsNotNull(obj);
            Assert.AreEqual(0, pool.Count);
            Assert.IsTrue(obj.gameObject.activeSelf);

            pool.Push(obj);
            Assert.AreEqual(1, pool.Count);
            Assert.IsFalse(obj.gameObject.activeSelf);

            var obj2 = pool.PopOrCreate();
            Assert.AreSame(obj, obj2);
            Assert.IsTrue(obj2.gameObject.activeSelf);
            Assert.AreEqual(0, pool.Count);
        }

        [UnityTest]
        public IEnumerator PushSameObjPasses()
        {
            yield return null;

            var pool = new UnityComponentPool<Transform>(new TestInstanceCreator());
            var obj = pool.PopOrCreate();
            pool.Push(obj);
            pool.Push(obj);
            Assert.AreEqual(1, pool.Count);
        }

        [UnityTest]
        public IEnumerator GetOrCreateObjectOrderPasses()
        {
            yield return null;

            var pool = new UnityComponentPool<Transform>(new TestInstanceCreator());
            var objs = Enumerable.Range(0, 5).Select(_ => pool.PopOrCreate()).ToList();
            foreach (var o in objs)
            {
                pool.Push(o);
            }

            while(0 < pool.Count)
            {
                var o = pool.PopOrCreate();
                Assert.IsTrue(objs.Any(_o => _o == o));
                objs.Remove(o);
            }
            Assert.AreEqual(0, objs.Count);
        }
    }
}

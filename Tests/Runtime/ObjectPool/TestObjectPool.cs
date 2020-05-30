using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.ObjectPool
{
    /// <summary>
    /// <seealso cref="ObjectPool{T}"/>
    /// </summary>
    public class TestObjectPool
    {
        class TestClass { }

        class TestInstanceCreator : ObjectPool<TestClass>.IInstanceCreater
        {
            public TestClass Create()
            {
                return new TestClass();
            }
        }

        [Test]
        public void BasicUsagePasses()
        {
            var pool = new ObjectPool<TestClass>(new TestInstanceCreator());
            Assert.AreEqual(0, pool.Count);

            var obj = pool.PopOrCreate();
            Assert.IsNotNull(obj);
            Assert.AreEqual(0, pool.Count);

            pool.Push(obj);
            Assert.AreEqual(1, pool.Count);

            var obj2 = pool.PopOrCreate();
            Assert.AreSame(obj, obj2);
            Assert.AreEqual(0, pool.Count);
        }

        [Test]
        public void PushSameObjPasses()
        {
            var pool = new ObjectPool<TestClass>(new TestInstanceCreator());
            var obj = pool.PopOrCreate();
            pool.Push(obj);
            pool.Push(obj);
            Assert.AreEqual(1, pool.Count);
        }

        [Test]
        public void GetOrCreateObjectOrderPasses()
        {
            var pool = new ObjectPool<TestClass>(new TestInstanceCreator());
            var objs = Enumerable.Range(0, 5).Select(_ => pool.PopOrCreate()).ToList();
            foreach (var o in objs)
            {
                pool.Push(o);
            }

            while (0 < pool.Count)
            {
                var o = pool.PopOrCreate();
                Assert.IsTrue(objs.Any(_o => _o == o));
                objs.Remove(o);
            }
            Assert.AreEqual(0, objs.Count);
        }

        [Test, Description("")]
        public void RemovePasses()
        {
            var pool = new ObjectPool<TestClass>(new TestInstanceCreator());

            var obj = pool.PopOrCreate();
            pool.Push(obj);
            Assert.AreEqual(1, pool.Count);

            pool.Remove(obj);
            Assert.AreEqual(0, pool.Count);

            var obj2 = pool.PopOrCreate();
            Assert.AreNotSame(obj, obj2);

            Assert.DoesNotThrow(() => {
                pool.Remove(obj);
            });
        }

        [Test, Description("")]
        public void ClearPasses()
        {
            var pool = new ObjectPool<TestClass>(new TestInstanceCreator());

            var count = 5;
            for(var i=0; i<count; ++i)
            {
                pool.Push(new TestClass());
            }
            Assert.AreEqual(count, pool.Count);

            pool.Clear();
            Assert.AreEqual(0, pool.Count);
        }

    }
}

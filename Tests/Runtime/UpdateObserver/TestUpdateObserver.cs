using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.UpdateObserver
{
    public class TestUpdateObserver
    {
        [Test]
        public void BasicUsagePasses()
        {
            var v = new UpdateObserver<int>();
            Assert.IsFalse(v.DidUpdated);

            v.Value = 100;
            Assert.AreEqual(100, v.Value);
            Assert.IsTrue(v.DidUpdated);

            v.Value = -100;
            Assert.AreEqual(-100, v.Value);
            Assert.IsTrue(v.DidUpdated);

            var prevValue = v.Value;
            v.Reset();
            Assert.AreEqual(prevValue, v.Value);
            Assert.IsFalse(v.DidUpdated);
        }

    }
}

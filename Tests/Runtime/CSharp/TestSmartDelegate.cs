using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp
{
    /// <summary>
    /// <seealso cref="SmartDelegate{T}"/>
    /// <seealso cref="NotInvokableDelegate{T}"/>
    /// </summary>
    public class TestSmartDelegate
    {
        delegate void BasicUsagePassesDelegate();

        [Test]
        public void BasicUsagePasses()
        {
            var predicate = new SmartDelegate<BasicUsagePassesDelegate>();
            Assert.IsFalse(predicate.IsValid);
            Assert.IsNull(predicate.Instance);

            int count = 0;
            BasicUsagePassesDelegate incrementFunc = () => { count++; };
            predicate.Set(incrementFunc);
            Assert.IsTrue(predicate.IsValid);
            Assert.IsNotNull(predicate.Instance);

            predicate.Instance.Invoke();
            Assert.AreEqual(1, count);

            predicate.Remove(incrementFunc);
            Assert.IsFalse(predicate.IsValid);
            Assert.IsNull(predicate.Instance);

            predicate.Add(incrementFunc);
            Assert.IsTrue(predicate.IsValid);
            Assert.IsNotNull(predicate.Instance);


            var predicate2 = new SmartDelegate<BasicUsagePassesDelegate>(predicate);
            Assert.IsTrue(predicate2.IsValid);
            Assert.IsNotNull(predicate2.Instance);

            count = 0;
            predicate2.Instance.Invoke();
            Assert.AreEqual(1, count);

            predicate.Clear();
            Assert.IsFalse(predicate.IsValid);
            Assert.IsNull(predicate.Instance);
        }

        void Apple() { }
        [Test]
        public void RegistedDelegateCountPasses()
        {
            var d = new SmartDelegate<BasicUsagePassesDelegate>();

            d.Add(() => { });
            d.Add(Apple);
            Assert.AreEqual(2, d.RegistedDelegateCount);

            d.Remove(Apple);
            Assert.AreEqual(1, d.RegistedDelegateCount);
        }
    }
}

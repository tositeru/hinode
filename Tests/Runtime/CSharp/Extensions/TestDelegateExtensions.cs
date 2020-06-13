using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp.Extensions
{
    /// <summary>
    /// <seealso cref="DelegateExtensions"/>
    /// </summary>
    public class TestDelegateExtensions : TestBase
    {
        delegate void TestDelegate();

        /// <summary>
        /// <seealso cref="DelegateExtensions.ClearInvocations(System.Delegate)"/>
        /// <seealso cref="DelegateExtensions.ClearInvocations{T}(T)"/>
        /// </summary>
        [Test]
        public void ClearInvocationsPasses()
        {
            TestDelegate predicate = () => { Debug.Log("Pred1"); };
            predicate += () => { Debug.Log("Pred2"); };
            predicate = predicate.ClearInvocations();
            //predicate.Invoke();
            Assert.IsNull(predicate);

            {//nullなDelefateの時のテスト
                TestDelegate emptyPredicate = null;
                emptyPredicate = emptyPredicate.ClearInvocations();
                Assert.IsNull(predicate);
            }
        }
    }
}

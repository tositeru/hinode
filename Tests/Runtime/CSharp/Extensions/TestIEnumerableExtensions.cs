using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp.Extensions
{
    /// <summary>
    /// test case
    /// ## IsEmptyOrNull
    /// <seealso cref="IEnumerableExtensions"/>
    /// </summary>
    public class TestIEnumerableExtensions
    {
        const int Order_IsEmptyOrNull = 0;
        #region IsEmptyOrNull
        class IsEmptyOrNullTest { }
        /// <summary>
        /// <seealso cref="IEnumerableExtensions.IsEmptyOrNull{T}(IEnumerable{T})"/>
        /// </summary>
        [Test, Order(Order_IsEmptyOrNull), Description("")]
        public void IsEmptyOrNull_Passes()
        {
            Assert.IsTrue(IEnumerableExtensions.IsEmptyOrNull<int>(null));
            Assert.IsTrue(IEnumerableExtensions.IsEmptyOrNull<IsEmptyOrNullTest>(null));
            Assert.IsTrue(IEnumerableExtensions.IsEmptyOrNull(new int[] { }));
            Assert.IsTrue(IEnumerableExtensions.IsEmptyOrNull(new List<string>() { }));
            Assert.IsTrue(IEnumerableExtensions.IsEmptyOrNull(new IsEmptyOrNullTest[] { }));
            Assert.IsTrue(IEnumerableExtensions.IsEmptyOrNull(new List<IsEmptyOrNullTest> { }));

            Assert.IsFalse(IEnumerableExtensions.IsEmptyOrNull(new int[2]));
            Assert.IsFalse(IEnumerableExtensions.IsEmptyOrNull(new List<string>() { "", "" }));
            Assert.IsFalse(IEnumerableExtensions.IsEmptyOrNull(new IsEmptyOrNullTest[] { new IsEmptyOrNullTest() }));
            Assert.IsFalse(IEnumerableExtensions.IsEmptyOrNull(new List<IsEmptyOrNullTest> { new IsEmptyOrNullTest() }));
        }
        #endregion
    }
}

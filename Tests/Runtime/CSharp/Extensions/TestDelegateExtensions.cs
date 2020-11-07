using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp.Extensions
{
    /// <summary>
    /// test case
    /// ## ClerInvocations
    /// ## GetFieldValue
    /// <seealso cref="DelegateExtensions"/>
    /// </summary>
    public class TestDelegateExtensions : TestBase
    {
        const int Order_ClearInvocations = 0;
        const int Order_GetFieldValue = 0;
        const int Order_DoMatchFieldValues = Order_GetFieldValue + 100;

        #region ClearInvocations
        delegate void TestDelegate();

        /// <summary>
        /// <seealso cref="DelegateExtensions.ClearInvocations(System.Delegate)"/>
        /// <seealso cref="DelegateExtensions.ClearInvocations{T}(T)"/>
        /// </summary>
        [Test, Order(Order_ClearInvocations), Description("")]
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
        #endregion

        #region GetFieldValue
        class GetFieldValueTest
        {
            public delegate void Delegate();
            public static Delegate MakeDelegate(int a, int b)
            {
                return () => { var _ = a + b; };
            }
        }

        /// <summary>
        /// <seealso cref="DelegateExtensions.GetField(System.Delegate, string, out object)"/>
        /// </summary>
        [Test, Order(Order_GetFieldValue), Description("")]
        public void GetFieldValue_Passes()
        {
            GetFieldValueTest.Delegate pred = GetFieldValueTest.MakeDelegate(10, 20);
            Assert.IsTrue(pred.GetFieldValue("a", out var a));
            Assert.AreEqual(10, a);
            Assert.IsTrue(pred.GetFieldValue("b", out var b));
            Assert.AreEqual(20, b);

            Assert.IsFalse(pred.GetFieldValue("1XXX1", out var _));
        }
        #endregion

        #region DoMatchFieldValues
        class DoMatchFieldValuesTest
        {
            public delegate void Delegate();
            public static Delegate MakeDelegate(int a, int b)
            {
                return () => { var _ = a + b; };
            }
        }

        /// <summary>
        /// <seealso cref="DelegateExtensions.DoMatchFieldValues(System.Delegate, (string fieldName, object value)[])"/>
        /// </summary>
        [Test, Order(Order_DoMatchFieldValues), Description("")]
        public void DoMatchFieldValues_Passes()
        {
            DoMatchFieldValuesTest.Delegate pred = DoMatchFieldValuesTest.MakeDelegate(10, 20);
            Assert.IsTrue(pred.GetFieldValue("a", out var a));
            Assert.IsTrue(((object)(10)).Equals(a));

            Assert.IsTrue(pred.DoMatchFieldValues(("a", 10)));
            Assert.IsTrue(pred.DoMatchFieldValues(("b", 20)));
            Assert.IsTrue(pred.DoMatchFieldValues(("a", 10), ("b", 20)));

            Assert.IsFalse(pred.DoMatchFieldValues(("a", -10)));
            Assert.IsFalse(pred.DoMatchFieldValues(("b", -20)));
            Assert.IsFalse(pred.DoMatchFieldValues(("a", -10), ("b", -20)));
            Assert.IsFalse(pred.DoMatchFieldValues(("a", null)));
            Assert.IsFalse(pred.DoMatchFieldValues(("2XX2", -10)));
        }
        #endregion
    }
}

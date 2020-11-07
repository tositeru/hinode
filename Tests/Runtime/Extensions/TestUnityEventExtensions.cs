using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;

namespace Hinode.Tests.Extensions
{
    /// <summary>
    /// test case
    /// ## GetInvocationList
    /// <seealso cref="UnityEventExtensions"/>
    /// </summary>
    public class TestUnityEventExtensions
    {
        const int Order_GetInvocationList = 0;

        #region GetInvocationList
        // A Test behaves as an ordinary method
        [Test, Order(Order_GetInvocationList), Description("")]
        public void TestUnityEventExtensionsSimplePasses()
        {
            UnityAction action = () => { };
            UnityAction action2 = () => { };

            var events = new UnityEvent();
            events.AddListener(action);
            events.AddListener(action2);

            AssertionUtils.AssertEnumerableByUnordered(
                new System.Delegate[]
                {
                    action, action2
                }
                , events.GetInvocationList()
                , ""
            );
        }
        #endregion
    }
}

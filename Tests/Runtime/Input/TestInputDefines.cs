using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Input
{
    /// <summary>
    /// <seealso cref="InputDefines"/>
    /// </summary>
    public class TestInputDefines
    {
        /// <summary>
		/// <seealso cref="InputDefines.ToButtonCondition(Touch)"/>
		/// </summary>
		/// <returns></returns>
        [UnityTest]
        public IEnumerator TouchToButtonConditionPasses()
        {
            yield return null;

            var touch = new Touch();
            touch.phase = TouchPhase.Began;
            Assert.AreEqual(InputDefines.ButtonCondition.Down, InputDefines.ToButtonCondition(touch));
            touch.phase = TouchPhase.Moved;
            Assert.AreEqual(InputDefines.ButtonCondition.Push, InputDefines.ToButtonCondition(touch));
            touch.phase = TouchPhase.Stationary;
            Assert.AreEqual(InputDefines.ButtonCondition.Push, InputDefines.ToButtonCondition(touch));
            touch.phase = TouchPhase.Ended;
            Assert.AreEqual(InputDefines.ButtonCondition.Up, InputDefines.ToButtonCondition(touch));
            touch.phase = TouchPhase.Canceled;
            Assert.AreEqual(InputDefines.ButtonCondition.Up, InputDefines.ToButtonCondition(touch));
        }

    }
}

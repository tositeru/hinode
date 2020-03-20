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
        [UnityTest]
        public IEnumerator MouseButtonToButtonConditionPasses()
        {
            yield return null;

            var baseInputObj = new GameObject("__baseInput", typeof(ReplayableBaseInput));
            var baseInput = baseInputObj.GetComponent<ReplayableBaseInput>();
            baseInput.IsReplaying = true;
            foreach(InputDefines.MouseButton btn in System.Enum.GetValues(typeof(InputDefines.MouseButton)))
            {
                baseInput.SetRecordedMouseButton(btn, InputDefines.ButtonCondition.Down);
                Assert.AreEqual(InputDefines.ButtonCondition.Down, InputDefines.ToButtonCondition(baseInput, btn));
                baseInput.SetRecordedMouseButton(btn, InputDefines.ButtonCondition.Push);
                Assert.AreEqual(InputDefines.ButtonCondition.Push, InputDefines.ToButtonCondition(baseInput, btn));
                baseInput.SetRecordedMouseButton(btn, InputDefines.ButtonCondition.Up);
                Assert.AreEqual(InputDefines.ButtonCondition.Up, InputDefines.ToButtonCondition(baseInput, btn));
                baseInput.SetRecordedMouseButton(btn, InputDefines.ButtonCondition.Free);
                Assert.AreEqual(InputDefines.ButtonCondition.Free, InputDefines.ToButtonCondition(baseInput, btn));
            }
        }

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

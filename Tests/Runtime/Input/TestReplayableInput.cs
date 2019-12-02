using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Input
{
    /// <summary>
    /// <seealso cref="ReplayableInput"/>
    /// </summary>
    public class TestReplayableInput
    {
        [UnityTest]
        public IEnumerator MouseInputPasses()
        {
            yield return null;

            var input = ReplayableInput.Instance;

            {//GetMouseButton
                foreach (InputDefines.MouseButton btn in System.Enum.GetValues(typeof(InputDefines.MouseButton)))
                {
                    input.IsReplaying = false;
                    input.GetMouseButton(btn);
                    Assert.AreEqual(InputDefines.ToButtonCondition(btn), input.GetMouseButton(btn));

                    input.IsReplaying = true;
                    var condition = InputDefines.ButtonCondition.Push;
                    input.SetRecordedMouseButton(btn, condition);
                    Assert.AreEqual(condition, input.GetRecordedMouseButton(btn));
                    Assert.AreEqual(condition, input.GetMouseButton(btn));
                }
            }

            {//MousePresent
                input.IsReplaying = false;
                Assert.AreEqual(UnityEngine.Input.mousePosition, input.MousePos);
                input.IsReplaying = true;
                input.RecordedMousePos = Vector3.one;
                Assert.AreEqual(input.RecordedMousePos, input.MousePos);
            }

            {//MousePresent
                input.IsReplaying = false;
                Assert.AreEqual(UnityEngine.Input.mousePresent, input.MousePresent);
                input.IsReplaying = true;
                input.RecordedMousePresent = !UnityEngine.Input.mousePresent;
                Assert.AreEqual(input.RecordedMousePresent, input.MousePresent);
            }

            {//MouseScrollDelta
                input.IsReplaying = false;
                Assert.AreEqual(UnityEngine.Input.mouseScrollDelta, input.MouseScrollDelta);
                input.IsReplaying = true;
                input.RecordedMouseScrollDelta = Vector2.one;
                Assert.AreEqual(input.RecordedMouseScrollDelta, input.MouseScrollDelta);
            }
        }

        [UnityTest]
        public IEnumerator TouchInputPasses()
        {
            yield return null;
            var input = ReplayableInput.Instance;

            {//TouchSupported
                input.IsReplaying = false;
                Assert.AreEqual(UnityEngine.Input.touchSupported, input.TouchSupported);
                input.IsReplaying = true;
                input.RecordedTouchSupported = !UnityEngine.Input.touchSupported;
                Assert.AreEqual(input.RecordedTouchSupported, input.TouchSupported);
            }

            {//MultiTouchEnabled
                input.IsReplaying = false;
                Assert.AreEqual(UnityEngine.Input.multiTouchEnabled, input.MultiTouchEnabled);
                input.IsReplaying = true;
                input.RecordedMultiTouchEnabled = !UnityEngine.Input.multiTouchEnabled;
                Assert.AreEqual(input.RecordedMultiTouchEnabled, input.MultiTouchEnabled);
            }

            {//StylusTouchSupported
                input.IsReplaying = false;
                Assert.AreEqual(UnityEngine.Input.stylusTouchSupported, input.StylusTouchSupported);
                input.IsReplaying = true;
                input.RecordedStylusTouchSupported = !UnityEngine.Input.stylusTouchSupported;
                Assert.AreEqual(input.RecordedStylusTouchSupported, input.StylusTouchSupported);
            }

            {//TouchPressureSupported
                input.IsReplaying = false;
                Assert.AreEqual(UnityEngine.Input.touchPressureSupported, input.TouchPressureSupported);
                input.IsReplaying = true;
                input.RecordedTouchPressureSupported = !UnityEngine.Input.touchPressureSupported;
                Assert.AreEqual(input.RecordedTouchPressureSupported, input.TouchPressureSupported);
            }

            {//TouchCount
                input.IsReplaying = false;
                Assert.AreEqual(UnityEngine.Input.touchCount, input.TouchCount);
                input.IsReplaying = true;
                input.RecordedTouchCount = 3;
                Assert.AreEqual(input.RecordedTouchCount, input.TouchCount);
            }

            {//GetTouch
                input.IsReplaying = true;
                input.RecordedTouchCount = 3;
                for(var i=0; i< input.TouchCount; ++i)
                {
                    var t = new Touch()
                    {
                        fingerId = i,
                    };
                    input.SetRecordedTouch(i, t);
                    Assert.AreEqual(input.GetRecordedTouch(i), input.GetTouch(i));
                }
            }

        }
    }
}

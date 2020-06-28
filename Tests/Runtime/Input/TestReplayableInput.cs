using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        /// <summary>
        /// <seealso cref="ReplayableInput.Instance"/>
        /// <seealso cref="ReplayableInput.IsReplaying"/>
        /// <seealso cref="ReplayableInput.GetMouseButton(InputDefines.MouseButton)"/>
        /// <seealso cref="ReplayableInput.SetRecordedMouseButton(InputDefines.MouseButton, InputDefines.ButtonCondition)"/>
        /// <seealso cref="ReplayableInput.GetRecordedMouseButton(InputDefines.MouseButton)"/>
        /// <seealso cref="ReplayableInput.MousePos"/>
        /// <seealso cref="ReplayableInput.RecordedMousePos"/>
        /// <seealso cref="ReplayableInput.MousePresent"/>
        /// <seealso cref="ReplayableInput.RecordedMousePresent"/>
        /// <seealso cref="ReplayableInput.MouseScrollDelta"/>
        /// <seealso cref="ReplayableInput.RecordedMouseScrollDelta"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator MouseInputPasses()
        {
            yield return null;

            var input = new ReplayableInput();

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

            {//MousePos
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


        /// <summary>
        /// <seealso cref="ReplayableInput.IsReplaying"/>
        /// <seealso cref="ReplayableInput.TouchSupported"/>
        /// <seealso cref="ReplayableInput.RecordedTouchSupported"/>
        /// <seealso cref="ReplayableInput.MultiTouchEnabled"/>
        /// <seealso cref="ReplayableInput.RecordedMultiTouchEnabled"/>
        /// <seealso cref="ReplayableInput.StylusTouchSupported"/>
        /// <seealso cref="ReplayableInput.RecordedStylusTouchSupported"/>
        /// <seealso cref="ReplayableInput.TouchPressureSupported"/>
        /// <seealso cref="ReplayableInput.RecordedTouchPressureSupported"/>
        /// <seealso cref="ReplayableInput.TouchCount"/>
        /// <seealso cref="ReplayableInput.RecordedTouchCount"/>
        /// <seealso cref="ReplayableInput.GetTouch(int)"/>
        /// <seealso cref="ReplayableInput.SetRecordedTouch(int, Touch)"/>
        /// <seealso cref="ReplayableInput.GetRecordedTouch(int)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator TouchInputPasses()
        {
            yield return null;
            var input = new ReplayableInput();

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

        /// <summary>
        /// <seealso cref="ReplayableInput.AnyKey"/>
        /// <seealso cref="ReplayableInput.AnyKeyDown"/>
        /// <seealso cref="ReplayableInput.ContainsRecordedKeyCode(KeyCode)"/>
        /// <seealso cref="ReplayableInput.GetKey(KeyCode)"/>
        /// <seealso cref="ReplayableInput.GetKeyDown(KeyCode)"/>
        /// <seealso cref="ReplayableInput.GetKeyUp(KeyCode)"/>
        /// <seealso cref="ReplayableInput.SetRecordedKeyButton(KeyCode, InputDefines.ButtonCondition)"/>
        /// <seealso cref="ReplayableInput.GetRecordedKeyButton(KeyCode)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator KeyPasses()
        {
            yield return null;
            var input = new ReplayableInput();

            {//ContainsRecordedKeyCode

                foreach(var keyCode in System.Enum.GetValues(typeof(KeyCode))
                    .GetEnumerable<KeyCode>())
                {
                    var errorMessage = $"Failed KeyCode({keyCode})...";
                    Assert.IsFalse(input.ContainsRecordedKeyCode(keyCode), errorMessage);

                    input.SetRecordedKeyButton(keyCode, InputDefines.ButtonCondition.Down);
                    Assert.IsTrue(input.ContainsRecordedKeyCode(keyCode), errorMessage);

                    input.SetRecordedKeyButton(keyCode, InputDefines.ButtonCondition.Push);
                    Assert.IsTrue(input.ContainsRecordedKeyCode(keyCode), errorMessage);

                    input.SetRecordedKeyButton(keyCode, InputDefines.ButtonCondition.Up);
                    Assert.IsTrue(input.ContainsRecordedKeyCode(keyCode), errorMessage);

                    input.SetRecordedKeyButton(keyCode, InputDefines.ButtonCondition.Free);
                    Assert.IsFalse(input.ContainsRecordedKeyCode(keyCode), errorMessage);
                }
            }
            Debug.Log($"Success to ReplayableInput#RecordedKeyButton Methods");

            {//GetKey
                var keyCode = KeyCode.A;
                Assert.IsFalse(input.ContainsRecordedKeyCode(keyCode));

                input.IsReplaying = false;
                Assert.AreEqual(UnityEngine.Input.GetKey(keyCode), input.GetKey(keyCode));

                input.IsReplaying = true;

                input.SetRecordedKeyButton(keyCode, InputDefines.ButtonCondition.Down);
                Assert.IsTrue(input.GetKey(keyCode));

                input.SetRecordedKeyButton(keyCode, InputDefines.ButtonCondition.Push);
                Assert.IsTrue(input.GetKey(keyCode));

                input.SetRecordedKeyButton(keyCode, InputDefines.ButtonCondition.Up);
                Assert.IsTrue(input.GetKey(keyCode));

                input.SetRecordedKeyButton(keyCode, InputDefines.ButtonCondition.Free);
                Assert.IsFalse(input.GetKey(keyCode));
            }
            Debug.Log($"Success to ReplayableInput#GetKey()");

            {//GetKeyDown
                var keyCode = KeyCode.A;
                input.SetRecordedKeyButton(keyCode, InputDefines.ButtonCondition.Free);
                Assert.IsFalse(input.ContainsRecordedKeyCode(keyCode));

                input.IsReplaying = false;
                Assert.AreEqual(UnityEngine.Input.GetKeyDown(keyCode), input.GetKeyDown(keyCode));

                input.IsReplaying = true;

                input.SetRecordedKeyButton(keyCode, InputDefines.ButtonCondition.Down);
                Assert.IsTrue(input.GetKeyDown(keyCode));

                input.SetRecordedKeyButton(keyCode, InputDefines.ButtonCondition.Push);
                Assert.IsFalse(input.GetKeyDown(keyCode));

                input.SetRecordedKeyButton(keyCode, InputDefines.ButtonCondition.Up);
                Assert.IsFalse(input.GetKeyDown(keyCode));

                input.SetRecordedKeyButton(keyCode, InputDefines.ButtonCondition.Free);
                Assert.IsFalse(input.GetKeyDown(keyCode));
            }
            Debug.Log($"Success to ReplayableInput#GetKeyDown()");

            {//GetKeyUp
                var keyCode = KeyCode.A;
                input.SetRecordedKeyButton(keyCode, InputDefines.ButtonCondition.Free);
                Assert.IsFalse(input.ContainsRecordedKeyCode(keyCode));

                input.IsReplaying = false;
                Assert.AreEqual(UnityEngine.Input.GetKeyUp(keyCode), input.GetKeyUp(keyCode));

                input.IsReplaying = true;

                input.SetRecordedKeyButton(keyCode, InputDefines.ButtonCondition.Down);
                Assert.IsFalse(input.GetKeyUp(keyCode));

                input.SetRecordedKeyButton(keyCode, InputDefines.ButtonCondition.Push);
                Assert.IsFalse(input.GetKeyUp(keyCode));

                input.SetRecordedKeyButton(keyCode, InputDefines.ButtonCondition.Up);
                Assert.IsTrue(input.GetKeyUp(keyCode));

                input.SetRecordedKeyButton(keyCode, InputDefines.ButtonCondition.Free);
                Assert.IsFalse(input.GetKeyUp(keyCode));
            }
            Debug.Log($"Success to ReplayableInput#GetKeyUp()");

            {//anyKey
                input.IsReplaying = false;
                Assert.AreEqual(UnityEngine.Input.anyKey, input.AnyKey);

                input.IsReplaying = true;

                input.SetRecordedKeyButton(KeyCode.A, InputDefines.ButtonCondition.Push);
                Assert.IsTrue(input.AnyKey);

                input.SetRecordedKeyButton(KeyCode.A, InputDefines.ButtonCondition.Free);
                Assert.IsFalse(input.AnyKey);
            }
            Debug.Log($"Success to ReplayableInput#AnyKey()");

            {//anyKeyDown
                input.IsReplaying = false;
                Assert.AreEqual(UnityEngine.Input.anyKeyDown, input.AnyKeyDown);

                input.IsReplaying = true;

                input.SetRecordedKeyButton(KeyCode.A, InputDefines.ButtonCondition.Down);
                Assert.IsTrue(input.AnyKeyDown);

                input.SetRecordedKeyButton(KeyCode.A, InputDefines.ButtonCondition.Free);
                Assert.IsFalse(input.AnyKeyDown);
            }
            Debug.Log($"Success to ReplayableInput#AnyKeyDown()");
        }

        /// <summary>
        /// <seealso cref="ReplayableInput.GetButton(string)"/>
        /// <seealso cref="ReplayableInput.GetButtonDown(string)"/>
        /// <seealso cref="ReplayableInput.GetButtonUp(string)"/>
        /// <seealso cref="ReplayableInput.SetRecordedButton(string, InputDefines.ButtonCondition)"/>
        /// <seealso cref="ReplayableInput.GetRecordedButton(string)"/>
        /// <seealso cref="ReplayableInput.ContainsRecordedButton(string)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator ButtonPasses()
        {
            yield return null;
            var input = new ReplayableInput();
            var buttonNames = new string[] {
                "Fire1",
                "Fire2",
                "Jump",
            };
            
            {//ContainsRecordedButton
                foreach (var buttonName in buttonNames)
                {
                    var errorMessage = $"Failed Button({buttonName})...";
                    Assert.IsFalse(input.ContainsRecordedButton(buttonName), errorMessage);

                    input.SetRecordedButton(buttonName, InputDefines.ButtonCondition.Down);
                    Assert.IsTrue(input.ContainsRecordedButton(buttonName), errorMessage);

                    input.SetRecordedButton(buttonName, InputDefines.ButtonCondition.Push);
                    Assert.IsTrue(input.ContainsRecordedButton(buttonName), errorMessage);

                    input.SetRecordedButton(buttonName, InputDefines.ButtonCondition.Up);
                    Assert.IsTrue(input.ContainsRecordedButton(buttonName), errorMessage);

                    input.SetRecordedButton(buttonName, InputDefines.ButtonCondition.Free);
                    Assert.IsFalse(input.ContainsRecordedButton(buttonName), errorMessage);
                }
            }
            Debug.Log($"Success to ReplayableInput#RecordedButton Methods");

            {//GetButton
                var buttonName = buttonNames[0];
                Assert.IsFalse(input.ContainsRecordedButton(buttonName));

                input.IsReplaying = false;
                Assert.AreEqual(UnityEngine.Input.GetButton(buttonName), input.GetButton(buttonName));

                input.IsReplaying = true;

                input.SetRecordedButton(buttonName, InputDefines.ButtonCondition.Down);
                Assert.IsTrue(input.GetButton(buttonName));

                input.SetRecordedButton(buttonName, InputDefines.ButtonCondition.Push);
                Assert.IsTrue(input.GetButton(buttonName));

                input.SetRecordedButton(buttonName, InputDefines.ButtonCondition.Up);
                Assert.IsTrue(input.GetButton(buttonName));

                input.SetRecordedButton(buttonName, InputDefines.ButtonCondition.Free);
                Assert.IsFalse(input.GetButton(buttonName));
            }
            Debug.Log($"Success to ReplayableInput#GetButton()");

            {//GetButtonDown
                var buttonName = buttonNames[0];
                input.SetRecordedButton(buttonName, InputDefines.ButtonCondition.Free);
                Assert.IsFalse(input.ContainsRecordedButton(buttonName));

                input.IsReplaying = false;
                Assert.AreEqual(UnityEngine.Input.GetButtonDown(buttonName), input.GetButtonDown(buttonName));

                input.IsReplaying = true;

                input.SetRecordedButton(buttonName, InputDefines.ButtonCondition.Down);
                Assert.IsTrue(input.GetButtonDown(buttonName));

                input.SetRecordedButton(buttonName, InputDefines.ButtonCondition.Push);
                Assert.IsFalse(input.GetButtonDown(buttonName));

                input.SetRecordedButton(buttonName, InputDefines.ButtonCondition.Up);
                Assert.IsFalse(input.GetButtonDown(buttonName));

                input.SetRecordedButton(buttonName, InputDefines.ButtonCondition.Free);
                Assert.IsFalse(input.GetButtonDown(buttonName));
            }
            Debug.Log($"Success to ReplayableInput#GetButtonDown()");

            {//GetButtonUp
                var buttonName = buttonNames[0];
                input.SetRecordedButton(buttonName, InputDefines.ButtonCondition.Free);
                Assert.IsFalse(input.ContainsRecordedButton(buttonName));

                input.IsReplaying = false;
                Assert.AreEqual(UnityEngine.Input.GetButtonUp(buttonName), input.GetButtonUp(buttonName));

                input.IsReplaying = true;

                input.SetRecordedButton(buttonName, InputDefines.ButtonCondition.Down);
                Assert.IsFalse(input.GetButtonUp(buttonName));

                input.SetRecordedButton(buttonName, InputDefines.ButtonCondition.Push);
                Assert.IsFalse(input.GetButtonUp(buttonName));

                input.SetRecordedButton(buttonName, InputDefines.ButtonCondition.Up);
                Assert.IsTrue(input.GetButtonUp(buttonName));

                input.SetRecordedButton(buttonName, InputDefines.ButtonCondition.Free);
                Assert.IsFalse(input.GetButtonUp(buttonName));
            }
            Debug.Log($"Success to ReplayableInput#GetButtonUp()");
        }

        /// <summary>
        /// <seealso cref="ReplayableInput.SetRecordedButton(string, InputDefines.ButtonCondition)"/>
        /// <seealso cref="ReplayableInput.GetButtonCondition(string)"/>
        /// </summary>
        [Test]
        public void GetButtonConditionPasses()
        {
            var input = new ReplayableInput();
            input.IsReplaying = true;
            var buttonNames = new string[] {
                "Fire1",
                "Fire2",
                "Jump",
            };

            foreach (var name in buttonNames)
            {
                foreach (var condition in System.Enum.GetValues(typeof(InputDefines.ButtonCondition))
                    .OfType<InputDefines.ButtonCondition>())
                {
                    var errorMessage = $"Failed Button({name}) and Condition({condition})...";
                    input.SetRecordedButton(name, condition);
                    Assert.AreEqual(condition, input.GetButtonCondition(name), errorMessage);
                }
            }
        }

        /// <summary>
        /// <seealso cref="ReplayableInput.GetAxis(string)"/>
        /// <seealso cref="ReplayableInput.ContainsRecordedAxis(string)"/>
        /// <seealso cref="ReplayableInput.GetRecordedAxis(string)"/>
        /// <seealso cref="ReplayableInput.SetRecordedAxis(string, float)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator AxisPasses()
        {
            yield return null;
            var input = new ReplayableInput();
            var axisNames = new string[] {
                "Horizontal",
                "Vertical",
            };

            {//ContainsRecordedButton
                foreach (var buttonName in axisNames)
                {
                    var errorMessage = $"Failed Button({buttonName})...";
                    Assert.IsFalse(input.ContainsRecordedAxis(buttonName), errorMessage);

                    var axis = 1f;
                    input.SetRecordedAxis(buttonName, axis);
                    Assert.AreEqual(axis, input.GetRecordedAxis(buttonName), errorMessage);

                    input.SetRecordedAxis(buttonName, 0f);
                    Assert.IsFalse(input.ContainsRecordedAxis(buttonName), errorMessage);
                    Assert.AreEqual(0f, input.GetRecordedAxis(buttonName), errorMessage);
                }
            }
            Debug.Log($"Success to ReplayableInput#RecordedButton Methods");

            {//GetAxis
                var axisName = axisNames[0];
                Assert.IsFalse(input.ContainsRecordedAxis(axisName));

                input.IsReplaying = false;
                Assert.AreEqual(UnityEngine.Input.GetAxis(axisName), input.GetAxis(axisName));

                input.IsReplaying = true;
                var axis = 1f;
                input.SetRecordedAxis(axisName, axis);
                Assert.AreEqual(axis, input.GetAxis(axisName));

                input.SetRecordedAxis(axisName, 0f);
                Assert.AreEqual(0f, input.GetAxis(axisName));
            }
            Debug.Log($"Success to ReplayableInput#GetAxis()");
        }
    }
}

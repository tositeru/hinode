using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// UnityEngine.Inputをリプレイ可能にしたクラス
    ///
    /// TODO 残りのInputのプロパティ対応
    /// <seealso cref="Hinode.Tests.Input.TestReplayableInput"/>
    /// </summary>
    public class ReplayableInput : ISingleton<ReplayableInput>
    {
        //Input.acceleration;
        //Input.accelerationEventCount;
        //Input.accelerationEvents;
        //Input.GetAccelerationEvent(index)

        //Input.ResetInputAxes(); // <- not support?

        //Input.compositionCursorPos;
        //Input.compositionString;
        //Input.imeCompositionMode;
        //Input.imeIsSelected;
        //Input.inputString;

        //Input.IsJoystickPreconfigured(joystickName); // <- not support
        //Input.GetAxisRaw(axisName); // <- not support

        //Input.gyro;
        //Input.location;

        //Input.deviceOrientation // <- not support?
        //Input.compensateSensors; // <- ?
        //Input.backButtonLeavesApp; // <- not support
        //Input.compass // <- not support

        /// <summary>
        /// trueの時はInputはrecordedXXXプロパティに設定された値を返すようになります
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.MouseInputPasses()"/>
        /// </summary>
        public bool IsReplaying { get; set; } = false;

        public ReplayableInput() { }

        #region Mouse
        readonly InputDefines.ButtonCondition[] _recordedMouseButtons = new InputDefines.ButtonCondition[3] {
                InputDefines.ButtonCondition.Free,
                InputDefines.ButtonCondition.Free,
                InputDefines.ButtonCondition.Free,
            };

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.MouseInputPasses()"/>
        /// </summary>
        public Vector3 RecordedMousePos { get; set; } = Vector3.zero;

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.MouseInputPasses()"/>
        /// </summary>
        public Vector2 RecordedMouseScrollDelta { get; set; } = Vector2.zero;

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.MouseInputPasses()"/>
        /// </summary>
        public bool RecordedMousePresent { get; set; } = false;

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.MouseInputPasses()"/>
        /// </summary>
        /// <param name="btn"></param>
        /// <returns></returns>
        public InputDefines.ButtonCondition GetMouseButton(InputDefines.MouseButton btn)
        {
            if (IsReplaying) return _recordedMouseButtons[(int)btn];
            var index = (int)btn;
            return Input.GetMouseButtonDown(index)
                ? InputDefines.ButtonCondition.Down
                : Input.GetMouseButtonUp(index)
                    ? InputDefines.ButtonCondition.Up
                    : Input.GetMouseButton(index)
                        ? InputDefines.ButtonCondition.Push
                        : InputDefines.ButtonCondition.Free;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.MouseInputPasses()"/>
        /// </summary>
        public bool MousePresent { get => IsReplaying ? RecordedMousePresent : Input.mousePresent; }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.MouseInputPasses()"/>
        /// </summary>
        public Vector3 MousePos
        {
            get => IsReplaying ? RecordedMousePos : Input.mousePosition;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.MouseInputPasses()"/>
        /// </summary>
        public Vector2 MouseScrollDelta
        {
            get => IsReplaying ? RecordedMouseScrollDelta : Input.mouseScrollDelta;
        }

        #region Recorded Value Methods
        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.MouseInputPasses()"/>
        /// </summary>
        /// <param name="button"></param>
        /// <param name="buttonCondition"></param>
        public void SetRecordedMouseButton(InputDefines.MouseButton button, InputDefines.ButtonCondition buttonCondition)
            => SetRecordedMouseButton((int)button, buttonCondition);

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.MouseInputPasses()"/>
        /// </summary>
        /// <param name="button"></param>
        /// <param name="buttonCondition"></param>
        public void SetRecordedMouseButton(int button, InputDefines.ButtonCondition buttonCondition)
        {
            _recordedMouseButtons[button] = buttonCondition;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.MouseInputPasses()"/>
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public InputDefines.ButtonCondition GetRecordedMouseButton(InputDefines.MouseButton button)
            => GetRecordedMouseButton((int)button);

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.MouseInputPasses()"/>
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public InputDefines.ButtonCondition GetRecordedMouseButton(int button)
        {
            return _recordedMouseButtons[button];
        }
        #endregion

        #endregion

        #region Touch
        readonly List<Touch> _recordedTouches = new List<Touch>();

        public int RecordedTouchCount { get; set; } = 0;
        public bool RecordedTouchSupported { get; set; } = false;
        public bool RecordedTouchPressureSupported { get; set; } = false;
        public bool RecordedMultiTouchEnabled { get; set; } = false;
        public bool RecordedSimulateMouseWithTouches { get; set; } = false;
        public bool RecordedStylusTouchSupported { get; set; } = false;

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.TouchInputPasses()"/>
        /// </summary>
        public int TouchCount { get => IsReplaying ? RecordedTouchCount : Input.touchCount; }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.TouchInputPasses()"/>
        /// </summary>
        public bool TouchPressureSupported { get => IsReplaying ? RecordedTouchPressureSupported : Input.touchPressureSupported; }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.TouchInputPasses()"/>
        /// </summary>
        public bool TouchSupported { get => IsReplaying ? RecordedTouchSupported : Input.touchSupported; }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.TouchInputPasses()"/>
        /// </summary>
        public bool MultiTouchEnabled { get => IsReplaying ? RecordedMultiTouchEnabled : Input.multiTouchEnabled; }

        public bool SimulateMouseWithTouches { get => IsReplaying ? RecordedSimulateMouseWithTouches : Input.simulateMouseWithTouches; }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.TouchInputPasses()"/>
        /// </summary>
        public bool StylusTouchSupported { get => IsReplaying ? RecordedStylusTouchSupported : Input.stylusTouchSupported; }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.TouchInputPasses()"/>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Touch GetTouch(int index)
        {
            Assert.IsTrue(0 <= index && index < TouchCount, $"Out of Index... index={index}, TouchCount={TouchCount}");
            if (IsReplaying)
                return GetRecordedTouch(index);
            else
                return Input.GetTouch(index);
        }

        public IEnumerable<Touch> GetTouches()
        {
            if (IsReplaying)
                return _recordedTouches
                    .Zip(Enumerable.Range(0, _recordedTouches.Count), (_t, _i) => (index: _i, touch: _t))
                    .Where(_t => _t.index < TouchCount)
                    .Select(_t => _t.touch);
            else
                return Input.touches;
        }

        #region Recorded Value Methods
        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.TouchInputPasses()"/>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="touch"></param>
        public void SetRecordedTouch(int index, Touch touch)
        {
            ResizeRecordedTouches(index + 1);
            _recordedTouches[index] = touch;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.TouchInputPasses()"/>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Touch GetRecordedTouch(int index)
        {
            ResizeRecordedTouches(index + 1);
            return _recordedTouches[index];
        }

        void ResizeRecordedTouches(int size)
        {
            if (_recordedTouches.Count < size)
            {
                _recordedTouches.Capacity = size;
                while (_recordedTouches.Count < _recordedTouches.Capacity)
                    _recordedTouches.Add(default);
            }
        }
        #endregion

        #endregion

        #region Keyboard
        readonly Dictionary<KeyCode, InputDefines.ButtonCondition> _recordedKeyboardButtons = new Dictionary<KeyCode, InputDefines.ButtonCondition>();

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.KeyPasses()"/>
        /// </summary>
        public bool AnyKey
        {
            get => IsReplaying
                ? _recordedKeyboardButtons.Count > 0
                : Input.anyKey;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.KeyPasses()"/>
        /// </summary>
        public bool AnyKeyDown
        {
            get => IsReplaying
                ? _recordedKeyboardButtons.Any(_t => _t.Value == InputDefines.ButtonCondition.Down)
                : Input.anyKeyDown;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.KeyPasses()"/>
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        public bool GetKey(KeyCode keyCode)
            => IsReplaying
            ? GetRecordedKeyButton(keyCode) != InputDefines.ButtonCondition.Free
            : Input.GetKey(keyCode);

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.KeyPasses()"/>
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        public bool GetKeyDown(KeyCode keyCode)
            => IsReplaying
            ? GetRecordedKeyButton(keyCode) == InputDefines.ButtonCondition.Down
            : Input.GetKeyDown(keyCode);

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.KeyPasses()"/>
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        public bool GetKeyUp(KeyCode keyCode)
            => IsReplaying
            ? GetRecordedKeyButton(keyCode) == InputDefines.ButtonCondition.Up
            : Input.GetKeyUp(keyCode);

        #region Recorded Value And Methods
        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.KeyPasses()"/>
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        public bool ContainsRecordedKeyCode(KeyCode keyCode)
            => _recordedKeyboardButtons.ContainsKey(keyCode);

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.KeyPasses()"/>
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="condition"></param>
        public void SetRecordedKeyButton(KeyCode keyCode, InputDefines.ButtonCondition condition)
        {
            if(condition == InputDefines.ButtonCondition.Free)
            {
                if (_recordedKeyboardButtons.ContainsKey(keyCode))
                {
                    _recordedKeyboardButtons.Remove(keyCode);
                }
            }
            else
            {
                if (!_recordedKeyboardButtons.ContainsKey(keyCode))
                {
                    _recordedKeyboardButtons.Add(keyCode, condition);
                }
                else
                {
                    _recordedKeyboardButtons[keyCode] = condition;
                }
            }
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.KeyPasses()"/>
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        public InputDefines.ButtonCondition GetRecordedKeyButton(KeyCode keyCode)
            => _recordedKeyboardButtons.ContainsKey(keyCode)
            ? _recordedKeyboardButtons[keyCode]
            : InputDefines.ButtonCondition.Free;
        #endregion

        #endregion

        #region Buttons
        //Input.GetButton(name);
        //Input.GetButtonDown(name);
        //Input.GetButtonUp(name);
        readonly Dictionary<string, InputDefines.ButtonCondition> _recordedButtons = new Dictionary<string, InputDefines.ButtonCondition>();

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.ButtonPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool GetButton(string name)
            => IsReplaying
            ? ContainsRecordedButton(name)
            : Input.GetButton(name);

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.ButtonPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool GetButtonDown(string name)
            => IsReplaying
            ? GetRecordedButton(name) == InputDefines.ButtonCondition.Down
            : Input.GetButtonDown(name);

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.ButtonPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool GetButtonUp(string name)
            => IsReplaying
            ? GetRecordedButton(name) == InputDefines.ButtonCondition.Up
            : Input.GetButtonUp(name);

        #region Recorded Value Methods
        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.ButtonPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ContainsRecordedButton(string name)
            => _recordedButtons.ContainsKey(name);

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.ButtonPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="condition"></param>
        public void SetRecordedButton(string name, InputDefines.ButtonCondition condition)
        {
            if (condition == InputDefines.ButtonCondition.Free)
            {
                if (_recordedButtons.ContainsKey(name))
                {
                    _recordedButtons.Remove(name);
                }
            }
            else
            {
                if (!_recordedButtons.ContainsKey(name))
                {
                    _recordedButtons.Add(name, condition);
                }
                else
                {
                    _recordedButtons[name] = condition;
                }
            }
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.ButtonPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public InputDefines.ButtonCondition GetRecordedButton(string name)
            => _recordedButtons.ContainsKey(name)
            ? _recordedButtons[name]
            : InputDefines.ButtonCondition.Free;
        #endregion

        #endregion

        #region Axis
        //Input.GetAxis(axisName);
        readonly Dictionary<string, float> _recordedAxis = new Dictionary<string, float>();

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.AxisPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public float GetAxis(string name)
            => IsReplaying
            ? GetRecordedAxis(name)
            : Input.GetAxis(name);

        #region Recorded Value Methods
        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.AxisPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ContainsRecordedAxis(string name)
            => _recordedAxis.ContainsKey(name);

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.AxisPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="condition"></param>
        public void SetRecordedAxis(string name, float axis)
        {
            if (axis == 0f)
            {
                if (_recordedAxis.ContainsKey(name))
                {
                    _recordedAxis.Remove(name);
                }
            }
            else
            {
                if (!_recordedAxis.ContainsKey(name))
                {
                    _recordedAxis.Add(name, axis);
                }
                else
                {
                    _recordedAxis[name] = axis;
                }
            }
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Input.TestReplayableInput.AxisPasses()"/>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public float GetRecordedAxis(string name)
            => _recordedAxis.ContainsKey(name)
            ? _recordedAxis[name]
            : 0f;
        #endregion

        #endregion

    }
}

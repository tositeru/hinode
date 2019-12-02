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

        //Input.IsJoystickPreconfigured(joystickName);
        //Input.GetAxis(axisName);
        //Input.GetAxisRaw(axisName);

        //Input.GetButton(name);
        //Input.GetButtonDown(name);
        //Input.GetButtonUp(name);

        //Input.anyKey;
        //Input.anyKeyDown;
        //Input.GetKey(keyCode);
        //Input.GetKeyDown(keyCode);
        //Input.GetKeyUp(keyCode);

        //Input.gyro;
        //Input.location;

        //Input.deviceOrientation // <- not support?
        //Input.compensateSensors; // <- ?
        //Input.backButtonLeavesApp; // <- not support
        //Input.compass // <- not support

        readonly InputDefines.ButtonCondition[] _recordedMouseButtons = new InputDefines.ButtonCondition[3] {
                InputDefines.ButtonCondition.Free,
                InputDefines.ButtonCondition.Free,
                InputDefines.ButtonCondition.Free,
            };
        readonly List<Touch> _recordedTouches = new List<Touch>();

        /// <summary>
        /// trueの時はInputはrecordedXXXプロパティに設定された値を返すようになります
        /// </summary>
        public bool IsReplaying { get; set; } = false;

        public Vector3 RecordedMousePos { get; set; } = Vector3.zero;
        public Vector2 RecordedMouseScrollDelta { get; set; } = Vector2.zero;
        public bool RecordedMousePresent { get; set; } = false;

        public int RecordedTouchCount { get; set; } = 0;
        public bool RecordedTouchSupported { get; set; } = false;
        public bool RecordedTouchPressureSupported { get; set; } = false;
        public bool RecordedMultiTouchEnabled { get; set; } = false;
        public bool RecordedSimulateMouseWithTouches { get; set; } = false;
        public bool RecordedStylusTouchSupported { get; set; } = false;

        public ReplayableInput() { }

        #region Mouse
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

        public bool MousePresent { get => IsReplaying ? RecordedMousePresent : Input.mousePresent; }

        public Vector3 MousePos
        {
            get => IsReplaying ? RecordedMousePos : Input.mousePosition;
        }
        public Vector2 MouseScrollDelta
        {
            get => IsReplaying ? RecordedMouseScrollDelta : Input.mouseScrollDelta;
        }

        #region Recorded Value Methods
        public void SetRecordedMouseButton(InputDefines.MouseButton button, InputDefines.ButtonCondition buttonCondition)
            => SetRecordedMouseButton((int)button, buttonCondition);

        public void SetRecordedMouseButton(int button, InputDefines.ButtonCondition buttonCondition)
        {
            _recordedMouseButtons[button] = buttonCondition;
        }

        public InputDefines.ButtonCondition GetRecordedMouseButton(InputDefines.MouseButton button)
            => GetRecordedMouseButton((int)button);

        public InputDefines.ButtonCondition GetRecordedMouseButton(int button)
        {
            return _recordedMouseButtons[button];
        }
        #endregion

        #endregion

        #region Touch
        public int TouchCount { get => IsReplaying ? RecordedTouchCount : Input.touchCount; }
        public bool TouchPressureSupported { get => IsReplaying ? RecordedTouchPressureSupported : Input.touchPressureSupported; }
        public bool TouchSupported { get => IsReplaying ? RecordedTouchSupported : Input.touchSupported; }
        public bool MultiTouchEnabled { get => IsReplaying ? RecordedMultiTouchEnabled : Input.multiTouchEnabled; }
        public bool SimulateMouseWithTouches { get => IsReplaying ? RecordedSimulateMouseWithTouches : Input.simulateMouseWithTouches; }
        public bool StylusTouchSupported { get => IsReplaying ? RecordedStylusTouchSupported : Input.stylusTouchSupported; }

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
        public void SetRecordedTouch(int index, Touch touch)
        {
            ResizeRecordedTouches(index + 1);
            _recordedTouches[index] = touch;
        }

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
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hinode
{
    /// <summary>
    /// 入力データの再生ができるようにしたBaseInput
    ///
    /// TODO　まだ未検証
    /// TODO BaseInput#GetAxisRaw, BaseInput#GetButtonDownの対応
    /// </summary>
    public class ReplayableBaseInput : BaseInput
    {
        InputDefines.ButtonCondition[] _mouseButtons = new InputDefines.ButtonCondition[3] {
                InputDefines.ButtonCondition.Free,
                InputDefines.ButtonCondition.Free,
                InputDefines.ButtonCondition.Free,
            };
        List<Touch> _touches = new List<Touch>();

        public IMECompositionMode recordedIMECompositionMode { get; set;}
        public string recordedCompositionString { get; set; }
        public Vector2 recordedCompositionCursorPos { get; set; }
        public bool recordedMousePresent { get; set; }
        
        public Vector2 recordedMousePosition { get; set; }
        public Vector2 recordedMouseScrollDelta { get; set; }
        public bool recordedTouchSupported { get; set; }
        public int recordedTouchCount { get; set; }

        /// <summary>
        /// trueの時はBaseInputプロパティはrecordedXXXプロパティに設定された値を返すようになります
        /// </summary>
        public bool IsReplaying { get; set; }

        public void SetRecordedMouseButton(InputDefines.MouseButton button, InputDefines.ButtonCondition buttonCondition)
            => SetRecordedMouseButton((int)button, buttonCondition);

        public void SetRecordedMouseButton(int button, InputDefines.ButtonCondition buttonCondition)
        {
            _mouseButtons[button] = buttonCondition;
        }

        public InputDefines.ButtonCondition GetRecordedMouseButton(InputDefines.MouseButton button)
            => GetRecordedMouseButton((int)button);

        public InputDefines.ButtonCondition GetRecordedMouseButton(int button)
        {
            return _mouseButtons[button];
        }

        public void SetRecordedTouch(int index, Touch touch)
        {
            ResizeRecordedTouches(index+1);
            _touches[index] = touch;
        }

        public Touch GetRecordedTouch(int index)
        {
            ResizeRecordedTouches(index+1);
            return _touches[index];
        }

        void ResizeRecordedTouches(int size)
        {
            if (_touches.Count < size)
            {
                _touches.Capacity = size;
                while (_touches.Count < _touches.Capacity)
                    _touches.Add(default);
            }
        }

        #region 再生データを画面に表示するためのもの
        InputViewer _replayVeiwer;
        #endregion

        #region BaseInput override
        /// <summary>
        /// Interface to Input.compositionString. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public override string compositionString
        {
            get { return IsReplaying ? recordedCompositionString : Input.compositionString; }
        }

        /// <summary>
        /// このプロパティで値を変更したものは再生中のデータには反映されません。
        /// その際はrecordedIMECompositionModeを使用してください。
        /// </summary>
        public override IMECompositionMode imeCompositionMode
        {
            get { return IsReplaying ? recordedIMECompositionMode : Input.imeCompositionMode; }
            set { Input.imeCompositionMode = value; }
        }

        /// <summary>
        /// このプロパティで値を変更したものは再生中のデータには反映されません。
        /// その際はrecorededCompositionCursorPosを使用してください。
        /// </summary>
        public override Vector2 compositionCursorPos
        {
            get { return IsReplaying ? recordedCompositionCursorPos : Input.compositionCursorPos; }
            set { Input.compositionCursorPos = value; }
        }

        /// <summary>
        /// Interface to Input.mousePresent. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public override bool mousePresent
        {
            get { return IsReplaying ? recordedMousePresent : Input.mousePresent; }
        }

        /// <summary>
        /// Interface to Input.GetMouseButtonDown. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public override bool GetMouseButtonDown(int button)
        {
            if (IsReplaying)
                return _mouseButtons[button] == InputDefines.ButtonCondition.Down;
            else
                return Input.GetMouseButtonDown(button);
        }

        /// <summary>
        /// Interface to Input.GetMouseButtonUp. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public override bool GetMouseButtonUp(int button)
        {
            if (IsReplaying)
                return _mouseButtons[button] == InputDefines.ButtonCondition.Up;
            else
                return Input.GetMouseButtonUp(button);
        }

        /// <summary>
        /// Interface to Input.GetMouseButton. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public override bool GetMouseButton(int button)
        {
            if (IsReplaying)
                return _mouseButtons[button] == InputDefines.ButtonCondition.Push;
            else
                return Input.GetMouseButton(button);
        }

        /// <summary>
        /// Interface to Input.mousePosition. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public override Vector2 mousePosition
        {
            get { return IsReplaying ? recordedMousePosition : base.mousePosition; }
        }

        /// <summary>
        /// Interface to Input.mouseScrollDelta. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public override Vector2 mouseScrollDelta
        {
            get { return IsReplaying ? recordedMouseScrollDelta : Input.mouseScrollDelta; }
        }

        /// <summary>
        /// Interface to Input.touchSupported. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public override bool touchSupported
        {
            get { return IsReplaying ? recordedTouchSupported : Input.touchSupported; }
        }

        /// <summary>
        /// Interface to Input.touchCount. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public override int touchCount
        {
            get { return IsReplaying ? recordedTouchCount : Input.touchCount; }
        }

        /// <summary>
        /// Interface to Input.GetTouch. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        /// <param name="index">Touch index to get</param>
        public override Touch GetTouch(int index)
        {
            if (IsReplaying)
                return GetRecordedTouch(index);
            else
                return Input.GetTouch(index);
        }
        #region まだ未対応 アナログスティックやパッドなどの入力データ
        /// <summary>
        /// Interface to Input.GetAxisRaw. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        /// <param name="axisName">Axis name to check</param>
        public override float GetAxisRaw(string axisName)
        {
            if (IsReplaying)
                throw new System.NotImplementedException("not implement yet...");
            else
                return Input.GetAxisRaw(axisName);
        }

        /// <summary>
        /// Interface to Input.GetButtonDown. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        /// <param name="buttonName">Button name to get</param>
        public override bool GetButtonDown(string buttonName)
        {
            if (IsReplaying)
                throw new System.NotImplementedException("not implement yet...");
            else
                return Input.GetButtonDown(buttonName);
        }
        #endregion

        #endregion
    }
}
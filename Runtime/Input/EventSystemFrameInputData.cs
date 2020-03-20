using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Hinode
{
    /// <summary>
    /// UnityEngine.EventSystemの1フレームにおける入力データを記録するためのもの
    /// 一つ前と変化がないデータはシリアライズの対象にならないようになっています。
    /// </summary>
    [System.Serializable, HasKeyAndTypeDictionaryGetter(typeof(EventSystemFrameInputData))]
    public class EventSystemFrameInputData : InputRecorder.IFrameDataRecorder, ISerializable
    {
        public static readonly int LIMIT_TOUCH_COUNT = 8;

        [SerializeField] UpdateObserver<string> _compositionString = new UpdateObserver<string>();
        [SerializeField] UpdateObserver<IMECompositionMode> _imeCompositionMode = new UpdateObserver<IMECompositionMode>();
        [SerializeField] UpdateObserver<Vector2> _compositionCursorPos = new UpdateObserver<Vector2>();
        [SerializeField] UpdateObserver<bool> _mousePresent = new UpdateObserver<bool>();
        [SerializeField]
        UpdateObserver<InputDefines.ButtonCondition>[] _mouseButtons = new UpdateObserver<InputDefines.ButtonCondition>[3] {
                new UpdateObserver<InputDefines.ButtonCondition>(InputDefines.ButtonCondition.Free),
                new UpdateObserver<InputDefines.ButtonCondition>(InputDefines.ButtonCondition.Free),
                new UpdateObserver<InputDefines.ButtonCondition>(InputDefines.ButtonCondition.Free),
            };
        [SerializeField] UpdateObserver<Vector2> _mousePosition = new UpdateObserver<Vector2>();
        [SerializeField] UpdateObserver<Vector2> _mouseScrollDelta = new UpdateObserver<Vector2>();
        [SerializeField] UpdateObserver<bool> _touchSupported = new UpdateObserver<bool>();
        [SerializeField] UpdateObserver<int> _touchCount = new UpdateObserver<int>();
        [SerializeField] TouchUpdateObserver[] _touches;

        JsonSerializer _jsonSerializer = new JsonSerializer();

        public string CompositionString
        {
            get => _compositionString.Value;
            set { _compositionString.Value = value; }
        }

        public IMECompositionMode IMECompositionMode
        {
            get => _imeCompositionMode.Value;
            set => _imeCompositionMode.Value = value;
        }

        public Vector2 CompositionCursorPos
        {
            get => _compositionCursorPos.Value;
            set => _compositionCursorPos.Value = value;
        }
        public bool MousePresent
        {
            get => _mousePresent.Value;
            set => _mousePresent.Value = value;
        }

        public void SetMouseButton(InputDefines.MouseButton btnType, InputDefines.ButtonCondition condition)
        {
            _mouseButtons[(int)btnType].Value = condition;
        }

        public InputDefines.ButtonCondition GetMouseButton(InputDefines.MouseButton btnType)
        {
            return _mouseButtons[(int)btnType].Value;
        }

        public Vector2 MousePosition
        {
            get => _mousePosition.Value;
            set => _mousePosition.Value = value;
        }

        public Vector2 MouseScrollDelta
        {
            get => _mouseScrollDelta.Value;
            set => _mouseScrollDelta.Value = value;
        }

        public bool TouchSupported
        {
            get => _touchSupported.Value;
            set => _touchSupported.Value = value;
        }

        public int TouchCount
        {
            get => _touchCount.Value;
            set
            {
                _touchCount.Value = value;
            }
        }

        public TouchUpdateObserver GetTouch(int index)
        {
            return Touches[index];
        }
        public void SetTouch(int index, TouchUpdateObserver t)
        {
            Touches[index].Update(t);
        }
        public void SetTouch(int index, Touch t)
        {
            Touches[index].Update(t);
        }

        TouchUpdateObserver[] Touches
        {
            get
            {
                if(null == _touches)
                {
                    _touches = new TouchUpdateObserver[LIMIT_TOUCH_COUNT];
                    for (var i=0; i<LIMIT_TOUCH_COUNT; ++i)
                    {
                        _touches[i] = new TouchUpdateObserver();
                    }
                }
                return _touches;
            }
        }

        public EventSystemFrameInputData()
        {
        }

        public void Reset()
        {
            _compositionString.Reset();
            _imeCompositionMode.Reset();
            _compositionCursorPos.Reset();

            _mousePresent.Reset();
            _mousePosition.Reset();
            _mouseScrollDelta.Reset();
            foreach (var btn in _mouseButtons) { btn.Reset(); }
            _touchSupported.Reset();
            _touchCount.Reset();
            foreach (var t in Touches) { t.Reset(); }
        }

        /// <summary>
        /// 他のインスタンスに自身の値を設定する。
        ///
        /// 設定される値は更新済みのものだけになります。
        /// </summary>
        /// <param name="other"></param>
        public void UpdateTo(EventSystemFrameInputData other)
        {
            //ime
            if (_compositionString.DidUpdated) other._compositionString.Value = _compositionString.Value;
            if (_imeCompositionMode.DidUpdated) other._imeCompositionMode.Value = _imeCompositionMode.Value;
            if (_compositionCursorPos.DidUpdated) other._compositionCursorPos.Value = _compositionCursorPos.Value;
            //mouse
            if (_mousePresent.DidUpdated) other._mousePresent.Value = _mousePresent.Value;
            for (var i = 0; i < _mouseButtons.Length; ++i)
            {
                if (_mouseButtons[i].DidUpdated) other._mouseButtons[i].Value = _mouseButtons[i].Value;
            }
            if (_mousePosition.DidUpdated) other._mousePosition.Value = _mousePosition.Value;
            if (_mouseScrollDelta.DidUpdated) other._mouseScrollDelta.Value = _mouseScrollDelta.Value;
            //touch
            if (_touchSupported.DidUpdated) other._touchSupported.Value = _touchSupported.Value;
            if (_touchCount.DidUpdated) other._touchCount.Value = _touchCount.Value;
            for (var i = 0; i < _touchCount.Value; ++i)
            {
                if (Touches[i].DidUpdated) other.Touches[i] = Touches[i];
            }
        }

        #region InputRecorder.IFrameDataRecorder
        public void ClearRecorder()
        {
            _compositionString.SetDefaultValue(true);
            _imeCompositionMode.SetDefaultValue(true);
            _compositionCursorPos.SetDefaultValue(true);

            _mousePresent.SetDefaultValue(true);
            _mousePosition.SetDefaultValue(true);
            _mouseScrollDelta.SetDefaultValue(true);
            foreach (var btn in _mouseButtons) { btn.SetDefaultValue(true); }
            _touchSupported.SetDefaultValue(true);
            _touchCount.SetDefaultValue(true);
            foreach (var t in Touches) { t.SetDefaultValue(true); }
        }

        public InputRecord.Frame Update(BaseInput baseInput)
        {
            Reset();

            {//値の更新
                CompositionString = baseInput.compositionString;
                IMECompositionMode = baseInput.imeCompositionMode;
                CompositionCursorPos = baseInput.compositionCursorPos;

                MousePresent = baseInput.mousePresent;
                MousePosition = baseInput.mousePosition;
                MouseScrollDelta = baseInput.mouseScrollDelta;
                foreach (var btn in System.Enum.GetValues(typeof(InputDefines.MouseButton)).OfType<InputDefines.MouseButton>())
                {
                    InputDefines.ButtonCondition condition = InputDefines.ButtonCondition.Free;
                    if (baseInput.GetMouseButtonDown((int)btn))
                    {
                        condition = InputDefines.ButtonCondition.Down;
                    }
                    else if (baseInput.GetMouseButtonUp((int)btn))
                    {
                        condition = InputDefines.ButtonCondition.Up;
                    }
                    else if (baseInput.GetMouseButton((int)btn))
                    {
                        condition = InputDefines.ButtonCondition.Push;
                    }
                    SetMouseButton(btn, condition);
                }

                TouchSupported = baseInput.touchSupported;
                TouchCount = baseInput.touchCount;
                for (var i = 0; i < baseInput.touchCount; ++i)
                {
                    var t = baseInput.GetTouch(i);
                    SetTouch(i, t);
                }
            }

            var frame = new InputRecord.Frame();
            frame.InputText = _jsonSerializer.Serialize(this);
            return frame;
        }

        /// <summary>
        /// フレームのデータを復元する
        /// </summary>
        /// <param name="baseInput"></param>
        /// <param name="frame"></param>
        public void RecoverFrame(ReplayableBaseInput baseInput, InputRecord.Frame frame)
        {
            var recoverInput = _jsonSerializer.Deserialize<EventSystemFrameInputData>(frame.InputText);

            recoverInput.UpdateTo(this);

            baseInput.recordedCompositionString = CompositionString;
            baseInput.recordedIMECompositionMode = IMECompositionMode;
            baseInput.recordedCompositionCursorPos = CompositionCursorPos;

            baseInput.recordedMousePresent = MousePresent;
            baseInput.recordedMousePosition = MousePosition;
            baseInput.recordedMouseScrollDelta = MouseScrollDelta;
            foreach (var btn in System.Enum.GetValues(typeof(InputDefines.MouseButton)).OfType<InputDefines.MouseButton>())
            {
                baseInput.SetRecordedMouseButton((int)btn, GetMouseButton(btn));
            }

            baseInput.recordedTouchSupported = TouchSupported;
            baseInput.recordedTouchCount = TouchCount;
            for (var i = 0; i < TouchCount; ++i)
            {
                baseInput.SetRecordedTouch(i, (Touch)GetTouch(i));
            }
        }

        #endregion

        #region ISerializable
        const string KeyCompositionString = "comStr";
        const string KeyIMECompositionMode = "comMode";
        const string KeyCompositionCursorPos = "comCurPos";
        const string KeyMousePresent = "musPre";
        const string KeyMouseButton = "mus";
        const string KeyMousePosition = "musPos";
        const string KeyMouseScrollDelta = "musSclDel";
        const string KeyTouchSupported = "tchSpt";
        const string KeyTouchCount = "tchCnt";
        const string KeyTouch = "tch";

        static Dictionary<string, System.Type> _keyAndTypeDict;
        [KeyAndTypeDictionaryGetter]
        public static IReadOnlyDictionary<string, System.Type> GetKeyAndTypeDictionary()
        {
            if (_keyAndTypeDict == null)
            {
                _keyAndTypeDict = new Dictionary<string, System.Type>
                {
                    { KeyCompositionString, typeof(string) },
                    { KeyIMECompositionMode, typeof(IMECompositionMode) },
                    { KeyCompositionCursorPos, typeof(string) },
                    { KeyMousePresent, typeof(bool) },
                    { KeyMousePosition, typeof(string)},
                    { KeyMouseScrollDelta, typeof(string) },
                    { KeyTouchSupported, typeof(bool) },
                    { KeyTouchCount, typeof(int) },
                };
                foreach(var btn in System.Enum.GetValues(typeof(InputDefines.MouseButton)).OfType<InputDefines.MouseButton>())
                {
                    _keyAndTypeDict.Add(KeyMouseButton + ((int)btn).ToString(), typeof(InputDefines.ButtonCondition));
                }
                for (var i=0; i<LIMIT_TOUCH_COUNT; ++i)
                {
                    _keyAndTypeDict.Add(KeyTouch + i.ToString(), typeof(TouchUpdateObserver));
                }
            }
            return _keyAndTypeDict;
        }

        public EventSystemFrameInputData(SerializationInfo info, StreamingContext context)
            : this()
        {
            var e = info.GetEnumerator();
            while (e.MoveNext())
            {
                switch (e.Name)
                {
                    case KeyCompositionString: _compositionString.Value = (string)e.Value; break;
                    case KeyIMECompositionMode: _imeCompositionMode.Value = (IMECompositionMode)e.Value; break;
                    case KeyCompositionCursorPos:
                        if (Vector2Extensions.TryParse((string)e.Value, out var v))
                            _compositionCursorPos.Value = v;
                        break;
                    case KeyMousePresent: _mousePresent.Value = (bool)e.Value; break;
                    case KeyMousePosition:
                        if (Vector2Extensions.TryParse((string)e.Value, out v))
                            _mousePosition.Value = v;
                        break;
                    case KeyMouseScrollDelta:
                        if (Vector2Extensions.TryParse((string)e.Value, out v))
                            _mouseScrollDelta.Value = v;
                        break;
                    case KeyTouchSupported: _touchSupported.Value = (bool)e.Value; break;
                    case KeyTouchCount: _touchCount.Value = (int)e.Value; break;
                    default:
                        if (0 == e.Name.IndexOf(KeyMouseButton))
                        {
                            if (int.TryParse(e.Name.Substring(KeyMouseButton.Length), out var index))
                            {
                                SetMouseButton((InputDefines.MouseButton)index, (InputDefines.ButtonCondition)e.Value);
                            }
                        }
                        else if (0 == e.Name.IndexOf(KeyTouch))
                        {
                            if (int.TryParse(e.Name.Substring(KeyTouch.Length), out var index))
                            {
                                SetTouch(index, (TouchUpdateObserver)e.Value);
                            }
                        }
                        break;
                }

            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //ime
            if (_compositionString.DidUpdated) info.AddValue(KeyCompositionString, _compositionString.Value);
            if (_imeCompositionMode.DidUpdated) info.AddValue(KeyIMECompositionMode, _imeCompositionMode.Value);
            if (_compositionCursorPos.DidUpdated) info.AddValue(KeyCompositionCursorPos, _compositionCursorPos.Value.ToString("F4"));
            //mouse
            if (_mousePresent.DidUpdated) info.AddValue(KeyMousePresent, _mousePresent.Value);
            for (var i = 0; i < _mouseButtons.Length; ++i)
            {
                if (_mouseButtons[i].DidUpdated) info.AddValue(KeyMouseButton + i.ToString(), _mouseButtons[i].Value);
            }
            if (_mousePosition.DidUpdated) info.AddValue(KeyMousePosition, _mousePosition.Value.ToString("F4"));
            if (_mouseScrollDelta.DidUpdated) info.AddValue(KeyMouseScrollDelta, _mouseScrollDelta.Value.ToString("F4"));
            //touch
            if (_touchSupported.DidUpdated) info.AddValue(KeyTouchSupported, _touchSupported.Value);
            if (_touchCount.DidUpdated) info.AddValue(KeyTouchCount, _touchCount.Value);
            for (var i = 0; i < _touchCount.Value; ++i)
            {
                if (Touches[i].DidUpdated) info.AddValue(KeyTouch + i.ToString(), Touches[i]);
            }
        }
        #endregion

        #region IEnumerable
        public IEnumerable<IUpdateObserver> GetUpdatedValueEnumerable()
        {
            return new UpdatedValueEnumerable(this);
        }

        class UpdatedValueEnumerable : IEnumerable<IUpdateObserver>, IEnumerable
        {
            EventSystemFrameInputData _target;
            public UpdatedValueEnumerable(EventSystemFrameInputData target)
            {
                _target = target;
            }

            public IEnumerator<IUpdateObserver> GetEnumerator()
            {
                return new Enumerator(_target);
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            class Enumerator : IEnumerator<IUpdateObserver>, IEnumerator, System.IDisposable
            {
                EventSystemFrameInputData _target;
                IEnumerator<IUpdateObserver> _enmuerator;

                public Enumerator(EventSystemFrameInputData target)
                {
                    _target = target;
                    Reset();
                }
                public IUpdateObserver Current => _enmuerator.Current;
                object IEnumerator.Current => Current;
                public void Dispose() { }
                public bool MoveNext() => _enmuerator.MoveNext();
                public void Reset() => _enmuerator = GetEnumerator();

                IEnumerator<IUpdateObserver> GetEnumerator()
                {
                    if (_target._compositionString.DidUpdated) yield return _target._compositionString;
                    if (_target._compositionCursorPos.DidUpdated) yield return _target._compositionCursorPos;
                    if (_target._imeCompositionMode.DidUpdated) yield return _target._imeCompositionMode;

                    if (_target._mousePresent.DidUpdated) yield return _target._mousePresent;
                    if (_target._mousePosition.DidUpdated) yield return _target._mousePosition;
                    if (_target._mouseScrollDelta.DidUpdated) yield return _target._mouseScrollDelta;

                    foreach (var btn in _target._mouseButtons.Where(_b => _b.DidUpdated))
                    {
                        yield return btn;
                    }

                    if (_target._touchSupported.DidUpdated) yield return _target._touchSupported;
                    if (_target._touchCount.DidUpdated) yield return _target._touchCount;

                    foreach (var t in _target.Touches.Where(_b => _b.DidUpdated))
                    {
                        yield return t;
                    }
                }
            }
        }

        #endregion
    }

}

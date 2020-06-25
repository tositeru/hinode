using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Hinode.Serialization;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// UnityEngine.EventSystemの1フレームにおける入力データを記録するためのもの
    /// 一つ前と変化がないデータはシリアライズの対象にならないようになっています。
    /// </summary>
    [System.Serializable, ContainsSerializationKeyTypeGetter(typeof(FrameInputData))]
    public class FrameInputData : InputRecorder.IFrameDataRecorder, ISerializable
    {
        public static readonly int LIMIT_TOUCH_COUNT = 16;

        public class KeyValue
        {
            public string Key { get; }
            public IUpdateObserver Value { get; }
            public KeyValue(string key, IUpdateObserver value)
            {
                Key = key;
                Value = value;
            }

            public static implicit operator KeyValue((string key, IUpdateObserver value) data)
                => new KeyValue(data.key, data.value);
            public static implicit operator (string key, IUpdateObserver value)(KeyValue data)
                => (data.Key, data.Value);
        }

        public interface IChildFrameInputData
        {
            void ResetInputDatas();
            void UpdateInputDatas(ReplayableInput input);
            IEnumerable<KeyValue> GetValuesEnumerable();
        }

        [SerializeField] UpdateObserver<bool> _mousePresent = new UpdateObserver<bool>();
        [SerializeField]
        UpdateObserver<InputDefines.ButtonCondition>[] _mouseButtons = new UpdateObserver<InputDefines.ButtonCondition>[3] {
                new UpdateObserver<InputDefines.ButtonCondition>(InputDefines.ButtonCondition.Free),
                new UpdateObserver<InputDefines.ButtonCondition>(InputDefines.ButtonCondition.Free),
                new UpdateObserver<InputDefines.ButtonCondition>(InputDefines.ButtonCondition.Free),
            };
        [SerializeField] UpdateObserver<Vector3> _mousePosition = new UpdateObserver<Vector3>();
        [SerializeField] UpdateObserver<Vector2> _mouseScrollDelta = new UpdateObserver<Vector2>();

        [SerializeField] UpdateObserver<bool> _touchSupported = new UpdateObserver<bool>();
        [SerializeField] UpdateObserver<int> _touchCount = new UpdateObserver<int>();
        [SerializeField] TouchUpdateObserver[] _touches;

        [SerializeField] Dictionary<KeyCode, UpdateObserver<InputDefines.ButtonCondition>> _keyButtonsDict = new Dictionary<KeyCode, UpdateObserver<InputDefines.ButtonCondition>>();

        JsonSerializer _jsonSerializer = new JsonSerializer();

        #region Mouse Property
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

        public Vector3 MousePosition
        {
            get => _mousePosition.Value;
            set => _mousePosition.Value = value;
        }

        public Vector2 MouseScrollDelta
        {
            get => _mouseScrollDelta.Value;
            set => _mouseScrollDelta.Value = value;
        }
        #endregion

        #region Touch
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
                if (null == _touches)
                {
                    _touches = new TouchUpdateObserver[LIMIT_TOUCH_COUNT];
                    for (var i = 0; i < LIMIT_TOUCH_COUNT; ++i)
                    {
                        _touches[i] = new TouchUpdateObserver();
                    }
                }
                return _touches;
            }
        }
        #endregion

        #region Key Buttons
        //_keyButtonsDict
        public void SetKeyButton(KeyCode keyCode, InputDefines.ButtonCondition condition)
        {
            if(!_keyButtonsDict.ContainsKey(keyCode))
            {
                _keyButtonsDict.Add(keyCode, new UpdateObserver<InputDefines.ButtonCondition>(condition));
            }
            _keyButtonsDict[keyCode].Value = condition;
        }
        public InputDefines.ButtonCondition GetKeyButton(KeyCode keyCode)
            => _keyButtonsDict.ContainsKey(keyCode)
            ? _keyButtonsDict[keyCode].Value
            : InputDefines.ButtonCondition.Free;

        public IReadOnlyDictionary<KeyCode, UpdateObserver<InputDefines.ButtonCondition>> KeyButtons { get => _keyButtonsDict; }
        #endregion

        public FrameInputData()
        {
        }

        public void Reset()
        {
            _mousePresent.Reset();
            _mousePosition.Reset();
            _mouseScrollDelta.Reset();
            foreach (var btn in _mouseButtons) { btn.Reset(); }

            _touchSupported.Reset();
            _touchCount.Reset();
            foreach (var t in Touches) { t.Reset(); }

            foreach(var key in _keyButtonsDict.Values)
            {
                key.Reset();
            }
        }

        /// <summary>
        /// 他のインスタンスに自身の値を設定する。
        ///
        /// 設定される値は更新済みのものだけになります。
        /// </summary>
        /// <param name="other"></param>
        public void UpdateTo(FrameInputData other)
        {
            //ime
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
            //key
            foreach (var (keyCode, condition) in _keyButtonsDict
                .Where(_t => _t.Value.DidUpdated)
                .Select(_t => (keyCode: _t.Key, condition: _t.Value)))
            {
                other.SetKeyButton(keyCode, condition.Value);
            }
        }

        #region InputRecorder.IFrameDataRecorder
        /// <summary>
        /// <see cref="InputRecorder.IFrameDataRecorder.ResetDatas()"/>
        /// </summary>
        public void ResetDatas()
        {
            _mousePresent.SetDefaultValue(true);
            _mousePosition.SetDefaultValue(true);
            _mouseScrollDelta.SetDefaultValue(true);
            foreach (var btn in _mouseButtons) { btn.SetDefaultValue(true); }
            _touchSupported.SetDefaultValue(true);
            _touchCount.SetDefaultValue(true);
            foreach (var t in Touches) { t.SetDefaultValue(true); }

            _keyButtonsDict.Clear();
        }

        /// <summary>
        /// <see cref="InputRecorder.IFrameDataRecorder.Update(ReplayableInput)"/>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public InputRecord.Frame Update(ReplayableInput input)
        {
            Reset();

            {//値の更新
                MousePresent = input.MousePresent;
                MousePosition = input.MousePos;
                MouseScrollDelta = input.MouseScrollDelta;
                foreach (var btn in System.Enum.GetValues(typeof(InputDefines.MouseButton)).OfType<InputDefines.MouseButton>())
                {
                    InputDefines.ButtonCondition condition = input.GetMouseButton(btn);
                    SetMouseButton(btn, condition);
                }

                TouchSupported = input.TouchSupported;
                TouchCount = input.TouchCount;
                for (var i = 0; i < input.TouchCount; ++i)
                {
                    var t = input.GetTouch(i);
                    SetTouch(i, t);
                }

                if(input.AnyKey)
                {
                    foreach(var keyCode in System.Enum.GetValues(typeof(KeyCode)).OfType<KeyCode>())
                    {
                        var condition = input.GetKeyDown(keyCode)
                            ? InputDefines.ButtonCondition.Down
                            : input.GetKeyUp(keyCode)
                                ? InputDefines.ButtonCondition.Up
                                : input.GetKey(keyCode)
                                    ? InputDefines.ButtonCondition.Push
                                    : InputDefines.ButtonCondition.Free;
                        SetKeyButton(keyCode, condition);
                    }
                }
                else
                {
                    foreach(var condition in _keyButtonsDict.Values
                        .Where(_c => _c.Value == InputDefines.ButtonCondition.Free))
                    {
                        condition.Value = InputDefines.ButtonCondition.Free;
                    }
                }
            }

            var frame = new InputRecord.Frame();
            frame.InputText = _jsonSerializer.Serialize(this);
            return frame;
        }

        /// <summary>
        /// フレームのデータを復元します
        /// この関数を呼び出した後は、このインスタンスとReplayableInputのパラメータがFrameのものへ更新されます。
        /// <see cref="InputRecorder.IFrameDataRecorder.RecoverFrame(ReplayableInput, InputRecord.Frame)"/>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="frame"></param>
        public void RecoverFrame(ReplayableInput input, InputRecord.Frame frame)
        {
            var recoverInput = _jsonSerializer.Deserialize<FrameInputData>(frame.InputText);

            recoverInput.UpdateTo(this);

            input.RecordedMousePresent = MousePresent;
            input.RecordedMousePos = MousePosition;
            input.RecordedMouseScrollDelta = MouseScrollDelta;
            foreach (var btn in System.Enum.GetValues(typeof(InputDefines.MouseButton)).OfType<InputDefines.MouseButton>())
            {
                input.SetRecordedMouseButton((int)btn, GetMouseButton(btn));
            }

            input.RecordedTouchSupported = TouchSupported;
            input.RecordedTouchCount = TouchCount;
            for (var i = 0; i < TouchCount; ++i)
            {
                input.SetRecordedTouch(i, (Touch)GetTouch(i));
            }

            foreach(var (keyCode, condition) in _keyButtonsDict.Select(_t => (_t.Key, _t.Value)))
            {
                input.SetRecordedKeyButton(keyCode, condition.Value);
            }
        }

        #endregion

        #region ISerializable
        //Mouse
        const string KeyMousePresent = "musPre";
        const string KeyMouseButton = "mus";
        const string KeyMousePosition = "musPos";
        const string KeyMouseScrollDelta = "musSclDel";

        //Touch
        const string KeyTouchSupported = "tchSpt";
        const string KeyTouchCount = "tchCnt";
        const string KeyTouch = "tch";
        const string KeyButtonPrefix = "k";

        static Dictionary<string, System.Type> _keyAndTypeDict;
        [SerializationKeyTypeGetter]
        public static System.Type GetKeyAndTypeDictionary(string key)
        {
            throw new System.NotImplementedException();
            //if (_keyAndTypeDict == null)
            //{
            //    _keyAndTypeDict = new Dictionary<string, System.Type>
            //    {
            //        { KeyMousePresent, typeof(bool) },
            //        { KeyMousePosition, typeof(string)},
            //        { KeyMouseScrollDelta, typeof(string) },
            //        { KeyTouchSupported, typeof(bool) },
            //        { KeyTouchCount, typeof(int) },
            //    };
            //    foreach (var btn in System.Enum.GetValues(typeof(InputDefines.MouseButton)).OfType<InputDefines.MouseButton>())
            //    {
            //        _keyAndTypeDict.Add(KeyMouseButton + ((int)btn).ToString(), typeof(InputDefines.ButtonCondition));
            //    }
            //    for (var i = 0; i < LIMIT_TOUCH_COUNT; ++i)
            //    {
            //        _keyAndTypeDict.Add(KeyTouch + i.ToString(), typeof(TouchUpdateObserver));
            //    }
            //    foreach(var keyCode in System.Enum.GetValues(typeof(KeyCode)).OfType<KeyCode>())
            //    {
            //        _keyAndTypeDict.Add($"{KeyButtonPrefix}{(int)keyCode}", typeof(int));
            //    }
            //}
            //return _keyAndTypeDict;
        }

        readonly Regex _keyButtonKeyRegex = new Regex(@"^k[0-9]+$");
        public FrameInputData(SerializationInfo info, StreamingContext context)
            : this()
        {
            var e = info.GetEnumerator();
            while (e.MoveNext())
            {
                Vector2 v;
                switch (e.Name)
                {
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
                        else if(_keyButtonKeyRegex.IsMatch(e.Name))
                        {
                            if (int.TryParse(e.Name.Substring(KeyButtonPrefix.Length), out var keyCodeValue))
                            {
                                SetKeyButton((KeyCode)keyCodeValue, (InputDefines.ButtonCondition)e.Value);
                            }
                        }
                        break;
                }

            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
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
            //Key Button
            foreach(var (keyCode, condition) in _keyButtonsDict
                .Select(_t => (keyCode: _t.Key, condition:_t.Value))
                .Where(_t => _t.condition.DidUpdated))
            {
                info.AddValue($"{KeyButtonPrefix}{(int)keyCode}", (int)condition.Value);
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
            FrameInputData _target;
            public UpdatedValueEnumerable(FrameInputData target)
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
                FrameInputData _target;
                IEnumerator<IUpdateObserver> _enmuerator;

                public Enumerator(FrameInputData target)
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
                    //Mouse
                    if (_target._mousePresent.DidUpdated) yield return _target._mousePresent;
                    if (_target._mousePosition.DidUpdated) yield return _target._mousePosition;
                    if (_target._mouseScrollDelta.DidUpdated) yield return _target._mouseScrollDelta;

                    foreach (var btn in _target._mouseButtons.Where(_b => _b.DidUpdated))
                    {
                        yield return btn;
                    }

                    //Touch
                    if (_target._touchSupported.DidUpdated) yield return _target._touchSupported;
                    if (_target._touchCount.DidUpdated) yield return _target._touchCount;

                    foreach (var t in _target.Touches.Where(_b => _b.DidUpdated))
                    {
                        yield return t;
                    }

                    //Key Buttons
                    foreach(var (key, condition) in _target.KeyButtons
                        .Select(_t => (keyCode : _t.Key, condition: _t.Value))
                        .Where(_t => _t.condition.DidUpdated))
                    {
                        yield return condition;
                    }
                }
            }
        }

        #endregion
    }
}

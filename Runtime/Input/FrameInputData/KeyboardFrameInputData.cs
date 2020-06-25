using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Hinode.Serialization;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// UnityEngine.EventSystemの1フレームにおける入力データを記録するためのもの
    /// 一つ前と変化がないデータはシリアライズの対象にならないようになっています。
    ///
    /// 以下のKeyCodeは他のKeyCodeと同じものと扱います。
    ///     KeyCode.LeftApple => KeyCode.LeftCommand
    ///     KeyCode.RightApple => KeyCode.RightCommand
    /// </summary>
    [System.Serializable, ContainsSerializationKeyTypeGetter(typeof(KeyboardFrameInputData))]
    public class KeyboardFrameInputData : InputRecorder.IFrameDataRecorder
        , ISerializable
        , FrameInputData.IChildFrameInputData
    {
        public static IEnumerable<KeyCode> AllKeyCodes { get; } = System.Enum.GetValues(typeof(KeyCode))
            .OfType<KeyCode>();

        Dictionary<KeyCode, UpdateObserver<InputDefines.ButtonCondition>> _keyButtonsDict = new Dictionary<KeyCode, UpdateObserver<InputDefines.ButtonCondition>>();

        HashSet<KeyCode> _enabledKeyCodeFilter = new HashSet<KeyCode>();

        JsonSerializer _jsonSerializer = new JsonSerializer();

        KeyCode TransformKeyCode(KeyCode keyCode)
        {
            switch(keyCode)
            {
                case KeyCode.LeftApple: return KeyCode.LeftCommand;
                case KeyCode.RightApple: return KeyCode.RightApple;
                default: return keyCode;
            }
        }

        public void SetKeyButton(KeyCode keyCode, InputDefines.ButtonCondition condition)
        {
            if (!EnabledKeyCodes.Contains(keyCode)) return;

            if (!_keyButtonsDict.ContainsKey(keyCode))
            {
                // InputDefines.ButtonCondition.Free must be InputDefines.ButtonCondition.Free for IUpdateObserver.DidUpdated!
                var initialCondition = InputDefines.ButtonCondition.Free;
                _keyButtonsDict.Add(keyCode, new UpdateObserver<InputDefines.ButtonCondition>(initialCondition));
            }
            _keyButtonsDict[keyCode].Value = condition;
        }

        public InputDefines.ButtonCondition GetKeyButton(KeyCode keyCode)
        {
            if (!EnabledKeyCodes.Contains(keyCode)) return InputDefines.ButtonCondition.Free;

            keyCode = TransformKeyCode(keyCode);
            return _keyButtonsDict.ContainsKey(keyCode)
                ? _keyButtonsDict[keyCode].Value
                : InputDefines.ButtonCondition.Free;
        }

        public IEnumerable<(KeyCode keyCode, UpdateObserver<InputDefines.ButtonCondition> observer)> KeyButtons
        {
            get => _keyButtonsDict
                .Where(_t => EnabledKeyCodes.Contains(_t.Key))
                .Select(_t => (keyCode: _t.Key, observer: _t.Value));
        }

        public KeyboardFrameInputData()
        {
        }

        #region Key Filter
        public void AddEnabledKeyCode(IEnumerable<KeyCode> enableKeyCodes)
        {
            foreach(var k in enableKeyCodes
                .Where(_k => !_enabledKeyCodeFilter.Contains(_k)))
            {
                _enabledKeyCodeFilter.Add(k);
            }
        }
        public void AddEnabledKeyCode(params KeyCode[] enableKeyCodes)
            => AddEnabledKeyCode(enableKeyCodes.AsEnumerable());

        public void RemoveEnabledKeyCode(IEnumerable<KeyCode> keyCodes)
        {
            foreach (var k in keyCodes
                .Where(_k => _enabledKeyCodeFilter.Contains(_k)))
            {
                _enabledKeyCodeFilter.Remove(k);
            }
        }
        public void RemoveEnabledKeyCode(params KeyCode[] enableKeyCodes)
            => RemoveEnabledKeyCode(enableKeyCodes.AsEnumerable());

        public IEnumerable<KeyCode> EnabledKeyCodes
        {
            get => _enabledKeyCodeFilter.Any()
                ? _enabledKeyCodeFilter
                : AllKeyCodes;
        }
        #endregion


        public void CopyUpdatedDatasTo(KeyboardFrameInputData other)
        {
            foreach (var (keyCode, condition) in KeyButtons
                .Where(_t => _t.observer.DidUpdated))
            {
                other.SetKeyButton(keyCode, condition.Value);
            }
        }

        #region FrameInputData.IChildFrameInputData interface
        public void ResetInputDatas()
        {
            foreach (var (key, observer) in KeyButtons)
            {
                observer.Reset();
            }
        }

        public void UpdateInputDatas(ReplayableInput input)
        {
            ResetInputDatas();

            if (input.AnyKey)
            {
                foreach (var keyCode in EnabledKeyCodes)
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
                foreach (var (keyCode, observer) in KeyButtons
                    .Where(_t => _t.observer.Value == InputDefines.ButtonCondition.Free))
                {
                    observer.Value = InputDefines.ButtonCondition.Free;
                }
            }
        }

        public IEnumerable<FrameInputData.KeyValue> GetValuesEnumerable()
        {
            return new ValuesEnumerable(this);
        }

        class ValuesEnumerable : IEnumerable<FrameInputData.KeyValue>
            , IEnumerable
        {
            KeyboardFrameInputData _target;
            public ValuesEnumerable(KeyboardFrameInputData target)
            {
                _target = target;
            }

            public IEnumerator<FrameInputData.KeyValue> GetEnumerator()
                => _target.KeyButtons
                .Select(_t => new FrameInputData.KeyValue(((int)_t.keyCode).ToString(), _t.observer))
                .GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }

        #endregion

        #region InputRecorder.IFrameDataRecorder interface
        /// <summary>
        /// <see cref="InputRecorder.IFrameDataRecorder.ResetDatas()"/>
        /// </summary>
        public void ResetDatas()
        {
            _keyButtonsDict.Clear();
        }

        /// <summary>
        /// <see cref="InputRecorder.IFrameDataRecorder.RecoverFrame(ReplayableInput, InputRecord.Frame)"/>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="frame"></param>
        public void RecoverFrame(ReplayableInput input, InputRecord.Frame frame)
        {
            var recoverInput = _jsonSerializer.Deserialize<KeyboardFrameInputData>(frame.InputText);

            recoverInput.CopyUpdatedDatasTo(this);

            foreach(var keyCode in AllKeyCodes)
            {
                input.SetRecordedKeyButton(keyCode, GetKeyButton(keyCode));
            }
        }

        /// <summary>
        /// <see cref="InputRecorder.IFrameDataRecorder.Update(ReplayableInput)"/>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public InputRecord.Frame Update(ReplayableInput input)
        {
            UpdateInputDatas(input);

            var frame = new InputRecord.Frame();
            frame.InputText = _jsonSerializer.Serialize(this);
            return frame;
        }
        #endregion

        #region ISerializable interface
        public KeyboardFrameInputData(SerializationInfo info, StreamingContext context)
        {
            var e = info.GetEnumerator();
            while (e.MoveNext())
            {
                if(int.TryParse(e.Name, out var keyCodeInt))
                {
                    var keyCode = (KeyCode)keyCodeInt;
                    SetKeyButton(keyCode, (InputDefines.ButtonCondition)e.Value);
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach(var (keyCode, observer) in KeyButtons
                .Where(_t => _t.observer.DidUpdated))
            {
                info.AddValue(((int)keyCode).ToString(), (int)observer.Value);
            }
        }
        #endregion

        [SerializationKeyTypeGetter]
        public static System.Type GetKeyType(string key)
        {
            if(int.TryParse(key, out var number))
            {
                return null != System.Enum.ToObject(typeof(KeyCode), number)
                    ? typeof(int)
                    : null;
            }
            return null;
        }

    }
}

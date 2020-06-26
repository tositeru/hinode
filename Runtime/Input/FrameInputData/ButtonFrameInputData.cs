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
    /// </summary>
    [System.Serializable, ContainsSerializationKeyTypeGetter(typeof(ButtonFrameInputData))]
    public class ButtonFrameInputData : InputRecorder.IFrameDataRecorder
        , ISerializable
        , FrameInputData.IChildFrameInputData
    {
        [SerializeField] Dictionary<string, UpdateObserver<InputDefines.ButtonCondition>> _buttons = new Dictionary<string, UpdateObserver<InputDefines.ButtonCondition>>();
        readonly HashSet<string> _observedButtonNames = new HashSet<string>();

        JsonSerializer _jsonSerializer = new JsonSerializer();

        public IReadOnlyCollection<string> ObservedButtonNames { get => _observedButtonNames; }

        public ButtonFrameInputData()
        {
        }

        public bool ContainsButton(string name)
            => _observedButtonNames.Contains(name);

        #region Observed Button Name
        public void AddObservedButtonNames(IEnumerable<string> buttonName)
        {
            foreach(var name in buttonName
                .Where(_n => !_observedButtonNames.Contains(_n)))
            {
                _observedButtonNames.Add(name);

                if (!_buttons.ContainsKey(name))
                {
                    var observer = new UpdateObserver<InputDefines.ButtonCondition>(InputDefines.ButtonCondition.Free);
                    _buttons.Add(name, observer);
                }
            }
        }
        public void AddObservedButtonNames(params string[] buttonName)
            => AddObservedButtonNames(buttonName.AsEnumerable());

        public void RemoveObservedButtonNames(IEnumerable<string> buttonName)
        {
            foreach (var name in buttonName
                .Where(_n => _observedButtonNames.Contains(_n)))
            {
                _observedButtonNames.Remove(name);

                if (_buttons.ContainsKey(name))
                {
                    _buttons[name].Value = InputDefines.ButtonCondition.Free;
                }
            }
        }
        public void RemoveObservedButtonNames(params string[] buttonName)
            => RemoveObservedButtonNames(buttonName.AsEnumerable());
        #endregion

        public InputDefines.ButtonCondition GetButton(string name)
            => _buttons.ContainsKey(name)
            ? _buttons[name].Value
            : InputDefines.ButtonCondition.Free;

        public void SetButton(string name, InputDefines.ButtonCondition condition)
        {
            if (!_buttons.ContainsKey(name)) return;

            _buttons[name].Value = condition;
        }

        public void CopyUpdatedDatasTo(ButtonFrameInputData other)
        {
            foreach(var (name, observer) in _buttons
                .Where(_b => _b.Value.DidUpdated)
                .Select(_b => (name: _b.Key, observer: _b.Value)))
            {
                other.SetButton(name, observer.Value);
            }
        }

        #region FrameInputData.IChildFrameInputData interface
        public void ResetInputDatas()
        {
            foreach (var observer in _buttons
                .Where(_b => _b.Value.DidUpdated)
                .Select(_b => _b.Value))
            {
                observer.Reset();
            }
        }

        public void UpdateInputDatas(ReplayableInput input)
        {
            ResetInputDatas();

            foreach(var name in _observedButtonNames)
            {
                var condition = input.GetButtonCondition(name);
                SetButton(name, condition);
            }
        }

        public IEnumerable<FrameInputData.KeyValue> GetValuesEnumerable()
        {
            return new ValuesEnumerable(this);
        }

        class ValuesEnumerable : IEnumerable<FrameInputData.KeyValue>
            , IEnumerable
        {
            ButtonFrameInputData _target;
            public ValuesEnumerable(ButtonFrameInputData target)
            {
                _target = target;
            }

            public IEnumerator<FrameInputData.KeyValue> GetEnumerator()
            {
                foreach(var name in _target._observedButtonNames)
                {
                    yield return (name, _target._buttons[name]);
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }

        #endregion

        #region InputRecorder.IFrameDataRecorder interface
        /// <summary>
        /// <see cref="InputRecorder.IFrameDataRecorder.ResetDatas()"/>
        /// </summary>
        public void ResetDatas()
        {
            foreach(var btn in _buttons)
            {
                btn.Value.SetDefaultValue(true);
            }
        }

        /// <summary>
        /// <see cref="InputRecorder.IFrameDataRecorder.RecoverFrame(ReplayableInput, InputRecord.Frame)"/>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="frame"></param>
        public void RecoverFrame(ReplayableInput input, InputRecord.Frame frame)
        {
            var recoverInput = _jsonSerializer.Deserialize<ButtonFrameInputData>(frame.InputText);

            recoverInput.CopyUpdatedDatasTo(this);

            foreach(var (name, observer) in _buttons.Select(_t => (_t.Key, _t.Value)))
            {
                input.SetRecordedButton(name, observer.Value);
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

        public ButtonFrameInputData(SerializationInfo info, StreamingContext context)
        {
            var e = info.GetEnumerator();
            while (e.MoveNext())
            {
                AddObservedButtonNames(e.Name);
                SetButton(e.Name, (InputDefines.ButtonCondition)e.Value);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach(var (name, observer) in _buttons.Select(_t => (_t.Key, _t.Value)))
            {
                info.AddValue(name, (int)observer.Value);
            }
        }
        #endregion

        [SerializationKeyTypeGetter]
        public static System.Type GetKeyType(string key)
        {
            return typeof(int);
        }

    }
}

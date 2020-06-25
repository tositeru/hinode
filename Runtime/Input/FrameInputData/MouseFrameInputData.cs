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
    [System.Serializable, ContainsSerializationKeyTypeGetter(typeof(MouseFrameInputData))]
    public class MouseFrameInputData : InputRecorder.IFrameDataRecorder
        , ISerializable
        , FrameInputData.IChildFrameInputData
    {
        [SerializeField] UpdateObserver<bool> _mousePresent = new UpdateObserver<bool>();
        [SerializeField]
        UpdateObserver<InputDefines.ButtonCondition>[] _mouseButtons = new UpdateObserver<InputDefines.ButtonCondition>[3] {
                new UpdateObserver<InputDefines.ButtonCondition>(InputDefines.ButtonCondition.Free),
                new UpdateObserver<InputDefines.ButtonCondition>(InputDefines.ButtonCondition.Free),
                new UpdateObserver<InputDefines.ButtonCondition>(InputDefines.ButtonCondition.Free),
            };
        [SerializeField] UpdateObserver<Vector3> _mousePosition = new UpdateObserver<Vector3>();
        [SerializeField] UpdateObserver<Vector2> _mouseScrollDelta = new UpdateObserver<Vector2>();

        JsonSerializer _jsonSerializer = new JsonSerializer();

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

        public MouseFrameInputData()
        {
        }

        public void CopyUpdatedDatasTo(MouseFrameInputData other)
        {
            if (_mousePresent.DidUpdated) other._mousePresent.Value = _mousePresent.Value;
            for (var i = 0; i < _mouseButtons.Length; ++i)
            {
                if (_mouseButtons[i].DidUpdated) other._mouseButtons[i].Value = _mouseButtons[i].Value;
            }
            if (_mousePosition.DidUpdated) other._mousePosition.Value = _mousePosition.Value;
            if (_mouseScrollDelta.DidUpdated) other._mouseScrollDelta.Value = _mouseScrollDelta.Value;

        }

        #region FrameInputData.IChildFrameInputData interface
        public void ResetInputDatas()
        {
            _mousePresent.Reset();
            _mousePosition.Reset();
            _mouseScrollDelta.Reset();
            foreach (var btn in _mouseButtons) { btn.Reset(); }
        }

        public void UpdateInputDatas(ReplayableInput input)
        {
            ResetInputDatas();

            MousePresent = input.MousePresent;
            MousePosition = input.MousePos;
            MouseScrollDelta = input.MouseScrollDelta;
            foreach (var btn in System.Enum.GetValues(typeof(InputDefines.MouseButton))
                .OfType<InputDefines.MouseButton>())
            {
                InputDefines.ButtonCondition condition = input.GetMouseButton(btn);
                SetMouseButton(btn, condition);
            }
        }

        public IEnumerable<FrameInputData.KeyValue> GetValuesEnumerable()
        {
            return new ValuesEnumerable(this);
        }

        class ValuesEnumerable : IEnumerable<FrameInputData.KeyValue>
            , IEnumerable
        {
            MouseFrameInputData _target;
            public ValuesEnumerable(MouseFrameInputData target)
            {
                _target = target;
            }

            public IEnumerator<FrameInputData.KeyValue> GetEnumerator()
            {
                yield return (KeyMousePresent, _target._mousePresent);
                yield return (KeyMousePosition,  _target._mousePosition);
                yield return (KeyMouseScrollDelta, _target._mouseScrollDelta);
                foreach(var btn in System.Enum.GetValues(typeof(InputDefines.MouseButton))
                    .OfType<InputDefines.MouseButton>())
                {
                    int btnNo = (int)btn;
                    yield return (btnNo.ToString(), _target._mouseButtons[btnNo]);
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
            _mousePresent.SetDefaultValue(true);
            _mousePosition.SetDefaultValue(true);
            _mouseScrollDelta.SetDefaultValue(true);
            foreach (var btn in _mouseButtons) { btn.SetDefaultValue(true); }
        }

        /// <summary>
        /// <see cref="InputRecorder.IFrameDataRecorder.RecoverFrame(ReplayableInput, InputRecord.Frame)"/>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="frame"></param>
        public void RecoverFrame(ReplayableInput input, InputRecord.Frame frame)
        {
            var recoverInput = _jsonSerializer.Deserialize<MouseFrameInputData>(frame.InputText);

            recoverInput.CopyUpdatedDatasTo(this);

            input.RecordedMousePresent = MousePresent;
            input.RecordedMousePos = MousePosition;
            input.RecordedMouseScrollDelta = MouseScrollDelta;
            foreach (var btn in System.Enum.GetValues(typeof(InputDefines.MouseButton)).OfType<InputDefines.MouseButton>())
            {
                input.SetRecordedMouseButton((int)btn, GetMouseButton(btn));
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
        //Mouse
        public static readonly string KeyMousePresent = "pre";
        public static readonly string KeyMousePosition = "pos";
        public static readonly string KeyMouseScrollDelta = "sclDel";

        public MouseFrameInputData(SerializationInfo info, StreamingContext context)
        {
            var e = info.GetEnumerator();
            while (e.MoveNext())
            {
                Vector2 v;
                if(e.Name == KeyMousePresent)
                {
                    _mousePresent.Value = (bool)e.Value;
                }
                else if(e.Name == KeyMousePosition)
                {
                    if (Vector2Extensions.TryParse((string)e.Value, out v))
                        _mousePosition.Value = v;
                }
                else if(e.Name == KeyMouseScrollDelta)
                {
                    if (Vector2Extensions.TryParse((string)e.Value, out v))
                        _mouseScrollDelta.Value = v;
                }
                else
                {
                    if (int.TryParse(e.Name, out var index))
                    {
                        SetMouseButton((InputDefines.MouseButton)index, (InputDefines.ButtonCondition)e.Value);
                    }
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (_mousePresent.DidUpdated) info.AddValue(KeyMousePresent, _mousePresent.Value);
            for (var i = 0; i < _mouseButtons.Length; ++i)
            {
                if (_mouseButtons[i].DidUpdated) info.AddValue(i.ToString(), _mouseButtons[i].Value);
            }
            if (_mousePosition.DidUpdated) info.AddValue(KeyMousePosition, _mousePosition.Value.ToString("F4"));
            if (_mouseScrollDelta.DidUpdated) info.AddValue(KeyMouseScrollDelta, _mouseScrollDelta.Value.ToString("F4"));
        }
        #endregion

        [SerializationKeyTypeGetter]
        public static System.Type GetKeyType(string key)
        {
            if(KeyMousePresent == key)
            {
                return typeof(bool);
            }
            else if(KeyMousePosition == key
                || KeyMouseScrollDelta == key)
            {
                return typeof(string);
            }
            else if(int.TryParse(key, out var _))
            {
                return typeof(InputDefines.ButtonCondition);
            }
            else
            {
                return null;
            }
        }

    }
}

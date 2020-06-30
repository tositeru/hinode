using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Hinode.Serialization;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// UnityEngine.EventSystemの1フレームにおける入力データを記録するためのもの
    /// 一つ前と変化がないデータはシリアライズの対象にならないようになっています。
    ///
    /// Input.GetAxisRawには対応していません。
    /// 
    /// <see cref="IFrameDataRecorder"/>
    /// <seealso cref="Hinode.Tests.Input.FrameInputDataRecorder.TestAxisButtonFrameInputData"/>
    /// </summary>
    [System.Serializable, ContainsSerializationKeyTypeGetter(typeof(AxisButtonFrameInputData))]
    public class AxisButtonFrameInputData : IFrameDataRecorder
        , ISerializable
    {
        public static readonly string KEY_CHILD_INPUT_DATA_TYPE = "axs";

        /// <summary>
        /// <seealso cref="FrameInputData.RegistChildFrameInputDataType(string, System.Type)"/>
        /// </summary>
        public static void RegistTypeToFrameInputData()
        {
            IFrameInputDateRecorderHelper.RegistTypeToFrameInputData(KEY_CHILD_INPUT_DATA_TYPE, typeof(AxisButtonFrameInputData));
        }

        [SerializeField] Dictionary<string, UpdateObserver<float>> _buttons = new Dictionary<string, UpdateObserver<float>>();
        readonly HashSet<string> _observedButtonNames = new HashSet<string>();

        JsonSerializer _jsonSerializer = new JsonSerializer();

        public IReadOnlyCollection<string> ObservedButtonNames { get => _observedButtonNames; }

        public AxisButtonFrameInputData()
        {
        }

        #region Observed Button Name
        public bool ContainsButton(string name)
            => _observedButtonNames.Contains(name);

        public void AddObservedButtonNames(IEnumerable<string> buttonName)
        {
            foreach (var name in buttonName
                .Where(_n => !_observedButtonNames.Contains(_n)))
            {
                _observedButtonNames.Add(name);

                if (!_buttons.ContainsKey(name))
                {
                    var observer = new UpdateObserver<float>(0f);
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
                    _buttons[name].Value = 0f;
                }
            }
        }
        public void RemoveObservedButtonNames(params string[] buttonName)
            => RemoveObservedButtonNames(buttonName.AsEnumerable());
        #endregion

        public float GetAxis(string name)
            => _buttons.ContainsKey(name)
            ? _buttons[name].Value
            : 0f;

        public void SetAxis(string name, float axis)
        {
            if (!_buttons.ContainsKey(name)) return;

            _buttons[name].Value = axis;
        }

        #region IFrameDataRecorder interface
        /// <summary>
        /// <see cref="IFrameDataRecorder.ResetDatas()"/>
        /// </summary>
        public void ResetDatas()
        {
            foreach (var btn in _buttons)
            {
                btn.Value.SetDefaultValue(true);
            }
        }

        public void RefleshUpdatedFlags()
        {
            foreach (var observer in _buttons
                .Where(_b => _b.Value.DidUpdated)
                .Select(_b => _b.Value))
            {
                observer.Reset();
            }
        }

        /// <summary>
        /// <see cref="IFrameDataRecorder.Record(ReplayableInput)"/>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public void Record(ReplayableInput input)
        {
            foreach (var name in _observedButtonNames)
            {
                var axis = input.GetAxis(name);
                SetAxis(name, axis);
            }
        }

        public void RecoverTo(ReplayableInput input)
        {
            foreach (var (name, observer) in _buttons.Select(_t => (_t.Key, _t.Value)))
            {
                input.SetRecordedAxis(name, observer.Value);
            }
        }

        public void CopyUpdatedDatasTo(IFrameDataRecorder other)
        {
            if (other is AxisButtonFrameInputData)
            {
                CopyUpdatedDatasTo(other as AxisButtonFrameInputData);
            }
            else
            {
                Assert.IsTrue(false, $"Not Support type({other.GetType()})...");
            }
        }

        public void CopyUpdatedDatasTo(AxisButtonFrameInputData other)
        {
            foreach (var (name, observer) in _buttons
                .Where(_b => _b.Value.DidUpdated)
                .Select(_b => (name: _b.Key, observer: _b.Value)))
            {
                other.SetAxis(name, observer.Value);
            }
        }

        public IEnumerable<FrameInputDataKeyValue> GetValuesEnumerable()
        {
            return new ValuesEnumerable(this);
        }

        class ValuesEnumerable : IEnumerable<FrameInputDataKeyValue>
            , IEnumerable
        {
            AxisButtonFrameInputData _target;
            public ValuesEnumerable(AxisButtonFrameInputData target)
            {
                _target = target;
            }

            public IEnumerator<FrameInputDataKeyValue> GetEnumerator()
            {
                foreach (var name in _target._observedButtonNames)
                {
                    yield return (name, _target._buttons[name]);
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }
        #endregion

        #region ISerializable interface

        public AxisButtonFrameInputData(SerializationInfo info, StreamingContext context)
        {
            var e = info.GetEnumerator();
            while (e.MoveNext())
            {
                AddObservedButtonNames(e.Name);
                SetAxis(e.Name, (float)e.Value);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (var (name, observer) in _buttons.Select(_t => (_t.Key, _t.Value)))
            {
                info.AddValue(name, observer.Value);
            }
        }
        #endregion

        [SerializationKeyTypeGetter]
        public static System.Type GetKeyType(string key)
        {
            return typeof(float);
        }

    }
}

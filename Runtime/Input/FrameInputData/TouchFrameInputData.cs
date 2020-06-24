﻿using System.Collections;
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
    /// <see cref="InputRecorder.IFrameDataRecorder"/>
    /// <seealso cref="FrameInputData.IChildFrameInputData"/>
    /// </summary>
    [System.Serializable, HasKeyAndTypeDictionaryGetter(typeof(TouchFrameInputData))]
    public class TouchFrameInputData : InputRecorder.IFrameDataRecorder
        , ISerializable
        , FrameInputData.IChildFrameInputData
    {
        public static readonly int LIMIT_TOUCH_COUNT = 16;

        [SerializeField] UpdateObserver<bool> _touchSupported = new UpdateObserver<bool>();
        [SerializeField] UpdateObserver<int> _touchCount = new UpdateObserver<int>();
        [SerializeField] TouchUpdateObserver[] _touches;
        [SerializeField] UpdateObserver<bool> _touchPressureSupported = new UpdateObserver<bool>();
        [SerializeField] UpdateObserver<bool> _multiTouchEnabled = new UpdateObserver<bool>();
        [SerializeField] UpdateObserver<bool> _simulateMouseWithTouches = new UpdateObserver<bool>();
        [SerializeField] UpdateObserver<bool> _stylusTouchSupported = new UpdateObserver<bool>();

        JsonSerializer _jsonSerializer = new JsonSerializer();

        public bool TouchSupported
        {
            get => _touchSupported.Value;
            set => _touchSupported.Value = value;
        }

        public bool TouchPressureSupported { get => _touchPressureSupported.Value; set => _touchPressureSupported.Value = value; }

        public bool MultiTouchEnabled { get => _multiTouchEnabled.Value; set => _multiTouchEnabled.Value = value; }

        public bool SimulateMouseWithTouches { get => _simulateMouseWithTouches.Value; set => _simulateMouseWithTouches.Value = value; }

        public bool StylusTouchSupported { get => _stylusTouchSupported.Value; set => _stylusTouchSupported.Value = value; }

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

        public TouchFrameInputData()
        {
        }

        public void CopyUpdatedDatasTo(TouchFrameInputData other)
        {
            if (_touchSupported.DidUpdated) other._touchSupported.Value = _touchSupported.Value;
            if (_touchCount.DidUpdated) other._touchCount.Value = _touchCount.Value;
            if (_multiTouchEnabled.DidUpdated) other._multiTouchEnabled.Value = _multiTouchEnabled.Value;
            if (_touchPressureSupported.DidUpdated) other._touchPressureSupported.Value = _touchPressureSupported.Value;
            if (_simulateMouseWithTouches.DidUpdated) other._simulateMouseWithTouches.Value = _simulateMouseWithTouches.Value;
            if (_stylusTouchSupported.DidUpdated) other._stylusTouchSupported.Value = _stylusTouchSupported.Value;
            for (var i = 0; i < _touchCount.Value; ++i)
            {
                if (Touches[i].DidUpdated) other.Touches[i] = Touches[i];
            }
        }

        #region FrameInputData.IChildFrameInputData interface
        public void ResetInputDatas()
        {
            _touchSupported.Reset();
            _multiTouchEnabled.Reset();
            _touchPressureSupported.Reset();
            _simulateMouseWithTouches.Reset();
            _stylusTouchSupported.Reset();
            _touchCount.Reset();
            foreach (var t in Touches) { t.Reset(); }
        }

        public void UpdateInputDatas(ReplayableInput input)
        {
            ResetInputDatas();

            TouchSupported = input.TouchSupported;
            TouchCount = input.TouchCount;
            TouchPressureSupported = input.TouchPressureSupported;
            MultiTouchEnabled = input.MultiTouchEnabled;
            StylusTouchSupported = input.StylusTouchSupported;
            SimulateMouseWithTouches = input.SimulateMouseWithTouches;
            for (var i = 0; i < input.TouchCount; ++i)
            {
                var t = input.GetTouch(i);
                SetTouch(i, t);
            }
        }

        public IEnumerable<FrameInputData.KeyValue> GetValuesEnumerable()
        {
            return new ValuesEnumerable(this);
        }

        class ValuesEnumerable : IEnumerable<FrameInputData.KeyValue>
            , IEnumerable
        {
            TouchFrameInputData _target;
            public ValuesEnumerable(TouchFrameInputData target)
            {
                _target = target;
            }

            public IEnumerator<FrameInputData.KeyValue> GetEnumerator()
            {
                yield return (KeyTouchSupported, _target._touchSupported);
                yield return (KeyTouchPressureSupported, _target._touchPressureSupported);
                yield return (KeyMultiTouchEnabled, _target._multiTouchEnabled);
                yield return (KeyStylusTouchSupported, _target._stylusTouchSupported);
                yield return (KeySimulateMouseWithTouches, _target._simulateMouseWithTouches);
                yield return (KeyTouchCount, _target._touchCount);

                for (int i = 0; i<_target.Touches.Length; ++i)
                {
                    yield return (i.ToString(), _target._touches[i]);
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
            _touchSupported.SetDefaultValue(true);
            _touchCount.SetDefaultValue(true);
            _multiTouchEnabled.SetDefaultValue(true);
            _stylusTouchSupported.SetDefaultValue(true);
            _touchPressureSupported.SetDefaultValue(true);
            _simulateMouseWithTouches.SetDefaultValue(true);
            foreach (var t in Touches) { t.SetDefaultValue(true); }
        }

        /// <summary>
        /// <see cref="InputRecorder.IFrameDataRecorder.RecoverFrame(ReplayableInput, InputRecord.Frame)"/>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="frame"></param>
        public void RecoverFrame(ReplayableInput input, InputRecord.Frame frame)
        {
            var recoverInput = _jsonSerializer.Deserialize<TouchFrameInputData>(frame.InputText);

            recoverInput.CopyUpdatedDatasTo(this);

            input.RecordedTouchSupported = TouchSupported;
            input.RecordedTouchPressureSupported = TouchPressureSupported;
            input.RecordedMultiTouchEnabled = MultiTouchEnabled;
            input.RecordedStylusTouchSupported = StylusTouchSupported;
            input.RecordedSimulateMouseWithTouches = SimulateMouseWithTouches;
            input.RecordedTouchCount = TouchCount;
            for (var i = 0; i < TouchCount; ++i)
            {
                input.SetRecordedTouch(i, (Touch)GetTouch(i));
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
        //Touch
        public static readonly string KeyTouchSupported = "spt";
        public static readonly string KeyTouchCount = "cnt";
        public static readonly string KeyTouchPressureSupported = "presSpt";
        public static readonly string KeyMultiTouchEnabled = "mlt";
        public static readonly string KeySimulateMouseWithTouches = "sml";
        public static readonly string KeyStylusTouchSupported = "stlSpt";

        public TouchFrameInputData(SerializationInfo info, StreamingContext context)
        {
            var e = info.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Name == KeyTouchSupported)
                {
                    _touchSupported.Value = (bool)e.Value;
                }
                else if(e.Name == KeyTouchCount)
                {
                    _touchCount.Value = (int)e.Value;
                }
                else if(e.Name == KeyTouchPressureSupported)
                {
                    _touchPressureSupported.Value = (bool)e.Value;
                }
                else if(e.Name == KeyMultiTouchEnabled)
                {
                    _multiTouchEnabled.Value = (bool)e.Value;
                }
                else if(e.Name == KeySimulateMouseWithTouches)
                {
                    _simulateMouseWithTouches.Value = (bool)e.Value;
                }
                else if(e.Name == KeyStylusTouchSupported)
                {
                    _stylusTouchSupported.Value = (bool)e.Value;
                }
                else
                {
                    if (int.TryParse(e.Name, out var index))
                    {
                        SetTouch(index, (TouchUpdateObserver)e.Value);
                    }
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (_touchSupported.DidUpdated) info.AddValue(KeyTouchSupported, _touchSupported.Value);
            if (_touchCount.DidUpdated) info.AddValue(KeyTouchCount, _touchCount.Value);
            if (_multiTouchEnabled.DidUpdated) info.AddValue(KeyMultiTouchEnabled, _multiTouchEnabled.Value);
            if (_touchPressureSupported.DidUpdated) info.AddValue(KeyTouchPressureSupported, _touchPressureSupported.Value);
            if (_stylusTouchSupported.DidUpdated) info.AddValue(KeyStylusTouchSupported, _stylusTouchSupported.Value);
            if (_simulateMouseWithTouches.DidUpdated) info.AddValue(KeySimulateMouseWithTouches, _simulateMouseWithTouches.Value);
            for (var i = 0; i < _touchCount.Value; ++i)
            {
                if (Touches[i].DidUpdated) info.AddValue(i.ToString(), Touches[i]);
            }
        }
        #endregion

        static Dictionary<string, System.Type> _keyAndTypeDict;
        [KeyAndTypeDictionaryGetter]
        public static IReadOnlyDictionary<string, System.Type> GetKeyAndTypeDictionary()
        {
            if (_keyAndTypeDict == null)
            {
                _keyAndTypeDict = new Dictionary<string, System.Type>
                {
                    { KeyTouchSupported, typeof(bool) },
                    { KeyTouchCount, typeof(int) },
                    { KeyTouchPressureSupported, typeof(bool) },
                    { KeyStylusTouchSupported, typeof(bool) },
                    { KeyMultiTouchEnabled, typeof(bool) },
                    { KeySimulateMouseWithTouches, typeof(bool) },
                };
                for (var i = 0; i < LIMIT_TOUCH_COUNT; ++i)
                {
                    _keyAndTypeDict.Add(i.ToString(), typeof(TouchUpdateObserver));
                }
            }
            return _keyAndTypeDict;
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// UnityEngine.Touchの値の更新を監視するクラス
    /// <seealso cref="Hinode.Tests.Input.TestTouchUpdateObserver"/>
    /// </summary>
    [System.Serializable]
    [HasKeyAndTypeDictionaryGetter(typeof(TouchUpdateObserver))]
    public class TouchUpdateObserver : ISerializable, IUpdateObserver
        , System.IEquatable<TouchUpdateObserver>
        , System.IEquatable<Touch>
    {
        public enum ValueKey
        {
            AltitudeAngle,
            AzimuthAngle,
            FingerId,
            MaximumPossiblePressure,
            Phase,
            Position,
            DeltaPosition,
            Pressure,
            Radius,
            RadiusVariance,
            RawPosition,
            TapCount,
            DeltaTime,
            Type,
        }

        private UpdateObserver<int> _fingerId = new UpdateObserver<int>();
        private UpdateObserver<Vector2> _position = new UpdateObserver<Vector2>();
        private UpdateObserver<Vector2> _rawPosition = new UpdateObserver<Vector2>();
        private UpdateObserver<Vector2> _positionDelta = new UpdateObserver<Vector2>();
        private UpdateObserver<float> _timeDelta = new UpdateObserver<float>();
        private UpdateObserver<int> _tapCount = new UpdateObserver<int>();
        private UpdateObserver<TouchPhase> _phase = new UpdateObserver<TouchPhase>();
        private UpdateObserver<TouchType> _type = new UpdateObserver<TouchType>();
        private UpdateObserver<float> _pressure = new UpdateObserver<float>();
        private UpdateObserver<float> _maximumPossiblePressure = new UpdateObserver<float>();
        private UpdateObserver<float> _radius = new UpdateObserver<float>();
        private UpdateObserver<float> _radiusVariance = new UpdateObserver<float>();
        private UpdateObserver<float> _altitudeAngle = new UpdateObserver<float>();
        private UpdateObserver<float> _azimuthAngle = new UpdateObserver<float>();

        public int FingerId
        {
            get => _fingerId.Value;
            set => _fingerId.Value = value;
        }
        public Vector2 Position
        {
            get => _position.Value;
            set => _position.Value = value;
        }
        public Vector2 RawPosition
        {
            get => _rawPosition.Value;
            set => _rawPosition.Value = value;
        }
        public Vector2 DeltaPosition
        {
            get => _positionDelta.Value;
            set => _positionDelta.Value = value;
        }
        public float DeltaTime
        {
            get => _timeDelta.Value;
            set => _timeDelta.Value = value;
        }
        public int TapCount
        {
            get => _tapCount.Value;
            set => _tapCount.Value = value;
        }
        public TouchPhase Phase
        {
            get => _phase.Value;
            set => _phase.Value = value;
        }
        public float Pressure
        {
            get => _pressure.Value;
            set => _pressure.Value = value;
        }
        public float MaximumPossiblePressure
        {
            get => _maximumPossiblePressure.Value;
            set => _maximumPossiblePressure.Value = value;
        }
        public TouchType Type
        {
            get => _type.Value;
            set => _type.Value = value;
        }
        public float AltitudeAngle
        {
            get => _altitudeAngle.Value;
            set => _altitudeAngle.Value = value;
        }
        public float AzimuthAngle
        {
            get => _azimuthAngle.Value;
            set => _azimuthAngle.Value = value;
        }
        public float Radius
        {
            get => _radius.Value;
            set => _radius.Value = value;
        }
        public float RadiusVariance
        {
            get => _radiusVariance.Value;
            set => _radiusVariance.Value = value;
        }

        #region IUpdateObserver interface
        /// <summary>
        /// 
        /// </summary>
        public bool DidUpdated
        {
            get
            {
                foreach(ValueKey key in System.Enum.GetValues(typeof(ValueKey)))
                {
                    if(DidUpdatedKey(key)) return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 自身を返すようにしています
        /// </summary>
        public object RawValue => this;

        public void SetDefaultValue(bool doResetDidUpdate)
        {
            _altitudeAngle.SetDefaultValue(doResetDidUpdate);
            _azimuthAngle.SetDefaultValue(doResetDidUpdate);
            _fingerId.SetDefaultValue(doResetDidUpdate);
            _maximumPossiblePressure.SetDefaultValue(doResetDidUpdate);
            _phase.SetDefaultValue(doResetDidUpdate);
            _position.SetDefaultValue(doResetDidUpdate);
            _positionDelta.SetDefaultValue(doResetDidUpdate);
            _pressure.SetDefaultValue(doResetDidUpdate);
            _radius.SetDefaultValue(doResetDidUpdate);
            _radiusVariance.SetDefaultValue(doResetDidUpdate);
            _rawPosition.SetDefaultValue(doResetDidUpdate);
            _tapCount.SetDefaultValue(doResetDidUpdate);
            _timeDelta.SetDefaultValue(doResetDidUpdate);
            _type.SetDefaultValue(doResetDidUpdate);
        }
        #endregion

        public TouchUpdateObserver()
        {

        }

        public static explicit operator Touch(TouchUpdateObserver observer)
        {
            return new Touch
            {
                altitudeAngle = observer.AltitudeAngle,
                azimuthAngle = observer.AzimuthAngle,
                fingerId = observer.FingerId,
                maximumPossiblePressure = observer.MaximumPossiblePressure,
                phase = observer.Phase,
                position = observer.Position,
                deltaPosition = observer.DeltaPosition,
                pressure = observer.Pressure,
                radius = observer.Radius,
                radiusVariance = observer.RadiusVariance,
                rawPosition = observer.RawPosition,
                tapCount = observer.TapCount,
                deltaTime = observer.DeltaTime,
                type = observer.Type,
            };
        }

        public void Reset()
        {
            _altitudeAngle.Reset();
            _azimuthAngle.Reset();
            _fingerId.Reset();
            _maximumPossiblePressure.Reset();
            _phase.Reset();
            _position.Reset();
            _positionDelta.Reset();
            _pressure.Reset();
            _radius.Reset();
            _radiusVariance.Reset();
            _rawPosition.Reset();
            _tapCount.Reset();
            _timeDelta.Reset();
            _type.Reset();
        }

        public void Update(Touch touch)
        {
            Reset();
            AltitudeAngle = touch.altitudeAngle;
            AzimuthAngle = touch.azimuthAngle;
            FingerId = touch.fingerId;
            MaximumPossiblePressure = touch.maximumPossiblePressure;
            Phase = touch.phase;
            Position = touch.position;
            DeltaPosition = touch.deltaPosition;
            Pressure = touch.pressure;
            Radius = touch.radius;
            RadiusVariance = touch.radiusVariance;
            RawPosition = touch.rawPosition;
            TapCount = touch.tapCount;
            DeltaTime = touch.deltaTime;
            Type = touch.type;
        }

        public void Update(TouchUpdateObserver other)
        {
            Reset();
            AltitudeAngle = other.AltitudeAngle;
            AzimuthAngle = other.AzimuthAngle;
            FingerId = other.FingerId;
            MaximumPossiblePressure = other.MaximumPossiblePressure;
            Phase = other.Phase;
            Position = other.Position;
            DeltaPosition = other.DeltaPosition;
            Pressure = other.Pressure;
            Radius = other.Radius;
            RadiusVariance = other.RadiusVariance;
            RawPosition = other.RawPosition;
            TapCount = other.TapCount;
            DeltaTime = other.DeltaTime;
            Type = other.Type;
        }

        public bool DidUpdatedKey(ValueKey key)
        {
            switch (key)
            {
                case ValueKey.AltitudeAngle: return _altitudeAngle.DidUpdated;
                case ValueKey.AzimuthAngle: return _azimuthAngle.DidUpdated;
                case ValueKey.FingerId: return _fingerId.DidUpdated;
                case ValueKey.MaximumPossiblePressure: return _maximumPossiblePressure.DidUpdated;
                case ValueKey.Phase: return _phase.DidUpdated;
                case ValueKey.Position: return _position.DidUpdated;
                case ValueKey.DeltaPosition: return _positionDelta.DidUpdated;
                case ValueKey.Pressure: return _pressure.DidUpdated;
                case ValueKey.Radius: return _radius.DidUpdated;
                case ValueKey.RadiusVariance: return _radiusVariance.DidUpdated;
                case ValueKey.RawPosition: return _rawPosition.DidUpdated;
                case ValueKey.TapCount: return _tapCount.DidUpdated;
                case ValueKey.DeltaTime: return _timeDelta.DidUpdated;
                case ValueKey.Type: return _type.DidUpdated;
                default:
                    throw new System.NotImplementedException();
            }
        }

        #region Object interface

        public override bool Equals(object obj)
        {
            var selfType = typeof(TouchUpdateObserver);
            if(obj is TouchUpdateObserver)
            {
                return Equals((TouchUpdateObserver)obj);
            }
            else if(obj is Touch)
            {
                return Equals((Touch)obj);
            }
            return false;
        }

        public bool Equals(TouchUpdateObserver other)
        {
            return FingerId == other.FingerId
                && Position == other.Position
                && RawPosition == other.RawPosition
                && DeltaPosition == other.DeltaPosition
                && DeltaTime == other.DeltaTime
                && TapCount == other.TapCount
                && Phase == other.Phase
                && Pressure == other.Pressure
                && MaximumPossiblePressure == other.MaximumPossiblePressure
                && Type == other.Type
                && AltitudeAngle == other.AltitudeAngle
                && AzimuthAngle == other.AzimuthAngle
                && Radius == other.Radius
                && RadiusVariance == other.RadiusVariance;
        }

        public bool Equals(Touch other)
        {
            return FingerId == other.fingerId
                && Position == other.position
                && RawPosition == other.rawPosition
                && DeltaPosition == other.deltaPosition
                && DeltaTime == other.deltaTime
                && TapCount == other.tapCount
                && Phase == other.phase
                && Pressure == other.pressure
                && MaximumPossiblePressure == other.maximumPossiblePressure
                && Type == other.type
                && AltitudeAngle == other.altitudeAngle
                && AzimuthAngle == other.azimuthAngle
                && Radius == other.radius
                && RadiusVariance == other.radiusVariance;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region ISerializer interface
        public TouchUpdateObserver(SerializationInfo info, StreamingContext context)
        {
            foreach(var (key, entry) in info.GetEnumerable()
                .Select(_e => {
                    var b = int.TryParse(_e.Name, out var key);
                    return (isValid: b, key: key, entry: _e);
                })
                .Where(_t => _t.isValid)
                .Select(_t => (key: (ValueKey)_t.key, entry: _t.entry)))
            {
                //Debug.Log($"debug TouchUpdateObserver -- key={key} value={entry.Value} type={entry.Value.GetType().FullName}");
                switch(key)
                {
                    case ValueKey.AltitudeAngle: AltitudeAngle = (float)entry.Value; break;
                    case ValueKey.AzimuthAngle: AzimuthAngle = (float)entry.Value; break;
                    case ValueKey.FingerId: FingerId = (int)entry.Value; break;
                    case ValueKey.MaximumPossiblePressure: MaximumPossiblePressure = (float)entry.Value; break;
                    case ValueKey.Phase: Phase = (TouchPhase)entry.Value; break;
                    case ValueKey.Position: Position = (Vector2)entry.Value; break;
                    case ValueKey.DeltaPosition: DeltaPosition = (Vector2)entry.Value; break;
                    case ValueKey.Pressure: Pressure = (float)entry.Value; break;
                    case ValueKey.Radius: Radius = (float)entry.Value; break;
                    case ValueKey.RadiusVariance: RadiusVariance = (float)entry.Value; break;
                    case ValueKey.RawPosition: RawPosition = (Vector2)entry.Value; break;
                    case ValueKey.TapCount: TapCount = (int)entry.Value; break;
                    case ValueKey.DeltaTime: DeltaTime = (float)entry.Value; break;
                    case ValueKey.Type: Type = (TouchType)entry.Value; break;
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (_altitudeAngle.DidUpdated) info.AddValue(((int)ValueKey.AltitudeAngle).ToString(), _altitudeAngle.Value);
            if (_azimuthAngle.DidUpdated) info.AddValue(((int)ValueKey.AzimuthAngle).ToString(), _azimuthAngle.Value);
            if (_fingerId.DidUpdated) info.AddValue(((int)ValueKey.FingerId).ToString(), _fingerId.Value);
            if (_maximumPossiblePressure.DidUpdated) info.AddValue(((int)ValueKey.MaximumPossiblePressure).ToString(), _maximumPossiblePressure.Value);
            if (_phase.DidUpdated) info.AddValue(((int)ValueKey.Phase).ToString(), _phase.Value);
            if (_position.DidUpdated) info.AddValue(((int)ValueKey.Position).ToString(), _position.Value);
            if (_positionDelta.DidUpdated) info.AddValue(((int)ValueKey.DeltaPosition).ToString(), _positionDelta.Value);
            if (_pressure.DidUpdated) info.AddValue(((int)ValueKey.Pressure).ToString(), _pressure.Value);
            if (_radius.DidUpdated) info.AddValue(((int)ValueKey.Radius).ToString(), _radius.Value);
            if (_radiusVariance.DidUpdated) info.AddValue(((int)ValueKey.RadiusVariance).ToString(), _radiusVariance.Value);
            if (_rawPosition.DidUpdated) info.AddValue(((int)ValueKey.RawPosition).ToString(), _rawPosition.Value);
            if (_tapCount.DidUpdated) info.AddValue(((int)ValueKey.TapCount).ToString(), _tapCount.Value);
            if (_timeDelta.DidUpdated) info.AddValue(((int)ValueKey.DeltaTime).ToString(), _timeDelta.Value);
            if (_type.DidUpdated) info.AddValue(((int)ValueKey.Type).ToString(), _type.Value);
        }

        static Dictionary<string, System.Type> _keyAndTypeDict;
        [KeyAndTypeDictionaryGetter]
        static IReadOnlyDictionary<string, System.Type> GetKeyAndTypeDictionary()
        {
            if(_keyAndTypeDict == null)
            {
                _keyAndTypeDict = new Dictionary<string, System.Type>
                {
                    { ((int)ValueKey.AltitudeAngle).ToString(), typeof(float) },
                    { ((int)ValueKey.AzimuthAngle).ToString(), typeof(float) },
                    { ((int)ValueKey.FingerId).ToString(), typeof(int) },
                    { ((int)ValueKey.MaximumPossiblePressure).ToString(), typeof(float) },
                    { ((int)ValueKey.Phase).ToString(), typeof(TouchPhase) },
                    { ((int)ValueKey.Position).ToString(), typeof(Vector2) },
                    { ((int)ValueKey.DeltaPosition).ToString(), typeof(Vector2) },
                    { ((int)ValueKey.Pressure).ToString(), typeof(float) },
                    { ((int)ValueKey.Radius).ToString(), typeof(float) },
                    { ((int)ValueKey.RadiusVariance).ToString(), typeof(float) },
                    { ((int)ValueKey.RawPosition).ToString(), typeof(Vector2) },
                    { ((int)ValueKey.TapCount).ToString(), typeof(int) },
                    { ((int)ValueKey.DeltaTime).ToString(), typeof(float) },
                    { ((int)ValueKey.Type).ToString(), typeof(TouchType) },
                };
            }
            return _keyAndTypeDict;
        }
        #endregion
    }

    public static partial class TouchExtensions
    {
        public static bool Equals(this Touch t, TouchUpdateObserver observer)
        {
            return observer.Equals(t);
        }
    }
}


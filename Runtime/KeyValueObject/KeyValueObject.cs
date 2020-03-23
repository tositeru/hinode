using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    [System.Serializable]
    public abstract class IKeyValueObject<TValue> : System.IEquatable<IKeyValueObject<TValue>>
    {
        [SerializeField] string _key = "";
        [SerializeField] TValue _value;

        public string Key { get => _key; }
        public TValue Value { get => _value; }

        protected IKeyValueObject(string key, TValue value)
        {
            var type = typeof(TValue);
            Assert.IsTrue(type.Equals(typeof(int))
                || type.Equals(typeof(float))
                || type.Equals(typeof(double))
                || type.Equals(typeof(bool))
                || type.Equals(typeof(string))
                || type.Equals(typeof(System.Enum))
                || type.Equals(typeof(Object)),
                $"Don't Support TValue Type('{type.FullName}')... ");

            _key = key;
            _value = value;
        }


        public bool Equals(IKeyValueObject<TValue> other)
        {
            return Key.Equals(other.Key) && Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is IKeyValueObject<TValue>) return Equals(obj as IKeyValueObject<TValue>);
            return false;
        }
        public override int GetHashCode()
        {
            return (Key, Value).GetHashCode();
        }

    }

    [System.Serializable]
    public class KeyIntObject : IKeyValueObject<int>
    {
        public KeyIntObject(string key, int value) : base(key, value) { }

        public static implicit operator (string, int)(KeyIntObject obj)
        {
            return (obj.Key, obj.Value);
        }
        public static implicit operator KeyIntObject((string, int) obj)
        {
            return new KeyIntObject(obj.Item1, obj.Item2);
        }
    }

    [System.Serializable]
    public class KeyBoolObject : IKeyValueObject<bool>
    {
        public KeyBoolObject(string key, bool value) : base(key, value) { }

        public static implicit operator (string, bool)(KeyBoolObject obj)
        {
            return (obj.Key, obj.Value);
        }
        public static implicit operator KeyBoolObject((string, bool) obj)
        {
            return new KeyBoolObject(obj.Item1, obj.Item2);
        }
    }

    /// <summary>
    /// 値にRangeAttributeを使用したい場合はRangeNumberAttributeを使用してください。
    /// <seealso cref="RangeNumberAttribute"/>
    /// </summary>
    [System.Serializable]
    public class KeyFloatObject : IKeyValueObject<float>
    {
        public KeyFloatObject(string key, float value) : base(key, value) { }

        public static implicit operator (string, float)(KeyFloatObject obj)
        {
            return (obj.Key, obj.Value);
        }
        public static implicit operator KeyFloatObject((string, float) obj)
        {
            return new KeyFloatObject(obj.Item1, obj.Item2);
        }
    }

    /// <summary>
    /// 値にRangeAttributeを使用したい場合はRangeNumberAttributeを使用してください。
    /// <seealso cref="RangeNumberAttribute"/>
    /// </summary>
    [System.Serializable]
    public class KeyDoubleObject : IKeyValueObject<double>
    {
        public KeyDoubleObject(string key, double value) : base(key, value) { }

        public static implicit operator (string, double)(KeyDoubleObject obj)
        {
            return (obj.Key, obj.Value);
        }
        public static implicit operator KeyDoubleObject((string, double) obj)
        {
            return new KeyDoubleObject(obj.Item1, obj.Item2);
        }
    }

    [System.Serializable]
    public class KeyStringObject : IKeyValueObject<string>
    {
        public KeyStringObject(string key, string value) : base(key, value) { }

        public static implicit operator (string, string)(KeyStringObject obj)
        {
            return (obj.Key, obj.Value);
        }
        public static implicit operator KeyStringObject((string, string) obj)
        {
            return new KeyStringObject(obj.Item1, obj.Item2);
        }
    }

    [System.Serializable]
    public abstract class IKeyValueObjectWithTypeName<T> : IKeyValueObject<T>, IHasTypeName
    {
        [SerializeField] string _typeName;
        System.Type _type;
        public System.Type HasType
        {
            get
            {
                if (_type != null) return _type;
                if (_typeName == "") return null;

                _type = GetTypeEnumerable()
                    .FirstOrDefault(_t => _t.FullName == _typeName);
                return _type;
            }
            set
            {
                Assert.IsNotNull(value);
                _type = value;
                _typeName = _type.FullName;
            }
        }

        /// <summary>
        /// 型を検索する時に検索範囲を指定するために使用する関数
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<System.Type> GetTypeEnumerable()
        {
            return System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(_asm => _asm.GetTypes());
        }

        protected IKeyValueObjectWithTypeName(string key, T value, System.Type type) : base(key, value)
        {
            Assert.IsNotNull(type);
            _typeName = type.FullName;
        }
    }

    /// <summary>
    /// System.Enum.GetNamesをベースに値を管理している
    /// </summary>
    [System.Serializable]
    public class KeyEnumObject : IKeyValueObjectWithTypeName<int>
    {
        new public System.Enum Value { get => GetValue(base.Value, HasType); }
        public int EnumIndex { get => base.Value; }
        public bool IsValid { get => IsValidValue(base.Value); }
        public bool IsFlags { get => IsFlagsEnum(HasType); }

        public static KeyEnumObject Create<T>(string key, T value)
            where T : System.Enum
        {
            return new KeyEnumObject(key, value, typeof(T));
        }

        public KeyEnumObject(string key, System.Enum value, System.Type type) : base(key, GetIndex(type, value), type)
        {
        }

        public KeyEnumObject(System.Type type) : base("", 0, type)
        {
        }

        private KeyEnumObject() : base("", 0, typeof(System.Enum))
        { }

        public bool IsValidValue(int value)
        {
            if (HasType == null) return false;
            if (!HasType.IsSubclassOf(typeof(System.Enum))) return false;

            if (IsFlags)
            {
                if (value == -1) return true;
                uint bits = System.Enum.GetValues(HasType).Cast<int>()
                    .Select(_v => (uint)_v)
                    .Aggregate((_s, _c) => _s | _c);
                uint valueBits = (uint)value;
                return (valueBits & (~bits)) <= 0;
            }
            else
            {
                return System.Enum.GetValues(HasType).Cast<int>().Any(_v => (int)_v == value);
            }
        }

        public int GetIndex(System.Enum value)
        {
            return GetIndex(HasType, value);
        }

        static System.Enum GetValue(int value, System.Type type)
        {
            if(IsFlagsEnum(type))
            {
                if (value == -1) return (System.Enum)System.Enum.Parse(type, (-1).ToString());

                var names = System.Enum.GetNames(type);
                var usedFlagNames = System.Enum.GetValues(type).Cast<int>()
                    .Zip(Enumerable.Range(0, System.Enum.GetValues(type).Length), (_v, _i) => (value: _v, i: _i))
                    .Where(_t => (_t.value & value) > 0)
                    .Select(_t => names[_t.i]);
                string enumName = usedFlagNames.Any()
                    ? usedFlagNames.Aggregate((_name, _c) =>
                        {
                            return _name
                                + (_name.Length > 0 ? ", " : "")
                                + _c;
                        })
                    : "0";
                return (System.Enum)System.Enum.Parse(type, enumName);
            }
            else
            {
                return (System.Enum)System.Enum.Parse(type, System.Enum.GetNames(type)[value]);
            }
        }

        protected override IEnumerable<System.Type> GetTypeEnumerable()
        {
            return GetEnumTypeEnumerable();
        }

        public static IEnumerable<System.Type> GetEnumTypeEnumerable()
        {
            return System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(_asm => _asm.GetTypes().Where(_t => _t.IsSubclassOf(typeof(System.Enum))));
        }

        static bool IsFlagsEnum(System.Type type) => type.GetCustomAttributes(false).Any(_a => _a is System.FlagsAttribute);

        static int GetIndex(System.Type type, System.Enum value)
        {
            var valueBits = (uint)(int)(object)value;
            if (valueBits == uint.MaxValue) return -1;

            var allValueIndex = System.Enum.GetValues(type).Cast<int>()
                .Select(_v => (uint)_v)
                .Aggregate((_bits, _c) => _bits |= _c);
            Assert.IsFalse(((~allValueIndex) & valueBits) > 0, $"value={valueBits:x}, allValueIndex={allValueIndex:x}");

            return (int)(allValueIndex & valueBits);
        }

        #region Cast operator
        public static implicit operator (string, System.Enum)(KeyEnumObject obj)
        {
            return (obj.Key, obj.Value);
        }
        public static implicit operator KeyEnumObject((string, System.Enum) obj)
        {
            return new KeyEnumObject(obj.Item1, obj.Item2, obj.Item2.GetType());
        }
        #endregion
    }

    [System.Serializable]
    public class KeyObjectRefObject : IKeyValueObjectWithTypeName<Object>
    {
        public static KeyObjectRefObject Create<T>(string key, Object value)
            where T : Object
        {
            return new KeyObjectRefObject(key, value, typeof(T));
        }

        protected KeyObjectRefObject() : this("", default, typeof(Object)) { }
        public KeyObjectRefObject(string key, Object value, System.Type type) : base(key, value, type)
        {
        }

        #region Cast operator
        public static implicit operator (string, Object)(KeyObjectRefObject obj)
        {
            return (obj.Key, obj.Value);
        }
        public static implicit operator KeyObjectRefObject((string, Object) obj)
        {
            return new KeyObjectRefObject(obj.Item1, obj.Item2, obj.Item2.GetType());
        }
        #endregion
    }
}

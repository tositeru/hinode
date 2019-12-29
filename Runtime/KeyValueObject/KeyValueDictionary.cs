using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// KeyValueObjectをDictionaryとしてまとめたクラス
    /// 各型ごとに専用の派生クラスを用意しています。
    /// <seealso cref="IKeyValueObject{TValue}"/>
    /// <seealso cref="KeyIntDictionary"/>
    /// <seealso cref="KeyFloatDictionary"/>
    /// <seealso cref="KeyDoubleDictionary"/>
    /// <seealso cref="KeyBoolDictionary"/>
    /// <seealso cref="KeyStringDictionary"/>
    /// <seealso cref="IKeyValueDictionaryWithTypeName{TKeyValue, T}"/>
    /// </summary>
    /// <typeparam name="TKeyValue">IKeyValueObjectの派生クラス</typeparam>
    /// <typeparam name="T">実際に内部で保持する値の型</typeparam>
    [System.Serializable]
    public abstract class IKeyValueDictionary<TKeyValue, T> : ScriptableObject, IEnumerable<TKeyValue>, IEnumerable
        where TKeyValue : IKeyValueObject<T>
    {
        [SerializeField] List<TKeyValue> _values = new List<TKeyValue>();
        Dictionary<string, TKeyValue> _dict = new Dictionary<string, TKeyValue>();

        protected IKeyValueDictionary()
        {
        }

        abstract protected TKeyValue CreateObj(string key, T value);

        public int Count { get => _values.Count; }
        public Dictionary<string, TKeyValue>.KeyCollection Keys { get => _dict.Keys; }
        public IEnumerable<T> Values { get => _dict.Values.Select(_v => _v.Value); }

        public void Add(string key, T value)
        {
            var obj = CreateObj(key, value);
            if(_dict.ContainsKey(key))
            {
                throw new System.ArgumentException($"Already have a same key in this... key={key}");
            }
            _values.Add(obj);
            _dict.Add(key, obj);
        }

        public bool Remove(string key)
        {
            if (!_dict.Remove(key))
                return false;

            var index = FindIndex(key);
            _values.RemoveAt(index);
            return true;
        }

        public TKeyValue Get(string key) => _dict[key];

        int FindIndex(string key)
        {
            return _values.FindIndex(_p => _p.Key == key);
        }

        public bool IsValid
        {
            get
            {
                if (_values.Count == _dict.Count) return true;
                return !_values.Any(_v => _values.Where(__v => __v.Key == _v.Key).Any());
            }
        }
        /// <summary>
        /// もし同じキーがあった時は異なるキーになるようにする
        /// 主にInspector上での編集時に使用することを想定しています。
        /// </summary>
        public virtual void Refresh()
        {
            var newDict = new Dictionary<string, TKeyValue>();
            for(var i=0; i<_values.Count; ++i)
            {
                var cur = _values[i];
                newDict.Add(cur.Key, cur);

                var sameNames = _values.Zip(Enumerable.Range(0, Count), (_e, _i) => (e: _e, index: _i))
                    .Where(pair => pair.e != cur)
                    .Where(pair => pair.e.Key == cur.Key).ToList();

                var suffixNumber = 1;
                foreach(var (e, index) in sameNames)
                {
                    var rename = $"{cur.Key}_{suffixNumber}";
                    while(_values.Any(_e => _e.Key == rename))
                    {
                        suffixNumber++;
                        rename = $"{cur.Key}_{suffixNumber}";
                    }
                    _values[index] = CreateObj(rename, e.Value);
                    suffixNumber++;
                }
            }
            _dict = newDict;
        }

        #region ScriptableObject
        void OnEnable()
        {
            Refresh();
        }
        #endregion

        #region IEnumerable
        public IEnumerator<TKeyValue> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }

    /// <summary>
    /// 型名付きのIKeyValueDictionary
    /// <seealso cref="KeyEnumDictionary"/>
    /// <seealso cref="KeyObjectRefDictionary"/>
    /// </summary>
    /// <typeparam name="TKeyValue"></typeparam>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public abstract class IKeyValueDictionaryWithTypeName<TKeyValue, T> : IKeyValueDictionary<TKeyValue, T>
        where TKeyValue : IKeyValueObject<T>
    {
        [SerializeField] string _typeName;
        System.Type _type;
        public System.Type CurrentType
        {
            get
            {
                if (_type != null) return _type;
                if (_typeName == "") return null;

                _type = FindType();
                return _type;
            }
            protected set
            {
                _type = value != null ? value : typeof(object);
                _typeName = _type.FullName;
            }
        }

        protected System.Type FindType() =>
            GetTypeEnumerable().FirstOrDefault(_t => _t.FullName == _typeName);

        protected abstract bool IsValidType(System.Type type);

        /// <summary>
        /// 型を検索する時に検索範囲を指定するために使用する関数
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<System.Type> GetTypeEnumerable()
        {
            return System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(_asm => _asm.GetTypes().Where(IsValidType));
        }

        public bool IsValidCurrentType { get => IsValidType(CurrentType); }
    }
}

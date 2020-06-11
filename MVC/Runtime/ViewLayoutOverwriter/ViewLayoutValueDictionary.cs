using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode.MVC
{
    public interface IReadOnlyViewLayoutValueDictionary : IEnumerable<KeyValuePair<string, object>>
    {
        int Count { get; }
        IReadOnlyDictionary<string, object> Layouts { get; }

        bool ContainsKey(string keyword);
        bool ContainsKey(System.Enum keyword);

        object GetValue(string keyword);
        object GetValue(System.Enum keyword);
    }

    /// <summary>
    /// <seealso cref="ViewLayoutValueDictionary"/>
    /// </summary>
    public class ViewLayoutValueDictionary : IReadOnlyViewLayoutValueDictionary
        , System.IDisposable
        , IEnumerable<KeyValuePair<string, object>>
    {
        Dictionary<string, object> _dict = new Dictionary<string, object>();

        public int Count { get => _dict.Count; }
        public IReadOnlyDictionary<string, object> Layouts { get => _dict; }

        public void Clear()
        {
            _dict.Clear();
        }

        public ViewLayoutValueDictionary AddValue(string keyword, object value)
        {
            if (_dict.ContainsKey(keyword))
            {
                throw new System.ArgumentException($"Already set ViewLayout keyword({keyword})...");
            }
            _dict.Add(keyword, value);
            return this;
        }
        public ViewLayoutValueDictionary AddValue(System.Enum keyword, object value)
            => AddValue(keyword.ToString(), value);

        public ViewLayoutValueDictionary RemoveValue(string keyword)
        {
            if (_dict.ContainsKey(keyword))
            {
                _dict.Remove(keyword);
            }
            return this;
        }
        public ViewLayoutValueDictionary RemoveValue(System.Enum keyword)
            => RemoveValue(keyword.ToString());

        public bool ContainsKey(string keyword)
            => _dict.ContainsKey(keyword);
        public bool ContainsKey(System.Enum keyword)
            => _dict.ContainsKey(keyword.ToString());

        public object GetValue(string keyword)
        {
            if (!_dict.ContainsKey(keyword))
            {
                throw new System.ArgumentException($"Not exist ViewLayout keyword({keyword})...");
            }
            return _dict[keyword];
        }
        public object GetValue(System.Enum keyword)
            => GetValue(keyword.ToString());

        #region System.IDisposable interface
        public void Dispose()
        {
            Clear();
        }
        #endregion

        #region IEnumerable<KeyValuePair<string, object>> interface
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            => _dict.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
        #endregion
    }

    public static partial class ViewLayoutValueDictionaryExtensions
    {
        public static ViewLayoutValueDictionary AddKeyAndValues(this ViewLayoutValueDictionary target, params (System.Enum key, object value)[] keyAndValues)
            => target.AddKeyAndValues(keyAndValues.AsEnumerable().Select(_t => (key: _t.key.ToString(), _t.value)));
        public static ViewLayoutValueDictionary AddKeyAndValues(this ViewLayoutValueDictionary target, IEnumerable<(System.Enum key, object value)> keyAndValues)
            => target.AddKeyAndValues(keyAndValues.Select(_t => (key: _t.key.ToString(), _t.value)));

        public static ViewLayoutValueDictionary AddKeyAndValues(this ViewLayoutValueDictionary target, params (string key, object value)[] keyAndValues)
            => target.AddKeyAndValues(keyAndValues.AsEnumerable());

        public static ViewLayoutValueDictionary AddKeyAndValues(this ViewLayoutValueDictionary target, IEnumerable<(string key, object value)> keyAndValues)
        {
            foreach (var (key, value) in keyAndValues)
            {
                target.AddValue(key, value);
            }
            return target;
        }
    }
}

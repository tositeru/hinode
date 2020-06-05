using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.MVC
{
    public interface IReadOnlyViewLayoutValueDictionary
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
    public class ViewLayoutValueDictionary : IReadOnlyViewLayoutValueDictionary, System.IDisposable
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
    }
}

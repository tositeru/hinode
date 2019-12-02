using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
    /// KeyによるTextリソースを管理するクラス
    ///
    /// <seealso cref="IHavingTextResource"/>
    /// </summary>
    public class TextResources : System.IDisposable
    {
        Dictionary<string, string> _textDict = new Dictionary<string, string>();

        public int Count { get => _textDict.Count; }

        public bool Contains(string key)
            => _textDict.ContainsKey(key);

        public TextResources Add(string key, string text)
        {
            Assert.IsFalse(_textDict.ContainsKey(key), $"Already exist Key({key})...");
            _textDict.Add(key, text);
            return this;
        }

        public string Get(string key)
        {
            Assert.IsTrue(_textDict.ContainsKey(key), $"Not exist already Key({key})...");
            return _textDict[key];
        }

        public string Get(string key, params object[] formatParams)
        {
            Assert.IsTrue(_textDict.ContainsKey(key), $"Not exist already Key({key})...");
            return string.Format(_textDict[key], formatParams);
        }

        #region IDisposable interface
        public void Dispose()
        {
            _textDict.Clear();
        }
        #endregion
    }
}
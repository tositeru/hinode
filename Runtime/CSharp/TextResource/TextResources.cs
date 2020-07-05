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
    /// <seealso cref="Hinode.Tests.CSharp.TextResource.TestTextResources"/>
    /// </summary>
    public class TextResources : System.IDisposable
    {
        Dictionary<string, string> _textDict = new Dictionary<string, string>();

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.TextResource.TestTextResources.BasicUsagePasses()"/>
        /// </summary>
        public int Count { get => _textDict.Count; }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.TextResource.TestTextResources.BasicUsagePasses()"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(string key)
            => _textDict.ContainsKey(key);

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.TextResource.TestTextResources.BasicUsagePasses()"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public TextResources Add(string key, string text)
        {
            Assert.IsFalse(_textDict.ContainsKey(key), $"Already exist Key({key})...");
            _textDict.Add(key, text);
            return this;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.TextResource.TestTextResources.BasicUsagePasses()"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            Assert.IsTrue(_textDict.ContainsKey(key), $"Not exist already Key({key})...");
            return _textDict[key];
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.TextResource.TestTextResources.BasicUsagePasses()"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="formatParams"></param>
        /// <returns></returns>
        public string Get(string key, params object[] formatParams)
        {
            Assert.IsTrue(_textDict.ContainsKey(key), $"Not exist already Key({key})...");
            return string.Format(_textDict[key], formatParams);
        }

        #region IDisposable interface
        /// <summary>
        /// <seealso cref="Hinode.Tests.CSharp.TextResource.TestTextResources.BasicUsagePasses()"/>
        /// </summary>
        public void Dispose()
        {
            _textDict.Clear();
        }
        #endregion
    }
}
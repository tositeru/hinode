using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp.Extensions
{
    /// <summary>
    /// <seealso cref="DictionaryExtensions"/>
    /// </summary>
    public class TestDictionaryExtensions
    {
        /// <summary>
        /// <seealso cref="DictionaryExtensions.Merge{TKey, TValue}(Dictionary{TKey, TValue}, bool, IEnumerable{KeyValuePair{TKey, TValue}}[])"/>
        /// <seealso cref="DictionaryExtensions.Merge{TKey, TValue}(Dictionary{TKey, TValue}, bool, IEnumerable{KeyValuePair{TKey, TValue}}[])"/>
        /// </summary>
        [Test]
        public void NotOverwriteMergePasses()
        {
            var dict = new Dictionary<string, int>()
            {
                { "Apple", 1 },
                { "Orange", 2 },
            };

            var src = new Dictionary<string, int>()
            {
                { "Grape", 3 },
                { "Apple", 111 }
            };
            var src2 = new Dictionary<string, int>()
            {
                { "Banana", 4 },
                { "Grape", 333 },
                { "Orange", 222 }
            };
            dict.Merge(false, src, src2);
            AssertionUtils.AreEqual(new Dictionary<string, int>() {
                    { "Apple", 1 },
                    { "Orange", 2 },
                    { "Grape", 3 },
                    { "Banana", 4 },
                }
                , dict, "Failed Merge by no overwrite mode...");
        }

        /// <summary>
        /// <seealso cref="DictionaryExtensions.Merge{TKey, TValue}(Dictionary{TKey, TValue}, bool, IEnumerable{KeyValuePair{TKey, TValue}}[])"/>
        /// <seealso cref="DictionaryExtensions.Merge{TKey, TValue}(Dictionary{TKey, TValue}, bool, IEnumerable{KeyValuePair{TKey, TValue}}[])"/>
        /// </summary>
        [Test]
        public void OverwriteMergePasses()
        {
            var dict = new Dictionary<string, int>()
            {
                { "Apple", 1 },
                { "Orange", 2 },
            };

            var src = new Dictionary<string, int>()
            {
                { "Grape", 3 },
                { "Apple", 111 }
            };
            var src2 = new Dictionary<string, int>()
            {
                { "Banana", 4 },
                { "Grape", 333 },
                { "Orange", 222 }
            };
            dict.Merge(true, src, src2);
            AssertionUtils.AreEqual(new Dictionary<string, int>() {
                    { "Apple", 111 },
                    { "Orange", 222 },
                    { "Grape", 333 },
                    { "Banana", 4 },
                }
                , dict, "Failed Merge by Overwrite mode...");
        }
    }
}

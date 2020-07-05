using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="Dictionary{TKey, TValue}"/>
    /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestDictionaryExtensions"/>
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// 他のDictinaryの要素を追加する
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestDictionaryExtensions.NotOverwriteMergePasses()"/>
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestDictionaryExtensions.OverwriteMergePasses()"/>
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="t"></param>
        /// <param name="isOverwrite"></param>
        /// <param name="srcDicts"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this Dictionary<TKey, TValue> t, bool isOverwrite, params IEnumerable<KeyValuePair<TKey, TValue>>[] srcDicts)
        {
            return t.Merge(isOverwrite, srcDicts.AsEnumerable());
        }

        /// <summary>
        /// 他のDictinaryの要素を追加する
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestDictionaryExtensions.NotOverwriteMergePasses()"/>
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestDictionaryExtensions.OverwriteMergePasses()"/>
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="t"></param>
        /// <param name="isOverwrite"></param>
        /// <param name="srcDicts"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this Dictionary<TKey, TValue> t, bool isOverwrite, IEnumerable<IEnumerable<KeyValuePair<TKey, TValue>>> srcDicts)
        {
            foreach (var keyValue in srcDicts.SelectMany(_d => _d.AsEnumerable().AsEnumerable()))
            {
                if (t.ContainsKey(keyValue.Key))
                {
                    if (isOverwrite) t[keyValue.Key] = keyValue.Value;
                }
                else
                {
                    t.Add(keyValue.Key, keyValue.Value);
                }
            }
            return t;
        }

    }
}

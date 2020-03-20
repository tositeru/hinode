using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// 他のDictinaryの要素を追加する
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

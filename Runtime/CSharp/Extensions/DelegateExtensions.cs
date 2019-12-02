using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="System.Delegate"/>
    /// </summary>
    public static class DelegateExtensions
    {
        /// <summary>
        /// delegateの仕様上、targetのInvocationListは空になりませんので、戻り値を使用してください。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static System.Delegate ClearInvocations(this System.Delegate target)
        {
            if (target == null) return null;
            var list = target.GetInvocationList();
            for (var i = 0; i < list.Length; ++i)
            {
                target = System.Delegate.Remove(target, list[i]);
            }
            return target;
        }

        /// <summary>
        /// delegateの仕様上、targetのInvocationListは空になりませんので、戻り値を使用してください。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static T ClearInvocations<T>(this T target)
            where T : System.Delegate
            => ClearInvocations(target as System.Delegate) as T;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="System.Delegate"/>
    /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestDelegateExtensions"/>
    /// </summary>
    public static class DelegateExtensions
    {
        /// <summary>
        /// delegateの仕様上、targetのInvocationListは空になりませんので、戻り値を使用してください。
        /// <seealso cref="Hinode.Tests.CSharp.Extensions.TestDelegateExtensions.ClearInvocationsPasses()"/>
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

        /// <summary>
        /// System.Delegate#Targetから値を取り出します。
        /// </summary>
        /// <param name="t"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool GetFieldValue(this System.Delegate t, string fieldName, out object value)
        {
            value = null;
            var type = t.Target.GetType();
            var info = type.GetField(fieldName);
            if (info == null) return false;
            value = info.GetValue(t.Target);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="checkValues"></param>
        /// <returns></returns>
        public static bool DoMatchFieldValues(this System.Delegate t, params (string fieldName, object value)[] checkValues)
        {
            return checkValues.All(_t => {
                if(!t.GetFieldValue(_t.fieldName, out var v)) return false;
                if (v == null)
                    return _t.value == null;
                else
                    return v.Equals(_t.value);
            });
        }
    }
}

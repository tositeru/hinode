using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// <seealso cref=""/>
    /// </summary>
    public static class IEnumerableExtensions
    {
        public static bool IsEmptyOrNull<T>(this IEnumerable<T> t)
        {
            return t == null || !t.Any();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="Hinode.Tests.Extensions.TestVector2Extensions"/>
    /// </summary>
    public static class Vector2Extensions
    {
        /// <summary>
        /// ([num], [num])の形式のみ対応している
        /// <seealso cref="Hinode.Tests.Extensions.TestVector2Extensions.TryParsePasses()"/>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParse(string text, out Vector2 result)
        {
            result = Vector2.zero;
            var t = text.Replace("(", "").Replace(")", "");
            var nums = t.Split(',');
            if (!float.TryParse(nums[0], out result.x)) return false;
            if (!float.TryParse(nums[1], out result.y)) return false;
            return true;
        }
    }
}

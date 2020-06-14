using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="Hinode.Tests.Extensions.TestRangeAttributeExtensions"/>
    /// </summary>
    public static partial class RangeAttributeExtensions
    {
        /// <summary>
        /// <seealso cref="Hinode.Tests.Extensions.TestRangeAttributeExtensions.IsInRangeByFloatPasses()"/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsInRange(this RangeAttribute target, float value)
		{
            return target.min <= value && value <= target.max;
		}
        /// <summary>
        /// <seealso cref="Hinode.Tests.Extensions.TestRangeAttributeExtensions.IsInRangeByDoublePasses()"/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsInRange(this RangeAttribute target, double value)
        {
            return target.min <= value && value <= target.max;
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Extensions.TestRangeAttributeExtensions.ClampByFloatPasses()"/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float Clamp(this RangeAttribute target, float value)
        {
            return Mathf.Clamp(value, target.min, target.max);
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Extensions.TestRangeAttributeExtensions.ClampByDoublePasses()"/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double Clamp(this RangeAttribute target, double value)
        {
            if (value < target.min) value = target.min;
            if (value > target.max) value = target.max;
            return value;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    public static partial class RangeAttributeExtensions
    {
        public static bool IsInRange(this RangeAttribute target, float value)
		{
            return target.min <= value && value <= target.max;
		}
        public static bool IsInRange(this RangeAttribute target, double value)
        {
            return target.min <= value && value <= target.max;
        }

        public static float Clamp(this RangeAttribute target, float value)
        {
            return Mathf.Clamp(value, target.min, target.max);
        }
        public static double Clamp(this RangeAttribute target, double value)
        {
            if (value < target.min) value = target.min;
            if (value > target.max) value = target.max;
            return value;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// KeyIntObjectに対応したRangeAttribute
    /// <seealso cref="KeyIntObject"/>
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class RangeIntAttribute : PropertyAttribute
    {
		readonly int _min;
		readonly int _max;
	    public int Min { get => _min; }
        public int Max { get => _max; }

        public bool IsInRange(int v)
		{
			return Min <= v && v <= Max;
        }

		public int Clamp(int v)
		{
			return Mathf.Clamp(v, Min, Max);
		}

		public RangeIntAttribute(int min, int max, int order=0)
        {
			_min = Mathf.Min(min, max);
			_max = Mathf.Max(min, max);
            this.order = order;
        }
    }

    /// <summary>
    /// KeyFloatObjectとKeyDoubleObjectに対応したRangeAttribute
    /// 標準のRangeAttributeを拡張できなかったので作成したクラス
    /// <seealso cref="KeyFloatObject"/>
    /// <seealso cref="KeyDoubleObject"/>
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class RangeNumberAttribute : PropertyAttribute
	{
        readonly float _min;
        readonly float _max;

        public float Min { get => _min; }
        public float Max { get => _max; }

		public bool IsInRange(float v)
		{
			return Min <= v && v <= Max;
		}
        public float Clamp(float v)
		{
			return Mathf.Clamp(v, Min, Max);
		}

		public RangeNumberAttribute(float min, float max, int order=0)
        {
            _min = Mathf.Min(min, max);
            _max = Mathf.Max(min, max);
			this.order = order;
        }
    }
}

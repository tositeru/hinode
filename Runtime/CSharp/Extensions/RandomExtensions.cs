using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    public static partial class RandomExtensions
    {
        /// <summary>
		/// 
		/// </summary>
		/// <param name="rnd"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
        public static double Range(this System.Random rnd, double min, double max)
        {
            var t = System.Math.Abs(rnd.NextDouble()) / double.MaxValue;
            var len = System.Math.Max(min, max) - System.Math.Min(min, max);
            return min + t * len;
        }

        /// <summary>
		/// 
		/// </summary>
		/// <param name="rnd"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
        public static int Range(this System.Random rnd, int min, int max)
        {
            var len = System.Math.Max(min, max) - System.Math.Min(min, max);
            return min + rnd.Next(len);
        }

        /// <summary>
		/// 
		/// </summary>
		/// <param name="rnd"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
        public static float Range(this System.Random rnd, float min, float max)
            => (float)rnd.Range((double)min, (double)max);

    }
}

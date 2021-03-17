using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            var t = System.Math.Abs(rnd.Next(int.MaxValue)) / (double)int.MaxValue;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rnd"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(this System.Random rnd, int length)
        {
            return Enumerable.Range(0, length)
                .Select(_ =>
                {
                    char ch;
                    do
                    {
                        ch = (char)rnd.Range(1, byte.MaxValue);
                    } while ((char.IsControl(ch) || (ch & 0x80) != 0));
                    return ch;
                })
                .Aggregate("", (_s, _c) => _s + _c);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rnd"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        public static object Get(this System.Random rnd, System.Array array)
        {
            return array.GetValue(rnd.Next() % array.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rnd"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        public static T Get<T>(this System.Random rnd, IList<T> array)
        {
            return array[rnd.Next() % array.Count];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rnd"></param>
        /// <returns></returns>
        public static T Get<T>(this System.Random rnd)
            where T : System.Enum
        {
            var values = System.Enum.GetValues(typeof(T));
            return (T)values.GetValue(rnd.Next() % values.Length);
        }

    }
}

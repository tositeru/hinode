using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Math;

namespace Hinode
{
    public static class MathUtils
    {
        /// <summary>
		/// UnityのTest Frameworksから借用
		/// </summary>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		/// <param name="epsilon"></param>
		/// <returns></returns>
        public static bool AreNearlyEqual(float expected, float actual, float epsilon=float.Epsilon)
        {
            // special case for infinity
            if (expected == float.PositiveInfinity || actual == float.PositiveInfinity || expected == float.NegativeInfinity || actual == float.NegativeInfinity)
                return expected == actual;

            // we cover both relative and absolute tolerance with this check
            // which is better than just relative in case of small (in abs value) args
            // please note that "usually" approximation is used [i.e. abs(x)+abs(y)+1]
            // but we speak about test code so we dont care that much about performance
            // but we do care about checks being more precise
            return Abs(actual - expected) <= epsilon * Max(Max(Abs(actual), Abs(expected)), 1.0f);
        }

        /// <summary>
        /// UnityのTest Frameworksから借用
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public static bool AreNearlyEqual(double expected, double actual, double epsilon=double.Epsilon)
        {
            // special case for infinity
            if (expected == double.PositiveInfinity || actual == double.PositiveInfinity || expected == double.NegativeInfinity || actual == double.NegativeInfinity)
                return expected == actual;

            // we cover both relative and absolute tolerance with this check
            // which is better than just relative in case of small (in abs value) args
            // please note that "usually" approximation is used [i.e. abs(x)+abs(y)+1]
            // but we speak about test code so we dont care that much about performance
            // but we do care about checks being more precise
            return Abs(actual - expected) <= epsilon * Max(Max(Abs(actual), Abs(expected)), 1.0f);
        }

    }
}

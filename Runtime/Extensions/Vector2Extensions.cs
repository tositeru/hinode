﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="Hinode.Tests.Extensions.TestVector2Extensions"/>
    /// </summary>
    public static class Vector2Extensions
    {
        /// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="other"></param>
		/// <returns></returns>
        public static Vector2 Mul(this Vector2 self, Vector2 other)
            => new Vector2(
                self.x * other.x,
                self.y * other.y
            );

        /// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
        public static Vector2 Abs(this Vector2 self)
        {
            return new Vector2(
                System.Math.Abs(self.x),
                System.Math.Abs(self.y)
            );
        }

        /// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="other"></param>
		/// <param name="epsilon"></param>
		/// <returns></returns>
        public static bool AreNearlyEqual(this Vector2 self, Vector2 other, float epsilon = float.Epsilon)
        {
            return System.Math.Abs(self.x - other.x) <= epsilon
                && System.Math.Abs(self.y - other.y) <= epsilon;
        }

        static readonly Regex REMOVE_CHAR_REGEX = new Regex(@"[\(\)\[\]]");

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
            text = REMOVE_CHAR_REGEX.Replace(text, "");
            var sections = text.Split(',', ' ').Where(_s => _s.Length > 0).ToArray();
            var loopCount = System.Math.Min(2, sections.Length);
            for (var i = 0; i < loopCount; ++i)
            {
                bool isSuccess = false;
                switch (i)
                {
                    case 0: isSuccess = float.TryParse(sections[i], out result.x); break;
                    case 1: isSuccess = float.TryParse(sections[i], out result.y); break;
                }
                if (!isSuccess) return false;
            }
            return true;
        }
    }
}

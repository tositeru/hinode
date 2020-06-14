using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="Hinode.Tests.Extensions.TestRectExtensions"/>
    /// </summary>
    public static partial class RectExtensions
    {
        /// <summary>
        /// <seealso cref="Hinode.Tests.Extensions.TestRectExtensions.OverlapsPointPasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool Overlaps(this Rect t, Vector2 point)
            => t.xMin <= point.x
            && t.xMax >= point.x
            && t.yMin <= point.y
            && t.yMax >= point.y;

        /// <summary>
        /// <seealso cref="Hinode.Tests.Extensions.TestRectExtensions.OverlapsRectPasses()"/>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool Overlaps(this Rect t, Rect other)
            => !(t.xMin > other.xMax
            || t.xMax < other.xMin
            || t.yMin > other.yMax
            || t.yMax < other.yMin);

    }
}

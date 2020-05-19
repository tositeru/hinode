using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    public static partial class RectExtensions
    {
        public static bool Overlaps(this Rect t, Vector2 point)
            => t.xMin <= point.x
            && t.xMax >= point.x
            && t.yMin <= point.y
            && t.yMax >= point.y;
    }
}

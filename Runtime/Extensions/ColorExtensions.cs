using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    public static partial class ColorExtensions
    {
        public static Color HSVToRGBA(float H, float S, float V, float a, bool hdr=false)
        {
            var c = Color.HSVToRGB(H, S, V, hdr);
            c.a = a;
            return c;
        }
    }
}


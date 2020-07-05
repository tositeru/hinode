using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// <seealso cref="Hinode.Tests.Extensions.TestColorExtensions"/>
    /// </summary>
    public static partial class ColorExtensions
    {
        /// <summary>
        /// <seealso cref="Hinode.Tests.Extensions.TestColorExtensions.HSVToRGBAPasses()"/>
        /// </summary>
        /// <param name="H"></param>
        /// <param name="S"></param>
        /// <param name="V"></param>
        /// <param name="a"></param>
        /// <param name="hdr"></param>
        /// <returns></returns>
        public static Color HSVToRGBA(float H, float S, float V, float a, bool hdr=false)
        {
            var c = Color.HSVToRGB(H, S, V, hdr);
            c.a = a;
            return c;
        }
    }
}


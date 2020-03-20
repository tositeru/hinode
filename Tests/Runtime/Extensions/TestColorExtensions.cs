using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Runtime.Extensions
{
    /// <summary>
    /// <seealso cref="ColorExtensions"/>
    /// </summary>
    public class TestColorExtensions
    {
        [Test]
        public void HSVToRGBAPasses()
        {
            float srcH=0f, srcS=0.5f, srcV=0.2f;
            float srcAlpha = 0.4f;
            var color = ColorExtensions.HSVToRGBA(srcH, srcS, srcV, srcAlpha);
            //再変換できているかで正しいかどうか判定しています。
            // srcHの値が1の時は変換後に0へなるので注意 (0と1は同じ意味合いの値になります)
            Color.RGBToHSV(color, out var H, out var S, out var V);
            Assert.AreEqual(srcH, H);
            Assert.AreEqual(srcS, S);
            Assert.AreEqual(srcV, V);
            Assert.AreEqual(srcAlpha, color.a);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Extensions
{
    /// <summary>
    /// <seealso cref="RectExtensions"/>
    /// </summary>
    public class TestRectExtensions
    {
        /// <summary>
        /// <seealso cref="RectExtensions.Overlaps(Rect, Vector2)"/>
        /// </summary>
        [Test]
        public void OverlapsPointPasses()
        {
            var rect = new Rect(0, 0, 100, 100);

            Assert.IsTrue(rect.Overlaps(new Vector2(50, 50)));
            Assert.IsTrue(rect.Overlaps(new Vector2(0, 0)));
            Assert.IsTrue(rect.Overlaps(rect.size));
            Assert.IsFalse(rect.Overlaps(Vector2.one * -100f));
        }

        /// <summary>
        /// <seealso cref="RectExtensions.Overlaps(Rect, Rect)"/>
        /// </summary>
        [Test]
        public void OverlapsRectPasses()
        {
            var rect = new Rect(0, 0, 100, 100);

            Assert.IsTrue(rect.Overlaps(Rect.MinMaxRect(-10, -10, 10, 10)));
            Assert.IsTrue(rect.Overlaps(Rect.MinMaxRect(-10, 40, 10, 60)));
            Assert.IsTrue(rect.Overlaps(Rect.MinMaxRect(40, -10, 60, 10)));

            Assert.IsTrue(rect.Overlaps(Rect.MinMaxRect(90, 90, 110, 110)));
            Assert.IsTrue(rect.Overlaps(Rect.MinMaxRect(90, 40, 110, 60)));
            Assert.IsTrue(rect.Overlaps(Rect.MinMaxRect(40, 90, 60, 110)));

            Assert.IsTrue(rect.Overlaps(Rect.MinMaxRect(40, 40, 60, 60)));
            Assert.IsTrue(rect.Overlaps(Rect.MinMaxRect(-10, -10, 110, 110)));



            Assert.IsFalse(rect.Overlaps(Rect.MinMaxRect(-10, -10, -5, -5)));
            Assert.IsFalse(rect.Overlaps(Rect.MinMaxRect(101, 101, 110, 110)));
        }
    }
}

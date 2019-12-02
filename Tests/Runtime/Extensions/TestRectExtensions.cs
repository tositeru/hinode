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
        [Test]
        public void OverlapsPointPasses()
        {
            var rect = new Rect(0, 0, 100, 100);

            Assert.IsTrue(rect.Overlaps(new Vector2(50, 50)));
            Assert.IsTrue(rect.Overlaps(new Vector2(0, 0)));
            Assert.IsTrue(rect.Overlaps(rect.size));
            Assert.IsFalse(rect.Overlaps(Vector2.one * -100f));
        }
    }
}

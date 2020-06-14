using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Extensions
{
    /// <summary>
    /// <seealso cref="RangeAttributeExtensions"/>
    /// </summary>
    public class TestRangeAttributeExtensions
    {
        class TestAttributeClass
        {
            [UnityEngine.Range(0, 100)]
            public float rangeValue;

            public static UnityEngine.RangeAttribute GetRangeAttribute()
            {
                return typeof(TestAttributeClass).GetField("rangeValue").GetCustomAttributes(false)
                    .OfType<UnityEngine.RangeAttribute>()
                    .First();
            }

        }

        /// <summary>
        /// <seealso cref="RangeAttributeExtensions.IsInRange(UnityEngine.RangeAttribute, float)"/>
        /// </summary>
        [Test]
        public void IsInRangeByFloatPasses()
        {
            var rangeAttr = TestAttributeClass.GetRangeAttribute();

            Assert.IsTrue(rangeAttr.IsInRange(0f));
            Assert.IsTrue(rangeAttr.IsInRange(50f));
            Assert.IsTrue(rangeAttr.IsInRange(100f));

            Assert.IsFalse(rangeAttr.IsInRange(0f - 0.1f));
            Assert.IsFalse(rangeAttr.IsInRange(100f + 0.1f));
        }

        /// <summary>
        /// <seealso cref="RangeAttributeExtensions.IsInRange(UnityEngine.RangeAttribute, double)"/>
        /// </summary>
        [Test]
        public void IsInRangeByDoublePasses()
        {
            var rangeAttr = TestAttributeClass.GetRangeAttribute();

            Assert.IsTrue(rangeAttr.IsInRange(0.0));
            Assert.IsTrue(rangeAttr.IsInRange(50.0));
            Assert.IsTrue(rangeAttr.IsInRange(100.0));

            Assert.IsFalse(rangeAttr.IsInRange(0.0 - 0.1));
            Assert.IsFalse(rangeAttr.IsInRange(100.0 + 0.1));
        }

        /// <summary>
        /// <seealso cref="RangeAttributeExtensions.Clamp(UnityEngine.RangeAttribute, float)"/>
        /// </summary>
        [Test]
        public void ClampByFloatPasses()
        {
            var rangeAttr = TestAttributeClass.GetRangeAttribute();

            Assert.AreEqual(0f, rangeAttr.Clamp(0f));
            Assert.AreEqual(50f, rangeAttr.Clamp(50f));
            Assert.AreEqual(100f, rangeAttr.Clamp(100f));

            Assert.AreEqual(0f, rangeAttr.Clamp(-0.1f));
            Assert.AreEqual(100f, rangeAttr.Clamp(101f));
        }

        /// <summary>
        /// <seealso cref="RangeAttributeExtensions.Clamp(UnityEngine.RangeAttribute, double)"/>
        /// </summary>
        [Test]
        public void ClampByDoublePasses()
        {
            var rangeAttr = TestAttributeClass.GetRangeAttribute();

            Assert.AreEqual(0.0, rangeAttr.Clamp(0.0));
            Assert.AreEqual(50.0, rangeAttr.Clamp(50.0));
            Assert.AreEqual(100.0, rangeAttr.Clamp(100.0));

            Assert.AreEqual(0.0, rangeAttr.Clamp(-0.1));
            Assert.AreEqual(100.0, rangeAttr.Clamp(101));
        }

    }
}

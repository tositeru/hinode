using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Hinode.Tests.Attributes.Extensions
{
    /// <summary>
	/// <see cref="RangeAttributeExtensions"/>
	/// </summary>
    public class TestRangeAttributeExtensions
    {
        class IsInRangePassesClass
		{
#pragma warning disable CS0649
            [UnityEngine.Range(-10, 10)] public float _f;
            [UnityEngine.Range(-20, 20)] public double _d;
#pragma warning restore CS0649
        }

        // A Test behaves as an ordinary method
        [Test]
        public void IsInRangeAndClampPasses()
        {
            var obj = new IsInRangePassesClass
            {
                _f = 0,
                _d = 2,
            };
            var floatRangeAttr = (UnityEngine.RangeAttribute)obj.GetType().GetField("_f").GetCustomAttributes(true).First(_a => _a is UnityEngine.RangeAttribute);
            Assert.IsTrue(floatRangeAttr.IsInRange(0f));
            Assert.IsFalse(floatRangeAttr.IsInRange(-1000f));
            Assert.IsFalse(floatRangeAttr.IsInRange(1000f));
            Assert.AreEqual(0f, floatRangeAttr.Clamp(0f));
            Assert.AreEqual(-10, floatRangeAttr.Clamp(-1000f));
            Assert.AreEqual(10, floatRangeAttr.Clamp(10000f));

            var doubleRangeAttr = (UnityEngine.RangeAttribute)obj.GetType().GetField("_d").GetCustomAttributes(true).First(_a => _a is UnityEngine.RangeAttribute);
            Assert.IsTrue(doubleRangeAttr.IsInRange(0f));
            Assert.IsFalse(doubleRangeAttr.IsInRange(-1000f));
            Assert.IsFalse(doubleRangeAttr.IsInRange(1000f));
            Assert.AreEqual(0f, doubleRangeAttr.Clamp(0f));
            Assert.AreEqual(-20, doubleRangeAttr.Clamp(-1000f));
            Assert.AreEqual(20, doubleRangeAttr.Clamp(10000f));
        }
    }
}

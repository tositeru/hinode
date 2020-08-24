using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Tests;

namespace Hinode.Layouts.Tests
{
    /// <summary>
    /// <seealso cref="LayoutInfo"/>
    /// </summary>
    public class TestLayoutInfo
    {
        #region UnitSize Property
        /// <summary>
        /// <seealso cref="LayoutInfo.UnitSize"/>
        /// </summary>
        [Test]
        public void UnitSizePropertyPasses()
        {
            var info = new LayoutInfo();

            var size = Vector3.one * 100f;
            info.UnitSize = size;

            AssertionUtils.AreNearlyEqual(size, info.UnitSize);
        }

        /// <summary>
        /// <seealso cref="LayoutInfo.UnitSize"/>
        /// <seealso cref="LayoutInfo.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValue_UnitSizePropertyPasses()
        {
            var info = new LayoutInfo();

            var callCounter = 0;
            (LayoutInfo self, LayoutInfo.ValueKind kind) recievedValue = default;
            info.OnChangedValue.Add((_self, _kind) => {
                callCounter++;
                recievedValue = (_self, _kind);
            });

            var size = Vector3.one * 100f;
            info.UnitSize = size;

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(info, recievedValue.self);
            Assert.AreEqual(LayoutInfo.ValueKind.UnitSize, recievedValue.kind);
        }

        /// <summary>
        /// <seealso cref="LayoutInfo.UnitSize"/>
        /// <seealso cref="LayoutInfo.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValue_UnitSizeProperty_WhenThrowExceptionPasses()
        {
            var info = new LayoutInfo();

            info.OnChangedValue.Add((_self, _kind) => {
                throw new System.Exception();
            });

            var size = Vector3.one * 100f;
            info.UnitSize = size;

            AssertionUtils.AreNearlyEqual(size, info.UnitSize);
        }
        #endregion
    }
}

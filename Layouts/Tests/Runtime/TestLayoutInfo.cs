using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Tests;
using System.Linq;

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

        #region Assign
        /// <summary>
        /// <seealso cref="LayoutInfo.Assign(LayoutInfo)"/>
        /// </summary>
        [Test]
        public void AssignPasses()
        {
            var info = new LayoutInfo();
            var other = new LayoutInfo();
            other.UnitSize = Vector3.one * 100f;

            info.Assign(other);

            AssertionUtils.AreNearlyEqual(other.UnitSize, info.UnitSize);
        }

        /// <summary>
        /// <seealso cref="LayoutInfo.Assign(LayoutInfo)"/>
        /// </summary>
        [Test]
        public void OnChangedValue_InAssignPasses()
        {
            var info = new LayoutInfo();
            var other = new LayoutInfo();

            var callCounter = 0;
            (LayoutInfo self, LayoutInfo.ValueKind kinds) recievedValues = default;
            info.OnChangedValue.Add((_self, _kinds) => {
                callCounter++;
                recievedValues = (_self, _kinds);
            });
            //例外が発生しても他のコールバックは実行されるようにしてください。
            info.OnChangedValue.Add((_, __) => throw new System.Exception());

            var flagCombination = IndexCombinationEnumerable.GetFlagEnumCombination(
                System.Enum.GetValues(typeof(LayoutInfo.ValueKind)).OfType<LayoutInfo.ValueKind>()
            );
            foreach(var kinds in flagCombination)
            {
                var errorMessage = $"Fail test... kinds={kinds}";
                if (0 != (kinds & LayoutInfo.ValueKind.UnitSize))
                    other.UnitSize = other.UnitSize + Vector3.one;

                callCounter = 0;
                recievedValues = default;

                info.Assign(other); //test point

                Assert.AreSame(info, recievedValues.self, errorMessage);
                Assert.AreEqual(kinds, recievedValues.kinds, errorMessage);
            }
        }
        #endregion
    }
}

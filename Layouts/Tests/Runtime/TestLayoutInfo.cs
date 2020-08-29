using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Tests;
using System.Linq;
using static System.Math;

namespace Hinode.Layouts.Tests
{
    /// <summary>
    /// <seealso cref="LayoutInfo"/>
    /// </summary>
    public class TestLayoutInfo
    {
        #region LayoutSize Property
        /// <summary>
        /// <seealso cref="LayoutInfo.LayoutSize"/>
        /// </summary>
        [Test]
        public void LayoutSizePropertyPasses()
        {
            var info = new LayoutInfo();
            AssertionUtils.AreNearlyEqual(LayoutInfo.UNFIXED_VECTOR3, info.LayoutSize, LayoutDefines.NUMBER_PRECISION, $"Default値が異なります。設定されていないことを表す値にしてください。");

            var size = Vector3.one * 100f;
            info.LayoutSize = size;

            AssertionUtils.AreNearlyEqual(size, info.LayoutSize);
        }

        /// <summary>
        /// <seealso cref="LayoutInfo.LayoutSize"/>
        /// <seealso cref="LayoutInfo.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValue_LayoutSizePropertyPasses()
        {
            var info = new LayoutInfo();

            var callCounter = 0;
            (LayoutInfo self, LayoutInfo.ValueKind kind, LayoutInfo.OnChangedValueParam prevInfo) recievedValue = default;
            info.OnChangedValue.Add((_self, _kind, _prev) => {
                callCounter++;
                recievedValue = (_self, _kind, _prev);
            });

            var prevSize = info.LayoutSize;
            var size = Vector3.one * 100f;
            info.LayoutSize = size;

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(info, recievedValue.self);
            Assert.AreEqual(LayoutInfo.ValueKind.LayoutSize, recievedValue.kind);
            Assert.AreEqual(prevSize, recievedValue.prevInfo.LayoutSize);
        }

        /// <summary>
        /// <seealso cref="LayoutInfo.LayoutSize"/>
        /// <seealso cref="LayoutInfo.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValue_LayoutSizeProperty_WhenThrowExceptionPasses()
        {
            var info = new LayoutInfo();

            info.OnChangedValue.Add((_self, _kind, _prev) => {
                throw new System.Exception();
            });

            var size = Vector3.one * 100f;
            info.LayoutSize = size;

            AssertionUtils.AreNearlyEqual(size, info.LayoutSize);
        }
        #endregion

        #region MinSize Propetry
        /// <summary>
        /// <seealso cref="LayoutInfo.MinSize"/>
        /// </summary>
        [Test]
        public void MinSizePropertyPasses()
        {
            {
                var info = new LayoutInfo();
                AssertionUtils.AreNearlyEqual(LayoutInfo.UNFIXED_VECTOR3, info.MinSize, LayoutDefines.NUMBER_PRECISION, $"Default値が異なります。設定されていないことを表す値にしてください。");
            }
            Debug.Log($"Success to Default MinSize Property!");

            {
                var info = new LayoutInfo();
                var size = Vector3.one * 100f;
                info.MinSize = size;

                AssertionUtils.AreNearlyEqual(size, info.MinSize);
            }
            Debug.Log($"Success to Set MinSize Property!");

            {
                var info = new LayoutInfo();
                info.MaxSize = Vector3.one * 50f;
                info.MinSize = Vector3.one * 100f;
                AssertionUtils.AreNearlyEqual(info.MaxSize, info.MinSize);
            }
            Debug.Log($"Success to MinSize Property When greater MaxSize!");

            {
                var info = new LayoutInfo();
                info.MaxSize = Vector3.one * 50f;
                info.MinSize = LayoutInfo.UNFIXED_VECTOR3;
                AssertionUtils.AreNearlyEqual(LayoutInfo.UNFIXED_VECTOR3, info.MinSize);
            }
            Debug.Log($"Success to MinSize Property When INVALID_VECTOR3!");
        }

        /// <summary>
        /// <seealso cref="LayoutInfo.MinSize"/>
        /// <seealso cref="LayoutInfo.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValue_MinSizePropertyPasses()
        {
            var info = new LayoutInfo();

            var callCounter = 0;
            (LayoutInfo self, LayoutInfo.ValueKind kind, LayoutInfo.OnChangedValueParam prevInfo) recievedValue = default;
            info.OnChangedValue.Add((_self, _kind, _prev) => {
                callCounter++;
                recievedValue = (_self, _kind, _prev);
            });

            var prevMinSize = info.MinSize;
            var size = Vector3.one * 100f;
            info.MinSize = size;

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(info, recievedValue.self);
            Assert.AreEqual(LayoutInfo.ValueKind.MinSize, recievedValue.kind);
            Assert.AreEqual(prevMinSize, recievedValue.prevInfo.MinSize);
        }

        /// <summary>
        /// <seealso cref="LayoutInfo.MinSize"/>
        /// <seealso cref="LayoutInfo.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValue_MinSizeProperty_WhenThrowExceptionPasses()
        {
            var info = new LayoutInfo();

            info.OnChangedValue.Add((_self, _kind, _prev) => {
                throw new System.Exception();
            });

            var size = Vector3.one * 100f;
            info.MinSize = size;

            AssertionUtils.AreNearlyEqual(size, info.MinSize);
        }
        #endregion

        #region MaxSize Property
        /// <summary>
        /// <seealso cref="LayoutInfo.MaxSize"/>
        /// </summary>
        [Test]
        public void MaxSizePropertyPasses()
        {
            {
                var info = new LayoutInfo();
                AssertionUtils.AreNearlyEqual(LayoutInfo.UNFIXED_VECTOR3, info.MaxSize, LayoutDefines.NUMBER_PRECISION, $"Default値が異なります。設定されていないことを表す値にしてください。");
            }
            Debug.Log($"Success to Default MaxSize Property!");

            {
                var info = new LayoutInfo();
                var size = Vector3.one * 100f;
                info.MaxSize = size;

                AssertionUtils.AreNearlyEqual(size, info.MaxSize);
            }
            Debug.Log($"Success to Set MaxSize Property!");

            {
                var info = new LayoutInfo();
                info.MinSize = Vector3.one * 100f;
                info.MaxSize = Vector3.one * 50f;
                AssertionUtils.AreNearlyEqual(info.MinSize, info.MaxSize);
            }
            Debug.Log($"Success to MaxSize Property When greater MaxSize!");

            {
                var info = new LayoutInfo();
                info.MinSize = Vector3.one * 50f;
                info.MaxSize = LayoutInfo.UNFIXED_VECTOR3;
                AssertionUtils.AreNearlyEqual(LayoutInfo.UNFIXED_VECTOR3, info.MaxSize);
            }
            Debug.Log($"Success to MaxSize Property When INVALID_VECTOR3!");
        }

        /// <summary>
        /// <seealso cref="LayoutInfo.MaxSize"/>
        /// <seealso cref="LayoutInfo.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValue_MaxSizePropertyPasses()
        {
            var info = new LayoutInfo();

            var callCounter = 0;
            (LayoutInfo self, LayoutInfo.ValueKind kind, LayoutInfo.OnChangedValueParam prevInfo) recievedValue = default;
            info.OnChangedValue.Add((_self, _kind, _prev) => {
                callCounter++;
                recievedValue = (_self, _kind, _prev);
            });

            var prevMaxSize = info.MaxSize;
            var size = Vector3.one * 100f;
            info.MaxSize = size;

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(info, recievedValue.self);
            Assert.AreEqual(LayoutInfo.ValueKind.MaxSize, recievedValue.kind);
            Assert.AreEqual(prevMaxSize, recievedValue.prevInfo.MaxSize);
        }

        /// <summary>
        /// <seealso cref="LayoutInfo.MaxSize"/>
        /// <seealso cref="LayoutInfo.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValue_MaxSizeProperty_WhenThrowExceptionPasses()
        {
            var info = new LayoutInfo();

            info.OnChangedValue.Add((_self, _kind, _prev) => {
                throw new System.Exception();
            });

            var size = Vector3.one * 100f;
            info.MaxSize = size;

            AssertionUtils.AreNearlyEqual(size, info.MaxSize);
        }
        #endregion

        #region SetMinMaxSize
        /// <summary>
        /// <seealso cref="LayoutInfo.SetMinMaxSize(Vector3, Vector3)"/>
        /// </summary>
        [Test]
        public void SetMinMaxSizePasses()
        {
            {
                var info = new LayoutInfo();
                var min = Vector3.one * 10f;
                var max = Vector3.one * 100f;
                info.SetMinMaxSize(min, max);

                AssertionUtils.AreNearlyEqual(min, info.MinSize);
                AssertionUtils.AreNearlyEqual(max, info.MaxSize);
            }
            Debug.Log($"Success to SetMinMaxSize()!");

            {
                var info = new LayoutInfo();
                var min = new Vector3(20, 40, 60);
                var max = new Vector3(30, 10, 70);
                info.SetMinMaxSize(min, max);

                AssertionUtils.AreNearlyEqual(Vector3.Min(min, max), info.MinSize);
                AssertionUtils.AreNearlyEqual(Vector3.Max(min, max), info.MaxSize);
            }
            Debug.Log($"Success to MaxSize Property When greater MaxSize!");

            {
                var info = new LayoutInfo();
                var min = new Vector3(-1, 40, -1);
                var max = new Vector3(30, -1, 70);
                info.SetMinMaxSize(min, max);

                AssertionUtils.AreNearlyEqual(new Vector3(-1, 40, -1), info.MinSize);
                AssertionUtils.AreNearlyEqual(new Vector3(30, -1, 70), info.MaxSize);
            }
            Debug.Log($"Success to MaxSize Property When INVALID_VECTOR3!");
        }

        /// <summary>
        /// <seealso cref="LayoutInfo.SetMinMaxSize(Vector3, Vector3)"/>
        /// <seealso cref="LayoutInfo.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValue_SetMinMaxSizePasses()
        {
            var info = new LayoutInfo();

            var callCounter = 0;
            (LayoutInfo self, LayoutInfo.ValueKind kind, LayoutInfo.OnChangedValueParam prevInfo) recievedValue = default;
            info.OnChangedValue.Add((_self, _kind, _prev) => {
                callCounter++;
                recievedValue = (_self, _kind, _prev);
            });

            {
                callCounter = 0;
                recievedValue = default;

                var prev = (info.MinSize, info.MaxSize);
                var min = Vector3.one * 10f;
                var max = Vector3.one * 100f;
                info.SetMinMaxSize(min, max);

                Assert.AreEqual(1, callCounter);
                Assert.AreSame(info, recievedValue.self);
                Assert.AreEqual(LayoutInfo.ValueKind.MinSize | LayoutInfo.ValueKind.MaxSize, recievedValue.kind);
                Assert.AreEqual(prev.MinSize, recievedValue.prevInfo.MinSize);
                Assert.AreEqual(prev.MaxSize, recievedValue.prevInfo.MaxSize);
            }
            Debug.Log($"Success to OnChangedValue When SetMinMaxSize()!");

            {
                callCounter = 0;
                recievedValue = default;

                var prev = (info.MinSize, info.MaxSize);
                var min = info.MinSize + Vector3.one;
                var max = info.MaxSize;
                info.SetMinMaxSize(min, max);

                Assert.AreEqual(1, callCounter);
                Assert.AreSame(info, recievedValue.self);
                Assert.AreEqual(LayoutInfo.ValueKind.MinSize, recievedValue.kind);
                Assert.AreEqual(prev.MinSize, recievedValue.prevInfo.MinSize);
                Assert.AreEqual(prev.MaxSize, recievedValue.prevInfo.MaxSize);
            }
            Debug.Log($"Success to OnChangedValue When SetMinMaxSize(min, same max)!");

            {
                callCounter = 0;
                recievedValue = default;

                var prev = (info.MinSize, info.MaxSize);
                var min = info.MinSize;
                var max = info.MaxSize + Vector3.one;
                info.SetMinMaxSize(min, max);

                Assert.AreEqual(1, callCounter);
                Assert.AreSame(info, recievedValue.self);
                Assert.AreEqual(LayoutInfo.ValueKind.MaxSize, recievedValue.kind);
                Assert.AreEqual(prev.MinSize, recievedValue.prevInfo.MinSize);
                Assert.AreEqual(prev.MaxSize, recievedValue.prevInfo.MaxSize);
            }
            Debug.Log($"Success to OnChangedValue When SetMinMaxSize(same min, max)!");
        }

        /// <summary>
        /// <seealso cref="LayoutInfo.SetMinMaxSize(Vector3, Vector3)"/>
        /// <seealso cref="LayoutInfo.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValue_SetMinMaxSize_WhenThrowExceptionPasses()
        {
            var info = new LayoutInfo();

            info.OnChangedValue.Add((_self, _kind, _prev) => {
                throw new System.Exception();
            });

            var min = Vector3.one * 10f;
            var max = Vector3.one * 100f;
            info.SetMinMaxSize(min, max);

            AssertionUtils.AreNearlyEqual(min, info.MinSize);
            AssertionUtils.AreNearlyEqual(max, info.MaxSize);
        }
        #endregion

        #region GetLayoutSize
        /// <summary>
        /// <seealso cref="LayoutInfo.GetLayoutSize(ILayoutTarget)"/>
        /// </summary>
        [Test]
        public void GetLayoutSizePasses()
        {
            var rnd = new System.Random();
            var minValue = -10f;
            var maxValue = 1000f;

            for (var i = 0; i < 100; ++i)
            {
                var info = new LayoutInfo();
                info.LayoutSize = new Vector3(
                    rnd.Range(minValue, maxValue),
                    rnd.Range(minValue, maxValue),
                    rnd.Range(minValue, maxValue)
                );
                info.MinSize = new Vector3(
                    rnd.Range(minValue, maxValue),
                    rnd.Range(minValue, maxValue),
                    rnd.Range(minValue, maxValue)
                );

                var target = new LayoutTargetObject();
                target.SetLocalSize(new Vector3(
                    rnd.Range(minValue, maxValue),
                    rnd.Range(minValue, maxValue),
                    rnd.Range(minValue, maxValue)
                ));

                var errorMessage = $"Fail test... LayoutSize={info.LayoutSize:F4}, MinSize={info.MinSize:F4}, Target LocalSize={target.LocalSize: F4}";
                var result = info.GetLayoutSize(target);
                var correct = Vector3.Max(info.MinSize, target.LocalSize);
                correct.x = info.LayoutSize.x < 0 ? correct.x : Min(correct.x, info.LayoutSize.x);
                correct.y = info.LayoutSize.y < 0 ? correct.y : Min(correct.y, info.LayoutSize.y);
                correct.z = info.LayoutSize.z < 0 ? correct.z : Min(correct.z, info.LayoutSize.z);
                AssertionUtils.AreNearlyEqual(correct, result, LayoutDefines.NUMBER_PRECISION, errorMessage);
            }
        }
        #endregion

        #region IgnoreLayoutGroup Property
        /// <summary>
        /// <seealso cref="LayoutInfo.IgnoreLayoutGroup"/>
        /// </summary>
        [Test]
        public void IgnoreLayoutGroupPropertyPasses()
        {
            var info = new LayoutInfo();
            Assert.IsFalse(info.IgnoreLayoutGroup, $"Default値が異なります。");

            info.IgnoreLayoutGroup = true;
            Assert.IsTrue(info.IgnoreLayoutGroup);
        }

        /// <summary>
        /// <seealso cref="LayoutInfo.IgnoreLayoutGroup"/>
        /// <seealso cref="LayoutInfo.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValue_IgnoreLayoutGroupPropertyPasses()
        {
            var info = new LayoutInfo();

            var callCounter = 0;
            (LayoutInfo self, LayoutInfo.ValueKind kind, LayoutInfo.OnChangedValueParam prevInfo) recievedValue = default;
            info.OnChangedValue.Add((_self, _kind, _prev) => {
                callCounter++;
                recievedValue = (_self, _kind, _prev);
            });

            {
                var prevIgnore = info.IgnoreLayoutGroup;
                info.IgnoreLayoutGroup = true;
                Assert.AreEqual(1, callCounter);
                Assert.AreSame(info, recievedValue.self);
                Assert.AreEqual(LayoutInfo.ValueKind.IgnoreLayoutGroup, recievedValue.kind);
                Assert.AreEqual(prevIgnore, recievedValue.prevInfo.IgnoreLayoutGroup);
            }

            {
                callCounter = 0;
                recievedValue = default;

                info.IgnoreLayoutGroup = info.IgnoreLayoutGroup;
                Assert.AreEqual(0, callCounter);
            }
        }

        /// <summary>
        /// <seealso cref="LayoutInfo.IgnoreLayoutGroup"/>
        /// <seealso cref="LayoutInfo.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValue_IgnoreLayoutGroupProperty_WhenThrowExceptionPasses()
        {
            var info = new LayoutInfo();

            info.OnChangedValue.Add((_self, _kind, _prev) => {
                throw new System.Exception();
            });

            info.IgnoreLayoutGroup = true;

            Assert.IsTrue(info.IgnoreLayoutGroup);
        }
        #endregion

        #region SizeGrowInGroup Property
        /// <summary>
        /// <seealso cref="LayoutInfo.SizeGrowInGroup"/>
        /// </summary>
        [Test]
        public void SizeGrowInGroupPropertyPasses()
        {
            var info = new LayoutInfo();
            AssertionUtils.AreNearlyEqual(1f, info.SizeGrowInGroup, LayoutDefines.NUMBER_PRECISION, $"Default値が異なります。");

            var rnd = new System.Random();
            for(var i=0; i<100; ++i)
            {
                var value = rnd.Range(-10, 100);
                info.SizeGrowInGroup = value;

                var correct = Max(value, 0f);
                AssertionUtils.AreNearlyEqual(correct, info.SizeGrowInGroup, LayoutDefines.NUMBER_PRECISION);
            }
        }

        /// <summary>
        /// <seealso cref="LayoutInfo.SizeGrowInGroup"/>
        /// <seealso cref="LayoutInfo.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValue_SizeGrowInGroupPropertyPasses()
        {
            var info = new LayoutInfo();

            var callCounter = 0;
            (LayoutInfo self, LayoutInfo.ValueKind kind, LayoutInfo.OnChangedValueParam prevInfo) recievedValue = default;
            info.OnChangedValue.Add((_self, _kind, _prev) => {
                callCounter++;
                recievedValue = (_self, _kind, _prev);
            });

            {
                var prevGrow = info.SizeGrowInGroup;
                info.SizeGrowInGroup = 2f;
                Assert.AreEqual(1, callCounter);
                Assert.AreSame(info, recievedValue.self);
                Assert.AreEqual(LayoutInfo.ValueKind.SizeGrowInGroup, recievedValue.kind);
                Assert.AreEqual(prevGrow, recievedValue.prevInfo.SizeGrowInGroup);
            }

            {
                callCounter = 0;
                recievedValue = default;

                info.SizeGrowInGroup = info.SizeGrowInGroup;
                Assert.AreEqual(0, callCounter);
            }
        }

        /// <summary>
        /// <seealso cref="LayoutInfo.SizeGrowInGroup"/>
        /// <seealso cref="LayoutInfo.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValue_SizeGrowInGroupProperty_WhenThrowExceptionPasses()
        {
            var info = new LayoutInfo();

            info.OnChangedValue.Add((_self, _kind, _prev) => {
                throw new System.Exception();
            });

            var value = 10f;
            info.SizeGrowInGroup = value;

            AssertionUtils.AreNearlyEqual(value, info.SizeGrowInGroup, LayoutDefines.NUMBER_PRECISION);
        }
        #endregion

        #region OrderInGroup OrderInGroup
        /// <summary>
        /// <seealso cref="LayoutInfo.OrderInGroup"/>
        /// </summary>
        [Test]
        public void OrderInGroupPropertyPasses()
        {
            var info = new LayoutInfo();
            Assert.AreEqual(0, info.OrderInGroup, $"Default値が異なります。");

            var rnd = new System.Random();
            for (var i = 0; i < 100; ++i)
            {
                var value = rnd.Range(-100, 100);
                info.OrderInGroup = value;

                Assert.AreEqual(value, info.OrderInGroup);
            }
        }

        /// <summary>
        /// <seealso cref="LayoutInfo.OrderInGroup"/>
        /// <seealso cref="LayoutInfo.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValue_OrderInGroupPropertyPasses()
        {
            var info = new LayoutInfo();

            var callCounter = 0;
            (LayoutInfo self, LayoutInfo.ValueKind kind, LayoutInfo.OnChangedValueParam prevInfo) recievedValue = default;
            info.OnChangedValue.Add((_self, _kind, _prev) => {
                callCounter++;
                recievedValue = (_self, _kind, _prev);
            });

            {
                var prevOrder = info.OrderInGroup;
                info.OrderInGroup += 2;
                Assert.AreEqual(1, callCounter);
                Assert.AreSame(info, recievedValue.self);
                Assert.AreEqual(LayoutInfo.ValueKind.OrderInGroup, recievedValue.kind);
                Assert.AreEqual(prevOrder, recievedValue.prevInfo.OrderInGroup);
            }

            {
                callCounter = 0;
                recievedValue = default;

                info.OrderInGroup = info.OrderInGroup;
                Assert.AreEqual(0, callCounter);
            }
        }

        /// <summary>
        /// <seealso cref="LayoutInfo.OrderInGroup"/>
        /// <seealso cref="LayoutInfo.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValue_OrderInGroupProperty_WhenThrowExceptionPasses()
        {
            var info = new LayoutInfo();

            info.OnChangedValue.Add((_self, _kind, _prev) => {
                throw new System.Exception();
            });

            var value = info.OrderInGroup + 2;
            info.OrderInGroup = value;

            Assert.AreEqual(value, info.OrderInGroup, LayoutDefines.NUMBER_PRECISION);
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
            other.LayoutSize = Vector3.one * 100f;
            other.SetMinMaxSize(Vector3.one * 10f, Vector3.one * 200f);
            other.IgnoreLayoutGroup = true;
            other.SizeGrowInGroup = 2f;
            other.OrderInGroup = 10;

            info.Assign(other);

            AssertionUtils.AreNearlyEqual(other.LayoutSize, info.LayoutSize);
            AssertionUtils.AreNearlyEqual(other.MinSize, info.MinSize);
            AssertionUtils.AreNearlyEqual(other.MaxSize, info.MaxSize);
            Assert.AreEqual(other.IgnoreLayoutGroup, info.IgnoreLayoutGroup);
            AssertionUtils.AreNearlyEqual(other.SizeGrowInGroup, info.SizeGrowInGroup);
            Assert.AreEqual(other.OrderInGroup, info.OrderInGroup);
        }

        /// <summary>
        /// <seealso cref="LayoutInfo.Assign(LayoutInfo)"/>
        /// </summary>
        [Test]
        public void OnChangedValue_InAssignPasses()
        {
            var info = new LayoutInfo();
            var other = new LayoutInfo();
            info.MaxSize = other.MaxSize = Vector3.one * 100f; // Callbackの呼び出しのみをテストするので、MinSizeより下回らないようにしています。

            var callCounter = 0;
            (LayoutInfo self, LayoutInfo.ValueKind kinds, LayoutInfo.OnChangedValueParam prevInfo) recievedValues = default;
            info.OnChangedValue.Add((_self, _kinds, _prev) => {
                callCounter++;
                recievedValues = (_self, _kinds, _prev);
            });
            //例外が発生しても他のコールバックは実行されるようにしてください。
            info.OnChangedValue.Add((_, __, ___) => throw new System.Exception());

            var flagCombination = IndexCombinationEnumerable.GetFlagEnumCombination(
                System.Enum.GetValues(typeof(LayoutInfo.ValueKind)).OfType<LayoutInfo.ValueKind>()
            );
            foreach(var kinds in flagCombination)
            {
                var prev = new LayoutInfo(info);
                var errorMessage = $"Fail test... kinds={kinds}";
                if (0 != (kinds & LayoutInfo.ValueKind.LayoutSize))
                    other.LayoutSize = other.LayoutSize + Vector3.one;
                if (0 != (kinds & LayoutInfo.ValueKind.MinSize))
                    other.MinSize = other.MinSize + Vector3.one;
                if (0 != (kinds & LayoutInfo.ValueKind.MaxSize))
                    other.MaxSize = other.MaxSize + Vector3.one;
                if (0 != (kinds & LayoutInfo.ValueKind.IgnoreLayoutGroup))
                    other.IgnoreLayoutGroup = !other.IgnoreLayoutGroup;
                if (0 != (kinds & LayoutInfo.ValueKind.SizeGrowInGroup))
                    other.SizeGrowInGroup= other.SizeGrowInGroup + 1f;
                if (0 != (kinds & LayoutInfo.ValueKind.OrderInGroup))
                    other.OrderInGroup = other.OrderInGroup + 1;

                callCounter = 0;
                recievedValues = default;

                info.Assign(other); //test point

                Assert.AreSame(info, recievedValues.self, errorMessage);
                Assert.AreEqual(kinds, recievedValues.kinds, errorMessage);

                Assert.AreNotSame(info, recievedValues.prevInfo);
                Assert.AreEqual(prev.LayoutSize, recievedValues.prevInfo.LayoutSize);
                Assert.AreEqual(prev.MinSize, recievedValues.prevInfo.MinSize);
                Assert.AreEqual(prev.MaxSize, recievedValues.prevInfo.MaxSize);
                Assert.AreEqual(prev.IgnoreLayoutGroup, recievedValues.prevInfo.IgnoreLayoutGroup);
                Assert.AreEqual(prev.SizeGrowInGroup, recievedValues.prevInfo.SizeGrowInGroup);
                Assert.AreEqual(prev.OrderInGroup, recievedValues.prevInfo.OrderInGroup);
            }
        }
        #endregion
    }
}

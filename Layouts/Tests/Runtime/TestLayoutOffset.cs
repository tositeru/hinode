using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Layouts.Tests
{
    /// <summary>
	/// <seealso cref="LayoutOffset"/>
	/// </summary>
    public class TestLayoutOffset
    {
        #region CurrentUnit Property
        /// <summary>
        /// <seealso cref="LayoutOffset.CurrentUnit"/>
        /// </summary>
        [Test]
        public void CurrentUnitPrpertyPasses()
        {
            var offset = new LayoutOffset();
            Assert.AreEqual(LayoutOffset.Unit.Pixel, offset.CurrentUnit);

            offset.CurrentUnit = LayoutOffset.Unit.Ratio;
            Assert.AreEqual(LayoutOffset.Unit.Ratio, offset.CurrentUnit);
        }

        /// <summary>
		/// <seealso cref="LayoutOffset.CurrentUnit"/>
		/// <seealso cref="LayoutOffset.OnChangedValue"/>
		/// </summary>
        [Test]
        public void OnChangedValueInCurrentUnitPrpertyPasses()
        {
            var offset = new LayoutOffset();
            offset.CurrentUnit = LayoutOffset.Unit.Pixel;
            int callCounter = 0;
            (LayoutOffset self, LayoutOffset.ValueKind kinds) recievedData = default;
            offset.OnChangedValue.Add((_self, _kind) => {
                callCounter++;
                recievedData = (_self, _kind);
            });

            offset.CurrentUnit = LayoutOffset.Unit.Ratio;
            Assert.AreEqual(1, callCounter);
            Assert.AreSame(offset, recievedData.self);
            Assert.AreEqual(LayoutOffset.ValueKind.CurrentUnit, recievedData.kinds);
        }

        /// <summary>
        /// <seealso cref="LayoutOffset.CurrentUnit"/>
        /// <seealso cref="LayoutOffset.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValueInCurrentUnitPrpertyWhenThrowExceptionPasses()
        {
            var offset = new LayoutOffset();
            offset.CurrentUnit = LayoutOffset.Unit.Pixel;
            offset.OnChangedValue.Add((_self, _kind) => {
                throw new System.Exception();
            });

            offset.CurrentUnit = LayoutOffset.Unit.Ratio;
            Assert.AreEqual(LayoutOffset.Unit.Ratio, offset.CurrentUnit);
        }
        #endregion


        #region Left Property
        /// <summary>
        /// <seealso cref="LayoutOffset.Left"/>
        /// </summary>
        [Test]
        public void LeftPrpertyPasses()
        {
            var offset = new LayoutOffset();
            Assert.AreEqual(0f, offset.Left);

            var value = 100f;
            offset.Left = value;
            Assert.AreEqual(value, offset.Left);
        }

        /// <summary>
		/// <seealso cref="LayoutOffset.Left"/>
		/// <seealso cref="LayoutOffset.OnChangedValue"/>
		/// </summary>
        [Test]
        public void OnChangedValueInLeftPrpertyPasses()
        {
            var offset = new LayoutOffset();
            int callCounter = 0;
            (LayoutOffset self, LayoutOffset.ValueKind kinds) recievedData = default;
            offset.OnChangedValue.Add((_self, _kind) => {
                callCounter++;
                recievedData = (_self, _kind);
            });

            offset.Left = 100f;
            Assert.AreEqual(1, callCounter);
            Assert.AreSame(offset, recievedData.self);
            Assert.AreEqual(LayoutOffset.ValueKind.Left, recievedData.kinds);
        }

        /// <summary>
        /// <seealso cref="LayoutOffset.Left"/>
        /// <seealso cref="LayoutOffset.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValueInLeftPrpertyWhenThrowExceptionPasses()
        {
            var offset = new LayoutOffset();
            offset.OnChangedValue.Add((_self, _kind) => {
                throw new System.Exception();
            });

            var value = 100f;
            offset.Left = value;
            Assert.AreEqual(value, offset.Left);
        }
        #endregion

        #region Right Property
        /// <summary>
        /// <seealso cref="LayoutOffset.Right"/>
        /// </summary>
        [Test]
        public void RightPrpertyPasses()
        {
            var offset = new LayoutOffset();
            Assert.AreEqual(0f, offset.Right);

            var value = 100f;
            offset.Right = value;
            Assert.AreEqual(value, offset.Right);
        }

        /// <summary>
		/// <seealso cref="LayoutOffset.Right"/>
		/// <seealso cref="LayoutOffset.OnChangedValue"/>
		/// </summary>
        [Test]
        public void OnChangedValueInRightPrpertyPasses()
        {
            var offset = new LayoutOffset();
            int callCounter = 0;
            (LayoutOffset self, LayoutOffset.ValueKind kinds) recievedData = default;
            offset.OnChangedValue.Add((_self, _kind) => {
                callCounter++;
                recievedData = (_self, _kind);
            });

            offset.Right = 100f;
            Assert.AreEqual(1, callCounter);
            Assert.AreSame(offset, recievedData.self);
            Assert.AreEqual(LayoutOffset.ValueKind.Right, recievedData.kinds);
        }

        /// <summary>
        /// <seealso cref="LayoutOffset.Right"/>
        /// <seealso cref="LayoutOffset.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValueInRightPrpertyWhenThrowExceptionPasses()
        {
            var offset = new LayoutOffset();
            offset.OnChangedValue.Add((_self, _kind) => {
                throw new System.Exception();
            });

            var value = 100f;
            offset.Right = value;
            Assert.AreEqual(value, offset.Right);
        }
        #endregion

        #region Top Property
        /// <summary>
        /// <seealso cref="LayoutOffset.Top"/>
        /// </summary>
        [Test]
        public void TopPrpertyPasses()
        {
            var offset = new LayoutOffset();
            Assert.AreEqual(0f, offset.Top);

            var value = 100f;
            offset.Top = value;
            Assert.AreEqual(value, offset.Top);
        }

        /// <summary>
		/// <seealso cref="LayoutOffset.Top"/>
		/// <seealso cref="LayoutOffset.OnChangedValue"/>
		/// </summary>
        [Test]
        public void OnChangedValueInTopPrpertyPasses()
        {
            var offset = new LayoutOffset();
            int callCounter = 0;
            (LayoutOffset self, LayoutOffset.ValueKind kinds) recievedData = default;
            offset.OnChangedValue.Add((_self, _kind) => {
                callCounter++;
                recievedData = (_self, _kind);
            });

            offset.Top = 100f;
            Assert.AreEqual(1, callCounter);
            Assert.AreSame(offset, recievedData.self);
            Assert.AreEqual(LayoutOffset.ValueKind.Top, recievedData.kinds);
        }

        /// <summary>
        /// <seealso cref="LayoutOffset.Top"/>
        /// <seealso cref="LayoutOffset.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValueInTopPrpertyWhenThrowExceptionPasses()
        {
            var offset = new LayoutOffset();
            offset.OnChangedValue.Add((_self, _kind) => {
                throw new System.Exception();
            });

            var value = 100f;
            offset.Top = value;
            Assert.AreEqual(value, offset.Top);
        }
        #endregion

        #region Bottom Property
        /// <summary>
        /// <seealso cref="LayoutOffset.Bottom"/>
        /// </summary>
        [Test]
        public void BottomPrpertyPasses()
        {
            var offset = new LayoutOffset();
            Assert.AreEqual(0f, offset.Bottom);

            var value = 100f;
            offset.Bottom = value;
            Assert.AreEqual(value, offset.Bottom);
        }

        /// <summary>
		/// <seealso cref="LayoutOffset.Bottom"/>
		/// <seealso cref="LayoutOffset.OnChangedValue"/>
		/// </summary>
        [Test]
        public void OnChangedValueInBottomPrpertyPasses()
        {
            var offset = new LayoutOffset();
            int callCounter = 0;
            (LayoutOffset self, LayoutOffset.ValueKind kinds) recievedData = default;
            offset.OnChangedValue.Add((_self, _kind) => {
                callCounter++;
                recievedData = (_self, _kind);
            });

            offset.Bottom = 100f;
            Assert.AreEqual(1, callCounter);
            Assert.AreSame(offset, recievedData.self);
            Assert.AreEqual(LayoutOffset.ValueKind.Bottom, recievedData.kinds);
        }

        /// <summary>
        /// <seealso cref="LayoutOffset.Bottom"/>
        /// <seealso cref="LayoutOffset.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValueInBottomPrpertyWhenThrowExceptionPasses()
        {
            var offset = new LayoutOffset();
            offset.OnChangedValue.Add((_self, _kind) => {
                throw new System.Exception();
            });

            var value = 100f;
            offset.Bottom = value;
            Assert.AreEqual(value, offset.Bottom);
        }
        #endregion

        #region SetOffsets
        /// <summary>
        /// <seealso cref="LayoutOffset.SetOffsets(float left, float right, float top, float bottom)"/>
        /// </summary>
        [Test]
        public void SetOffsetsPrpertyPasses()
        {
            var offset = new LayoutOffset();

            var left = 20;
            var right = 40f;
            var top = 10f;
            var bottom = 30f;
            offset.SetOffsets(left, right, top, bottom);

            Assert.AreEqual(left, offset.Left);
            Assert.AreEqual(right, offset.Right);
            Assert.AreEqual(top, offset.Top);
            Assert.AreEqual(bottom, offset.Bottom);
        }

        /// <summary>
		/// <seealso cref="LayoutOffset.SetOffsets(float left, float right, float top, float bottom)"/>
		/// <seealso cref="LayoutOffset.OnChangedValue"/>
		/// </summary>
        [Test]
        public void OnChangedValueInSetOffsetsPrpertyPasses()
        {
            var offset = new LayoutOffset();
            int callCounter = 0;
            (LayoutOffset self, LayoutOffset.ValueKind kinds) recievedData = default;
            offset.OnChangedValue.Add((_self, _kind) => {
                callCounter++;
                recievedData = (_self, _kind);
            });

            offset.SetOffsets(10, 20, 30, 40);

            Assert.AreEqual(1, callCounter, $"コールバックは一度だけ呼び出されるようにしてください");
            Assert.AreSame(offset, recievedData.self);
            Assert.AreEqual(LayoutOffset.ValueKind.Left | LayoutOffset.ValueKind.Right | LayoutOffset.ValueKind.Top | LayoutOffset.ValueKind.Bottom, recievedData.kinds);

            {//同じ値の時は変更されないので、OnChangedValueのkindsには含めないようにする
                var kindsCombination = IndexCombinationEnumerable.GetFlagEnumCombination(
                    LayoutOffset.ValueKind.Left,
                    LayoutOffset.ValueKind.Right,
                    LayoutOffset.ValueKind.Top,
                    LayoutOffset.ValueKind.Bottom
                );
                foreach(var kinds in kindsCombination)
                {
                    var left = (0 != (kinds & LayoutOffset.ValueKind.Left)) ? offset.Left + 10f : offset.Left;
                    var right = (0 != (kinds & LayoutOffset.ValueKind.Right)) ? offset.Right + 10f : offset.Right;
                    var top = (0 != (kinds & LayoutOffset.ValueKind.Top)) ? offset.Top + 10f : offset.Top;
                    var bottom = (0 != (kinds & LayoutOffset.ValueKind.Bottom)) ? offset.Bottom + 10f : offset.Bottom;
                    callCounter = 0;
                    offset.SetOffsets(left, right, top, bottom);

                    var errorMessage = $"Fail test... ValueKinds=>{kinds}";

                    Assert.AreEqual(1, callCounter, errorMessage);
                    Assert.AreSame(offset, recievedData.self, errorMessage);
                    Assert.AreEqual(kinds, recievedData.kinds, errorMessage);
                }
            }
            Debug.Log($"Success to All pattern!!");
        }

        /// <summary>
        /// <seealso cref="LayoutOffset.SetOffsets(float left, float right, float top, float bottom)"/>
        /// <seealso cref="LayoutOffset.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValueInSetOffsetsPrpertyWhenThrowExceptionPasses()
        {
            var offset = new LayoutOffset();
            offset.OnChangedValue.Add((_self, _kind) => {
                throw new System.Exception();
            });

            {
                var left = 20;
                var right = 40f;
                var top = 10f;
                var bottom = 30f;
                offset.SetOffsets(left, right, top, bottom);

                Assert.AreEqual(left, offset.Left);
                Assert.AreEqual(right, offset.Right);
                Assert.AreEqual(top, offset.Top);
                Assert.AreEqual(bottom, offset.Bottom);
            }

            {//同じ値の時は変更されないので、OnChangedValueのkindsには含めないようにする
                var kindsCombination = IndexCombinationEnumerable.GetFlagEnumCombination(
                    LayoutOffset.ValueKind.Left,
                    LayoutOffset.ValueKind.Right,
                    LayoutOffset.ValueKind.Top,
                    LayoutOffset.ValueKind.Bottom
                );
                foreach (var kinds in kindsCombination)
                {
                    var left = (0 != (kinds & LayoutOffset.ValueKind.Left)) ? offset.Left + 10f : offset.Left;
                    var right = (0 != (kinds & LayoutOffset.ValueKind.Right)) ? offset.Right + 10f : offset.Right;
                    var top = (0 != (kinds & LayoutOffset.ValueKind.Top)) ? offset.Top + 10f : offset.Top;
                    var bottom = (0 != (kinds & LayoutOffset.ValueKind.Bottom)) ? offset.Bottom + 10f : offset.Bottom;
                    offset.SetOffsets(left, right, top, bottom);

                    var errorMessage = $"Fail test... ValueKinds=>{kinds}";

                    Assert.AreEqual(left, offset.Left);
                    Assert.AreEqual(right, offset.Right);
                    Assert.AreEqual(top, offset.Top);
                    Assert.AreEqual(bottom, offset.Bottom);
                }
            }
            Debug.Log($"Success to All pattern!!");
        }
        #endregion

        #region SetHorizontalOffsets
        /// <summary>
        /// <seealso cref="LayoutOffset.SetHorizontalOffsets(float left, float right)"/>
        /// </summary>
        [Test]
        public void SetHorizontalOffsetsPrpertyPasses()
        {
            var offset = new LayoutOffset();

            var left = 20;
            var right = 40f;
            offset.SetHorizontalOffsets(left, right);

            Assert.AreEqual(left, offset.Left);
            Assert.AreEqual(right, offset.Right);
        }

        /// <summary>
		/// <seealso cref="LayoutOffset.SetHorizontalOffsets(float left, float right)"/>
		/// <seealso cref="LayoutOffset.OnChangedValue"/>
		/// </summary>
        [Test]
        public void OnChangedValueInSetHorizontalOffsetsPrpertyPasses()
        {
            var offset = new LayoutOffset();
            int callCounter = 0;
            (LayoutOffset self, LayoutOffset.ValueKind kinds) recievedData = default;
            offset.OnChangedValue.Add((_self, _kind) => {
                callCounter++;
                recievedData = (_self, _kind);
            });

            {
                offset.SetHorizontalOffsets(10, 20);

                Assert.AreEqual(1, callCounter, $"コールバックは一度だけ呼び出されるようにしてください");
                Assert.AreSame(offset, recievedData.self);
                Assert.AreEqual(LayoutOffset.ValueKind.Left | LayoutOffset.ValueKind.Right, recievedData.kinds);
            }

            {//同じ値の時は変更されないので、OnChangedValueのkindsには含めないようにする
                var kindsCombination = IndexCombinationEnumerable.GetFlagEnumCombination(
                    LayoutOffset.ValueKind.Left,
                    LayoutOffset.ValueKind.Right
                );
                foreach (var kinds in kindsCombination)
                {
                    var left = (0 != (kinds & LayoutOffset.ValueKind.Left)) ? offset.Left + 10f : offset.Left;
                    var right = (0 != (kinds & LayoutOffset.ValueKind.Right)) ? offset.Right + 10f : offset.Right;
                    callCounter = 0;
                    offset.SetHorizontalOffsets(left, right);

                    var errorMessage = $"Fail test... ValueKinds=>{kinds}";

                    Assert.AreEqual(1, callCounter, errorMessage);
                    Assert.AreSame(offset, recievedData.self, errorMessage);
                    Assert.AreEqual(kinds, recievedData.kinds, errorMessage);
                }
            }
            Debug.Log($"Success to All pattern!!");
        }

        /// <summary>
        /// <seealso cref="LayoutOffset.SetHorizontalOffsets(float left, float right)"/>
        /// <seealso cref="LayoutOffset.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValueInSetHorizontalOffsetsPrpertyWhenThrowExceptionPasses()
        {
            var offset = new LayoutOffset();
            offset.OnChangedValue.Add((_self, _kind) => {
                throw new System.Exception();
            });

            {
                var left = 20;
                var right = 40f;
                offset.SetHorizontalOffsets(left, right);

                Assert.AreEqual(left, offset.Left);
                Assert.AreEqual(right, offset.Right);
            }

            {//同じ値の時は変更されないので、OnChangedValueのkindsには含めないようにする
                var kindsCombination = IndexCombinationEnumerable.GetFlagEnumCombination(
                    LayoutOffset.ValueKind.Left,
                    LayoutOffset.ValueKind.Right
                );
                foreach (var kinds in kindsCombination)
                {
                    var left = (0 != (kinds & LayoutOffset.ValueKind.Left)) ? offset.Left + 10f : offset.Left;
                    var right = (0 != (kinds & LayoutOffset.ValueKind.Right)) ? offset.Right + 10f : offset.Right;
                    offset.SetHorizontalOffsets(left, right);

                    var errorMessage = $"Fail test... ValueKinds=>{kinds}";

                    Assert.AreEqual(left, offset.Left);
                    Assert.AreEqual(right, offset.Right);
                }
            }
            Debug.Log($"Success to All pattern!!");
        }
        #endregion

        #region SetVerticalOffsets
        /// <summary>
        /// <seealso cref="LayoutOffset.SetVerticalOffsets(float top, float bottom)"/>
        /// </summary>
        [Test]
        public void SetVerticalOffsetsPrpertyPasses()
        {
            var offset = new LayoutOffset();

            var top = 10f;
            var bottom = 30f;
            offset.SetVerticalOffsets(top, bottom);

            Assert.AreEqual(top, offset.Top);
            Assert.AreEqual(bottom, offset.Bottom);
        }

        /// <summary>
		/// <seealso cref="LayoutOffset.SetVerticalOffsets(float top, float bottom)"/>
		/// <seealso cref="LayoutOffset.OnChangedValue"/>
		/// </summary>
        [Test]
        public void OnChangedValueInSetVerticalOffsetsPrpertyPasses()
        {
            var offset = new LayoutOffset();
            int callCounter = 0;
            (LayoutOffset self, LayoutOffset.ValueKind kinds) recievedData = default;
            offset.OnChangedValue.Add((_self, _kind) => {
                callCounter++;
                recievedData = (_self, _kind);
            });

            offset.SetVerticalOffsets(30, 40);

            Assert.AreEqual(1, callCounter, $"コールバックは一度だけ呼び出されるようにしてください");
            Assert.AreSame(offset, recievedData.self);
            Assert.AreEqual(LayoutOffset.ValueKind.Top | LayoutOffset.ValueKind.Bottom, recievedData.kinds);

            {//同じ値の時は変更されないので、OnChangedValueのkindsには含めないようにする
                var kindsCombination = IndexCombinationEnumerable.GetFlagEnumCombination(
                    LayoutOffset.ValueKind.Top,
                    LayoutOffset.ValueKind.Bottom
                );
                foreach (var kinds in kindsCombination)
                {
                    var top = (0 != (kinds & LayoutOffset.ValueKind.Top)) ? offset.Top + 10f : offset.Top;
                    var bottom = (0 != (kinds & LayoutOffset.ValueKind.Bottom)) ? offset.Bottom + 10f : offset.Bottom;
                    callCounter = 0;
                    offset.SetVerticalOffsets(top, bottom);

                    var errorMessage = $"Fail test... ValueKinds=>{kinds}";

                    Assert.AreEqual(1, callCounter, errorMessage);
                    Assert.AreSame(offset, recievedData.self, errorMessage);
                    Assert.AreEqual(kinds, recievedData.kinds, errorMessage);
                }
            }
            Debug.Log($"Success to All pattern!!");
        }

        /// <summary>
        /// <seealso cref="LayoutOffset.SetVerticalOffsets(float top, float bottom)"/>
        /// <seealso cref="LayoutOffset.OnChangedValue"/>
        /// </summary>
        [Test]
        public void OnChangedValueInSetVerticalOffsetsPrpertyWhenThrowExceptionPasses()
        {
            var offset = new LayoutOffset();
            offset.OnChangedValue.Add((_self, _kind) => {
                throw new System.Exception();
            });

            {
                var top = 10f;
                var bottom = 30f;
                offset.SetVerticalOffsets(top, bottom);

                Assert.AreEqual(top, offset.Top);
                Assert.AreEqual(bottom, offset.Bottom);
            }

            {//同じ値の時は変更されないので、OnChangedValueのkindsには含めないようにする
                var kindsCombination = IndexCombinationEnumerable.GetFlagEnumCombination(
                    LayoutOffset.ValueKind.Top,
                    LayoutOffset.ValueKind.Bottom
                );
                foreach (var kinds in kindsCombination)
                {
                    var top = (0 != (kinds & LayoutOffset.ValueKind.Top)) ? offset.Top + 10f : offset.Top;
                    var bottom = (0 != (kinds & LayoutOffset.ValueKind.Bottom)) ? offset.Bottom + 10f : offset.Bottom;
                    offset.SetVerticalOffsets(top, bottom);

                    var errorMessage = $"Fail test... ValueKinds=>{kinds}";

                    Assert.AreEqual(top, offset.Top);
                    Assert.AreEqual(bottom, offset.Bottom);
                }
            }
            Debug.Log($"Success to All pattern!!");
        }
        #endregion

    }
}

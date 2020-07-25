﻿using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using Hinode.Tests;

namespace Hinode.Layouts.Tests
{
    /// <summary>
    /// <seealso cref="ILayoutTarget"/>
    /// <seealso cref="ILayoutTargetExtensions"/>
    /// </summary>
    public class TestILayoutTarget
    {
        readonly float EPSILON = LayoutDefines.NUMBER_PRECISION;

        class SetLocalSizeABTestParam : IABTest
        {
            public Vector3 LocalSize { get; set; }
            public Vector3 AnchorMin { get; set; }
            public Vector3 AnchorMax { get; set; }
            public Vector3 OffsetMin { get; set; }
            public Vector3 OffsetMax { get; set; }

            protected override (string name, string paramText)[] GetParamTexts()
            {
                return new (string name, string paramText)[]
                {
                    ("LocalSize", LocalSize.ToString("F4")),
                    ("AnchorMin", AnchorMin.ToString("F4")),
                    ("AnchorMax", AnchorMax.ToString("F4")),
                    ("OffsetMin", OffsetMin.ToString("F4")),
                    ("OffsetMin", OffsetMax.ToString("F4")),
                };
            }

            protected override void InitParams(System.Random rnd)
            {
                var range = 1000000f;
                var anchorRange = 10f;
                LocalSize = new Vector3(
                    rnd.Range(-range, range),
                    rnd.Range(-range, range),
                    rnd.Range(-range, range)
                );

                AnchorMin = new Vector3(
                    rnd.Range(-anchorRange, anchorRange),
                    rnd.Range(-anchorRange, anchorRange),
                    rnd.Range(-anchorRange, anchorRange)
                );
                AnchorMax = new Vector3(
                    rnd.Range(-anchorRange, anchorRange),
                    rnd.Range(-anchorRange, anchorRange),
                    rnd.Range(-anchorRange, anchorRange)
                );
                OffsetMin = new Vector3(
                    rnd.Range(-range, range),
                    rnd.Range(-range, range),
                    rnd.Range(-range, range)
                );
                OffsetMax = new Vector3(
                    rnd.Range(-range, range),
                    rnd.Range(-range, range),
                    rnd.Range(-range, range)
                );
            }

            protected override void TestMethod()
            {
                var self = new LayoutTargetObject();
                self.UpdateLocalSizeWithAnchorParam(AnchorMin, AnchorMax, OffsetMin, OffsetMax);

                self.SetLocalSize(LocalSize);

                {
                    var message = $"Detect Invalid LocalSize!! => result={self.LocalSize}";
                    if (LocalSize.x < 0) Assert.IsTrue(0 <= self.LocalSize.x, message);
                    if (LocalSize.y < 0) Assert.IsTrue(0 <= self.LocalSize.y, message);
                    if (LocalSize.z < 0) Assert.IsTrue(0 <= self.LocalSize.z, message);
                }
                {
                    var anchorAreaSize = self.AnchorAreaSize();
                    var message = $"Detect Invalid AnchorAreaSize!! => result={anchorAreaSize}";
                    Assert.IsTrue(0 <= anchorAreaSize.x, message);
                    Assert.IsTrue(0 <= anchorAreaSize.y, message);
                    Assert.IsTrue(0 <= anchorAreaSize.z, message);
                }
                {
                    var (localAreaMin, localAreaMax) = self.LocalAreaMinMaxPos();
                    var message = $"Detect Invalid LocalAreaMinMaxPos!! => result=({localAreaMin}, {localAreaMax})";
                    Assert.IsTrue(localAreaMin.x <= localAreaMax.x, message);
                    Assert.IsTrue(localAreaMin.y <= localAreaMax.y, message);
                    Assert.IsTrue(localAreaMin.z <= localAreaMax.z, message);
                }
            }

        }

        /// <summary>
		/// A/Bテストの実験コード
		/// TODO 失敗したデータを集計して記録するようにする機能の追加
		/// </summary>
        [Test]
        public void ABTestSetLocalSize()
        {
            var settings = TestSettings.CreateOrGet();
            var test = new SetLocalSizeABTestParam();
            test.RunTest(settings);
        }

        #region ParentLocalSize
        /// <summary>
        /// <seealso cref="ILayoutTargetExtensions.ParentLocalSize(ILayoutTarget)"/>
        /// </summary>
        [Test, Description("テスト用にLayoutTargetObjectを使用しています。")]
        public void ParentLocalSizePasses()
        {
            var self = new LayoutTargetObject();
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(1, 2, 3));

            self.SetParent(parent);
            AssertionUtils.AreNearlyEqual(parent.LocalSize, self.ParentLocalSize());
        }

        /// <summary>
        /// <seealso cref="ILayoutTargetExtensions.ParentLocalSize(ILayoutTarget)"/>
        /// </summary>
        [Test, Description("テスト用にLayoutTargetObjectを使用しています。")]
        public void ParentLocalSizeWhenNullParentPasses()
        {
            var self = new LayoutTargetObject();
            AssertionUtils.AreNearlyEqual(Vector3.zero, self.ParentLocalSize());
        }
        #endregion

        #region AnchorAreaSize
        /// <summary>
		/// <seealso cref="ILayoutTargetExtensions.AnchorAreaSize(ILayoutTarget)"/>
		/// </summary>
        [Test, Description("AnchorMode == Areaの時のテスト")]
        public void AnchorAreaSizeWhenAreaModePasses()
        {
            var testData = new (Vector3 correct, Vector3 anchorMin, Vector3 anchorMax, Vector3 parentSize)[]
            {
                (new Vector3(1, 2, 3), Vector3.zero, Vector3.one, new Vector3(1, 2, 3)),
                (new Vector3(0, 0, 0), Vector3.zero, Vector3.one, new Vector3(-10, -200, -300)),
                (new Vector3(10, 8, 6), new Vector3(0, 0.1f, 0.2f), new Vector3(1, 0.9f, 0.8f), new Vector3(10, 10, 10)),
            };

            foreach(var data in testData)
            {
                var errorMessage = $"Fail test... testData=>{data}";
                var self = new LayoutTargetObject();

                var parent = new LayoutTargetObject();
                parent.SetLocalSize(data.parentSize);
                self.SetParent(parent);
                self.UpdateLocalSizeWithAnchorParam(data.anchorMin, data.anchorMax, Vector3.zero, Vector3.zero);
                Assert.AreEqual(LayoutTargetAnchorMode.Area, self.AnchorMode(), $"AnchorMode Must be Area!! {errorMessage}");
                AssertionUtils.AreNearlyEqual(data.correct, self.AnchorAreaSize(), EPSILON, errorMessage);
            }
        }

        /// <summary>
        /// <seealso cref="ILayoutTargetExtensions.AnchorAreaSize(ILayoutTarget)"/>
        /// </summary>
        [Test, Description("AnchorMode == Pointの時のテスト")]
        public void AnchorAreaSizeWhenPointModePasses()
        {
            var testData = new (Vector3 correct, Vector3 anchorPoint, Vector3 parentSize)[]
            {
                (Vector3.zero, Vector3.zero, new Vector3(1, 2, 3)),
                (Vector3.zero, Vector3.one * 0.5f, new Vector3(-10, -200, -300)),
                (Vector3.zero, new Vector3(0, 0.1f, 0.2f), new Vector3(10, 10, 10)),
            };

            foreach (var data in testData)
            {
                var errorMessage = $"Fail test... testData=>{data}";
                var self = new LayoutTargetObject();

                var parent = new LayoutTargetObject();
                parent.SetLocalSize(data.parentSize);
                self.SetParent(parent);
                self.UpdateLocalSizeWithAnchorParam(data.anchorPoint, data.anchorPoint, Vector3.zero, Vector3.zero);
                Assert.AreEqual(LayoutTargetAnchorMode.Point, self.AnchorMode(), $"AnchorMode Must be Point!! {errorMessage}");
                AssertionUtils.AreNearlyEqual(data.correct, self.AnchorAreaSize(), EPSILON, errorMessage);
            }
        }
        #endregion

        #region AnchorAreaMinMaxPos
        /// <summary>
		/// <seealso cref="ILayoutTargetExtensions.AnchorAreaMinMaxPos(ILayoutTarget)"/>
		/// </summary>
        [Test, Description("AnchorMode == Pointの時のテスト")]
        public void AnchorAreaMinMaxPosWhenPointModePasses()
        {
            var testData = new (Vector3 correctMin, Vector3 correctMax, Vector3 anchorPoint)[]
            {
                (Vector3.zero, Vector3.zero, Vector3.zero),
                (Vector3.zero, Vector3.zero, Vector3.one * 0.5f),
                (Vector3.zero, Vector3.zero, new Vector3(0, 0.1f, 0.2f)),
            };

            foreach (var data in testData)
            {
                var errorMessage = $"Fail test... testData=>{data}";
                var self = new LayoutTargetObject();

                var parent = new LayoutTargetObject();
                parent.SetLocalSize(Vector3.one * 100);
                self.SetParent(parent);

                self.UpdateLocalSizeWithAnchorParam(data.anchorPoint, data.anchorPoint, Vector3.zero, Vector3.zero);
                Assert.AreEqual(LayoutTargetAnchorMode.Point, self.AnchorMode(), $"AnchorMode Must be Point!! {errorMessage}");

                var (min, max) = self.AnchorAreaMinMaxPos();
                AssertionUtils.AreNearlyEqual(data.correctMin, min, EPSILON, errorMessage);
                AssertionUtils.AreNearlyEqual(data.correctMax, max, EPSILON, errorMessage);
            }
        }

        /// <summary>
		/// <seealso cref="ILayoutTargetExtensions.AnchorAreaMinMaxPos(ILayoutTarget)"/>
        /// </summary>
        [Test, Description("AnchorMode == Areaの時のテスト")]
        public void AnchorAreaMinMaxPosWhenAreaModePasses()
        {
            var testData = new (Vector3 correctMin, Vector3 correctMax, Vector3 anchorMin, Vector3 anchorMax, Vector3 parentSize)[]
            {
                (new Vector3(1, 2, 3) * -0.5f, new Vector3(1, 2, 3) * 0.5f, Vector3.zero, Vector3.one, new Vector3(1, 2, 3)),
                (Vector3.zero, Vector3.zero, Vector3.zero, Vector3.one, new Vector3(-10, -200, -300)),
                (new Vector3(10, 10-2f, 10-4f) * -0.5f, new Vector3(10, 10-2f, 10-4f) * 0.5f, new Vector3(0, 0.1f, 0.2f), new Vector3(1, 0.9f, 0.8f), new Vector3(10, 10, 10)),
            };

            foreach (var data in testData)
            {
                var errorMessage = $"Fail test... testData=>{data}";
                var self = new LayoutTargetObject();

                var parent = new LayoutTargetObject();
                parent.SetLocalSize(data.parentSize);
                self.SetParent(parent);
                self.UpdateLocalSizeWithAnchorParam(data.anchorMin, data.anchorMax, Vector3.zero, Vector3.zero);
                Assert.AreEqual(LayoutTargetAnchorMode.Area, self.AnchorMode(), $"AnchorMode Must be Area!! {errorMessage}");

                var (min, max) = self.AnchorAreaMinMaxPos();
                AssertionUtils.AreNearlyEqual(data.correctMin, min, EPSILON, errorMessage);
                AssertionUtils.AreNearlyEqual(data.correctMax, max, EPSILON, errorMessage);
            }
        }
        #endregion

        #region LocalAreaMinMaxPos
        /// <summary>
		/// <seealso cref="ILayoutTargetExtensions.LocalAreaMinMaxPos(ILayoutTarget)"/>
        /// </summary>
        [Test, Description("AnchorMode == Pointの時のテスト")]
        public void LocalAreaMinMaxPosWhenPointModePasses()
        {
            var testData = new (
                string tag,
                Vector3 correctMin,
                Vector3 correctMax,
                Vector3 offsetMin,
                Vector3 offsetMax)[]
            {
                ("Normal Case", new Vector3(-1, -1, -1), new Vector3(2, 2, 2),  Vector3.one * 1, Vector3.one * 2),
                ("Minus OffsetMin Case", new Vector3(1, 1, 1), new Vector3(2, 2, 2), Vector3.one * -1, Vector3.one * 2),
                ("Minus OffsetMax Case", new Vector3(-2, -2, -2), new Vector3(-1, -1, -1), Vector3.one * 2, Vector3.one * -1),
                ("LocalSize Lower than 0 Case", Vector3.zero, Vector3.zero, Vector3.one * -1, Vector3.one * -1),
            };

            foreach (var data in testData)
            {
                var errorMessage = $"Fail test... testData=>{data}";
                var self = new LayoutTargetObject();

                var parent = new LayoutTargetObject();
                self.SetParent(parent);
                var anchorPoint = Vector3.one * 0.5f;
                self.UpdateLocalSizeWithAnchorParam(anchorPoint, anchorPoint, data.offsetMin, data.offsetMax);
                Assert.AreEqual(LayoutTargetAnchorMode.Point, self.AnchorMode(), $"AnchorMode Must be Point!! {errorMessage}");

                var (min, max) = self.LocalAreaMinMaxPos();
                AssertionUtils.AreNearlyEqual(data.correctMin, min, EPSILON, errorMessage);
                AssertionUtils.AreNearlyEqual(data.correctMax, max, EPSILON, errorMessage);
            }
        }

        /// <summary>
		/// <seealso cref="ILayoutTargetExtensions.LocalAreaMinMaxPos(ILayoutTarget)"/>
        /// </summary>
        [Test, Description("AnchorMode == Areaの時のテスト")]
        public void LocalAreaMinMaxPosWhenAreaModePasses()
        {
            var testData = new (
                string tag,
                Vector3 correctMin,
                Vector3 correctMax,
                Vector3 anchorMin,
                Vector3 anchorMax,
                Vector3 offsetMin,
                Vector3 offsetMax,
                Vector3 parentSize)[]
            {
                ("Plus Offset Case", new Vector3(-3, -5, -7), new Vector3(3, 5, 7), Vector3.zero, Vector3.one, Vector3.one, Vector3.one, new Vector3(4, 8, 12)),
                ("Minus Offset Case", new Vector3(-1, -3, -5), new Vector3(1, 3, 5), Vector3.zero, Vector3.one, Vector3.one * -1, Vector3.one * -1, new Vector3(4, 8, 12)),
                ("Variant AnchorMin/Max for Each Element Case ", new Vector3(-11, -9, -7), new Vector3(12, 10, 8), new Vector3(0, 0.1f, 0.2f), new Vector3(1, 0.9f, 0.8f), Vector3.one, Vector3.one*2, new Vector3(20, 20, 20)),
                ("LocalSize Lower than 0 Case", new Vector3(-2, -2, -2), new Vector3(4, 4, 4), Vector3.zero, Vector3.one, Vector3.one*2, Vector3.one*4, new Vector3(-10, -200, -300)), // Case LocalSize Lower than 0.
            };

            foreach (var data in testData)
            {
                var errorMessage = $"Fail test... testData=>{data}";
                var self = new LayoutTargetObject();

                var parent = new LayoutTargetObject();
                parent.SetLocalSize(data.parentSize);
                self.SetParent(parent);
                self.UpdateLocalSizeWithAnchorParam(data.anchorMin, data.anchorMax, data.offsetMin, data.offsetMax);
                Assert.AreEqual(LayoutTargetAnchorMode.Area, self.AnchorMode(), $"AnchorMode Must be Area!! {errorMessage}");

                var (min, max) = self.LocalAreaMinMaxPos();
                AssertionUtils.AreNearlyEqual(data.correctMin, min, EPSILON, errorMessage);
                AssertionUtils.AreNearlyEqual(data.correctMax, max, EPSILON, errorMessage);
            }
        }
        #endregion

        /// <summary>
        /// <seealso cref="ILayoutTarget"/>
        /// <seealso cref="LayoutTargetAnchorMode"/>
        /// </summary>
        [Test]
        public void AnchorModePasses()
        {
            var testData = new (LayoutTargetAnchorMode correct, Vector3 min, Vector3 max)[]
            {
                (LayoutTargetAnchorMode.Point, Vector3.one * 0.5f, Vector3.one * 0.5f),
                (LayoutTargetAnchorMode.Point, Vector3.one, Vector3.one),
                (LayoutTargetAnchorMode.Point, Vector3.zero, Vector3.zero),
                (LayoutTargetAnchorMode.Point, Vector3.one * 0.25f, Vector3.one * 0.25f),
                (LayoutTargetAnchorMode.Area, Vector3.zero, Vector3.one),
                (LayoutTargetAnchorMode.Area, Vector3.one * 0.23f, Vector3.one * 0.75f),
                (LayoutTargetAnchorMode.Area, Vector3.one * -0.1f, Vector3.one * 2.5f),
                (LayoutTargetAnchorMode.Area, Vector3.one * 0.5f, Vector3.one * (0.5f + LayoutDefines.NUMBER_PRECISION*1.1f)),
            };
            foreach(var data in testData)
            {
                var self = new LayoutTargetObject();
                self.SetAnchor(data.min, data.max);
                var errorMessage = $"Fail test... testData={data}";
                Assert.AreEqual(data.correct, self.AnchorMode(), errorMessage);
            }
        }

        /// <summary>
		/// <seealso cref="ILayoutTargetExtensions.SetLocalSize(ILayoutTarget, Vector3)"/>
        /// <seealso cref="LayoutTargetObject.LocalSize"/>
        /// <seealso cref="LayoutTargetObject.AnchorMin"/>
        /// <seealso cref="LayoutTargetObject.AnchorMax"/>
        /// <seealso cref="LayoutTargetObject.AnchorOffsetMin"/>
        /// <seealso cref="LayoutTargetObject.AnchorOffsetMax"/>
        /// </summary>
        [Test, Description("テスト用にLayoutTargetObjectを使用しています。")]
        public void SetLocalSizePasses()
        {
            System.Action<LayoutTargetObject, LayoutTargetObject, Vector3> validateFunc = (_self, _parent, _localSize) =>
            {
                var anchorMin = _self.AnchorMin;
                var anchorMax = _self.AnchorMax;

                var anchorLocalSize = _parent.LocalSize.Mul(_self.AnchorMax - _self.AnchorMin);
                var anchorMinPos = -(anchorLocalSize * 0.5f + _self.AnchorOffsetMin);
                var anchorMaxPos = anchorLocalSize * 0.5f + _self.AnchorOffsetMax;
                var anchorCenterPos = (anchorMaxPos + anchorMinPos) * 0.5f;

                var localMinPos = anchorCenterPos - _localSize * 0.5f;
                var localMaxPos = anchorCenterPos + _localSize * 0.5f;

                _self.SetParent(_parent);
                Debug.Log($"test-- anchorMinPos={anchorMinPos} anchorMaxPos={anchorMaxPos} anchorCenterPos={anchorCenterPos}");
                Debug.Log($"test-- localSize={_localSize} localMinPos={localMinPos} localMaxPos={localMaxPos}");
                _self.SetLocalSize(_localSize);

                Vector3 offsetMin, offsetMax;
                switch (_self.AnchorMode())
                {
                    case LayoutTargetAnchorMode.Point:
                        offsetMin = -1 * localMinPos;
                        offsetMax = localMaxPos;
                        break;
                    case LayoutTargetAnchorMode.Area:
                        offsetMin = -1 * (localMinPos + anchorLocalSize * 0.5f);
                        offsetMax = (localMaxPos - anchorLocalSize * 0.5f);
                        break;
                    default:
                        throw new System.NotImplementedException();
                }
                AssertionUtils.AreNearlyEqual(offsetMin, _self.AnchorOffsetMin, EPSILON);
                AssertionUtils.AreNearlyEqual(offsetMax, _self.AnchorOffsetMax, EPSILON);
                Debug.Log($"Success to Caluculate anchorOffsetMin/Max! offsetMin={offsetMin}, offsetMax{offsetMax}");

                AssertionUtils.AreNearlyEqual(_localSize, _self.LocalSize, EPSILON);
                AssertionUtils.AreNearlyEqual(anchorMin, _self.AnchorMin, EPSILON);
                AssertionUtils.AreNearlyEqual(anchorMax, _self.AnchorMax, EPSILON);
                Debug.Log($"Success not to Change Values(anchorMin/Max localSize)!!");
            };

            {
                var self = new LayoutTargetObject();
                var anchorPos = Vector3.one * 0.5f;
                self.UpdateLocalSizeWithAnchorParam(anchorPos, anchorPos, Vector3.one * 10f, Vector3.one * 20f);

                var parent = new LayoutTargetObject();
                parent.SetLocalSize(new Vector3(100, 100, 100));

                var localSize = Vector3.one * 10f;
                validateFunc(self, parent, localSize);
            }
            Debug.Log($"Success to Anchor Point Mode!");

            {
                var self = new LayoutTargetObject();
                self.UpdateLocalSizeWithAnchorParam(Vector3.zero, Vector3.one, Vector3.one * 10f, Vector3.one * 20f);

                var parent = new LayoutTargetObject();
                parent.SetLocalSize(new Vector3(100, 100, 100));

                var localSize = Vector3.one * 10f;
                validateFunc(self, parent, localSize);
            }
            Debug.Log($"Success to Anchor Area Mode!");
        }
    }
}

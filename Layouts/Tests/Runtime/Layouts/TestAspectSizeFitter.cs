using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using Hinode.Tests;
using static System.Math;

namespace Hinode.Layouts.Tests
{
    /// <summary>
	/// <seealso cref="AspectSizeFitter"/>
	/// </summary>
    public class TestAspectSizeFitter
    {
        #region OperationTargetFlags
        /// <summary>
        /// <seealso cref="AspectSizeFitter.OperationTargetFlags"/>
        /// </summary>
        [Test]
        public void OperationTargetFlagsPasses()
        {
            var aspectLayout = new AspectSizeFitter();
            Assert.AreEqual(LayoutOperationTarget.Self_LocalSize | LayoutOperationTarget.Self_Offset, aspectLayout.OperationTargetFlags);
        }
        #endregion

        #region
        /// <summary>
        /// <seealso cref="AspectSizeFitter.OperationPriority"/>
        /// <seealso cref="AspectSizeFitter.DefaultOperationPriority"/>
        /// </summary>
        [Test]
        public void OperaionPriorityPasses()
        {
            var aspectLayout = new AspectSizeFitter();
            Assert.AreEqual(AspectSizeFitter.DefaultOperationPriority, aspectLayout.OperationPriority);
        }
        #endregion

        #region AspectRatio Property
        /// <summary>
        /// <seealso cref="AspectSizeFitter.AspectRatio"/>
        /// </summary>
        [Test]
        public void AspectRatioPropertyPasses()
        {
            var aspectLayout = new AspectSizeFitter();
            var rnd = new System.Random();
            for(var i = 0; i<1000; ++i)
            {
                var prevRatio = aspectLayout.AspectRatio;
                var ratio = rnd.Range(-10, 1000);
                aspectLayout.AspectRatio = ratio;

                var errorMessage = $"Fail test... ratio={aspectLayout.AspectRatio}, set ratio={ratio}, prevRatio={prevRatio}";
                Assert.IsTrue(LayoutDefines.NUMBER_PRECISION <= aspectLayout.AspectRatio, errorMessage);
            }
        }
        #endregion

        #region FixedLength Property
        /// <summary>
        /// <seealso cref="AspectSizeFitter.FixedLength"/>
        /// </summary>
        [Test]
        public void FixedLengthPropertyPasses()
        {
            var aspectLayout = new AspectSizeFitter();
            var rnd = new System.Random();
            for (var i = 0; i < 1000; ++i)
            {
                var prev = aspectLayout.AspectRatio;
                var length = rnd.Range(-10, 1000);
                aspectLayout.FixedLength = length;

                var errorMessage = $"Fail test... ratio={aspectLayout.FixedLength}, set length={length}, prevLength={prev}";
                Assert.IsTrue(0f <= aspectLayout.AspectRatio, errorMessage);
            }
        }
        #endregion

        #region DoChanged Callback
        /// <summary>
        /// <seealso cref="AspectSizeFitter.DoChanged"/>
        /// </summary>
        [Test, Order(-100)]
        public void DoChanged_WhenChangeTargetPasses()
        {
            var target = new LayoutTargetObject();
            target.UpdateLocalSize(Vector3.one * 10, Vector3.zero);

            var aspectLayout = new AspectSizeFitter();
            Assert.IsFalse(aspectLayout.DoChanged);

            {
                aspectLayout.UpdateLayout();

                aspectLayout.Target = target;
                Assert.IsTrue(aspectLayout.DoChanged);
            }
            Debug.Log($"Success to Change DoChanged When Change Target!(part1)");

            {
                aspectLayout.UpdateLayout();

                aspectLayout.Target = null;
                Assert.IsTrue(aspectLayout.DoChanged);
            }
            Debug.Log($"Success to Change DoChanged When Change Target!(part2)");

            {
                aspectLayout.Target = target;
                aspectLayout.UpdateLayout();

                aspectLayout.Target = target;
                Assert.IsFalse(aspectLayout.DoChanged);
            }
            Debug.Log($"Success to Change DoChanged When not Change Target!");

            {
                aspectLayout.Target = target;
                aspectLayout.UpdateLayout();

                target.Dispose(); // test point
                Assert.IsTrue(aspectLayout.DoChanged);
            }
            Debug.Log($"Success to Change DoChanged When Target Dispose!");
        }

        /// <summary>
        /// <seealso cref="AspectSizeFitter.DoChanged"/>
        /// <seealso cref="AspectSizeFitter.UpdateLayout()"/>
        /// </summary>
        [Test, Order(-100)]
        public void DoChanged_AfterUpdateLayoutPasses()
        {
            var target = new LayoutTargetObject();
            target.UpdateLocalSize(Vector3.one * 10, Vector3.zero);

            var aspectLayout = new AspectSizeFitter();
            Assert.IsFalse(aspectLayout.DoChanged);

            aspectLayout.Target = target;
            Assert.IsTrue(aspectLayout.DoChanged);

            aspectLayout.UpdateLayout();
            Assert.IsFalse(aspectLayout.DoChanged);
        }

        /// <summary>
        /// <seealso cref="AspectSizeFitter.DoChanged"/>
        /// </summary>
        [Test]
        public void DoChanged_WhenChangeModePasses()
        {
            var target = new LayoutTargetObject();
            target.UpdateLocalSize(Vector3.one * 10, Vector3.zero);

            var aspectLayout = new AspectSizeFitter();
            Assert.IsFalse(aspectLayout.DoChanged);

            var rnd = new System.Random();
            var modeValues = System.Enum.GetValues(typeof(AspectSizeFitter.Mode));
            for(var i=0; i<100; ++i)
            {
                aspectLayout.UpdateLayout();

                var mode = (AspectSizeFitter.Mode)rnd.Next(modeValues.Length);
                var prevMode = aspectLayout.CurrentMode;
                var isDifferMode = mode != prevMode;
                aspectLayout.CurrentMode = mode;

                Assert.AreEqual(isDifferMode, aspectLayout.DoChanged, $"Fail test... change Mode({mode}), Prev Mode({prevMode})");
            }
        }

        /// <summary>
        /// <seealso cref="AspectSizeFitter.DoChanged"/>
        /// </summary>
        [Test]
        public void DoChanged_WhenChangeParentOfTargetPasses()
        {
            var target = new LayoutTargetObject();
            target.UpdateLocalSize(Vector3.one * 10, Vector3.zero);
            var parent = new LayoutTargetObject();
            parent.UpdateLocalSize(Vector3.one * 100f, Vector3.zero);

            var aspectLayout = new AspectSizeFitter();
            aspectLayout.Target = target;

            {
                aspectLayout.UpdateLayout();

                target.SetParent(parent);// test point
                Assert.IsTrue(aspectLayout.DoChanged);
            }
            Debug.Log($"Success to Update DoChanged when Change Parent of Target!(part1)");

            {
                aspectLayout.UpdateLayout();

                target.SetParent(null); // test point
                Assert.IsTrue(aspectLayout.DoChanged);
            }
            Debug.Log($"Success to Update DoChanged when Change Parent of Target!(part2)");

            {
                target.SetParent(parent);
                aspectLayout.UpdateLayout();

                target.SetParent(parent); // test point
                Assert.IsFalse(aspectLayout.DoChanged);
            }
            Debug.Log($"Success to Update DoChanged when Not Change Parent of Target!");

            {
                aspectLayout.UpdateLayout();

                parent.Dispose(); // test point
                Assert.IsTrue(aspectLayout.DoChanged);
            }
            Debug.Log($"Success to Update DoChanged when A Parent Of Target Dispose!");
        }

        /// <summary>
        /// <seealso cref="AspectSizeFitter.DoChanged"/>
        /// <seealso cref="AspectSizeFitter.AspectRatio"/>
        /// </summary>
        [Test]
        public void DoChanged_WhenChangeAspectRatioPasses()
        {
            var aspectLayout = new AspectSizeFitter();
            Assert.IsFalse(aspectLayout.DoChanged);

            {
                aspectLayout.AspectRatio = aspectLayout.AspectRatio + 1f;
                Assert.IsTrue(aspectLayout.DoChanged);
            }
            Debug.Log($"Success to Change DoChanged when Change AspectRatio!");

            {
                aspectLayout.UpdateLayout();
                Assert.IsFalse(aspectLayout.DoChanged);

                aspectLayout.AspectRatio = aspectLayout.AspectRatio;
                Assert.IsFalse(aspectLayout.DoChanged);
            }
            Debug.Log($"Success to not Change DoChanged when the Same Value set to AspectRatio!");
        }

        /// <summary>
        /// <seealso cref="AspectSizeFitter.DoChanged"/>
        /// <seealso cref="AspectSizeFitter.CurrentMode/>
        /// <seealso cref="AspectSizeFitter.FixedLength"/>
        /// </summary>
        [Test]
        public void DoChanged_WhenChangeFixedLengthPasses()
        {
            var aspectLayout = new AspectSizeFitter();
            Assert.IsFalse(aspectLayout.DoChanged);

            var fixedModes = new AspectSizeFitter.Mode[]
            {
                AspectSizeFitter.Mode.FixedWidth,
                AspectSizeFitter.Mode.FixedHeight,
            };

            foreach(var fixedMode in fixedModes)
            {
                var errorMessage = $"Fail test... fixedMode={fixedMode}";
                aspectLayout.CurrentMode = fixedMode;
                {
                    aspectLayout.UpdateLayout();
                    Assert.IsFalse(aspectLayout.DoChanged);

                    aspectLayout.FixedLength += 1f;
                    Assert.IsTrue(aspectLayout.DoChanged, errorMessage);
                }
                Debug.Log($"Success to Change DoChanged when Change FixedLength!");

                {
                    aspectLayout.UpdateLayout();
                    Assert.IsFalse(aspectLayout.DoChanged);

                    aspectLayout.FixedLength = aspectLayout.FixedLength;
                    Assert.IsFalse(aspectLayout.DoChanged, errorMessage);
                }
                Debug.Log($"Success to not Change DoChanged when the Same Value Set to FixedLength!");
            }
        }

        /// <summary>
        /// <seealso cref="AspectSizeFitter.DoChanged"/>
        /// <seealso cref="AspectSizeFitter.CurrentMode/>
        /// <seealso cref="AspectSizeFitter.FixedLength"/>
        /// </summary>
        [Test]
        public void DoChanged_WhenChangeFixedLengthAndNotFixedLengthModePasses()
        {
            var aspectLayout = new AspectSizeFitter();
            Assert.IsFalse(aspectLayout.DoChanged);

            aspectLayout.CurrentMode = AspectSizeFitter.Mode.ParentFit;
            {
                aspectLayout.UpdateLayout();
                Assert.IsFalse(aspectLayout.DoChanged);

                aspectLayout.FixedLength = aspectLayout.FixedLength + 1f;
                Assert.IsFalse(aspectLayout.DoChanged);
            }
            Debug.Log($"Success to Change DoChanged when Change FixedLength!");

            {
                aspectLayout.UpdateLayout();
                Assert.IsFalse(aspectLayout.DoChanged);

                aspectLayout.FixedLength = aspectLayout.FixedLength;
                Assert.IsFalse(aspectLayout.DoChanged);
            }
            Debug.Log($"Success to not Change DoChanged when the Same Value Set to FixedLength!");
        }


        /// <summary>
        /// <seealso cref="AspectSizeFitter.DoChanged"/>
        /// <seealso cref="AspectSizeFitter.Padding"/>
        /// </summary>
        [Test]
        public void DoChanged_WhenChangeOffsetPasses()
        {
            var aspectLayout = new AspectSizeFitter();
            Assert.IsFalse(aspectLayout.DoChanged);

            {
                aspectLayout.UpdateLayout();
                Assert.IsFalse(aspectLayout.DoChanged);

                aspectLayout.Padding.Left = aspectLayout.Padding.Left + 1f;
                Assert.IsTrue(aspectLayout.DoChanged);
            }
            Debug.Log($"Success to Change DoChanged when Change Offset!");

            {
                aspectLayout.UpdateLayout();
                Assert.IsFalse(aspectLayout.DoChanged);

                aspectLayout.Padding.Left = aspectLayout.Padding.Left;
                Assert.IsFalse(aspectLayout.DoChanged);
            }
            Debug.Log($"Success to not Change DoChanged when the Same Value set to Offset!");
        }

        /// <summary>
        /// <seealso cref="AspectSizeFitter.DoChanged"/>
        /// <seealso cref="AspectSizeFitter.Target"/>
        /// </summary>
        [Test]
        public void DoChanged_WhenChangeParentLocalSize()
        {
            var aspectLayout = new AspectSizeFitter();
            Assert.IsFalse(aspectLayout.DoChanged);

            var target = new LayoutTargetObject();
            target.UpdateLocalSize(Vector3.one * 10, Vector3.zero);
            var parent = new LayoutTargetObject();
            parent.UpdateLocalSize(Vector3.one * 100f, Vector3.zero);
            target.SetParent(parent);

            aspectLayout.Target = target;

            {
                aspectLayout.UpdateLayout();
                Assert.IsFalse(aspectLayout.DoChanged);

                parent.SetLocalSize(parent.LocalSize + Vector3.one * 10f);
                Assert.IsTrue(aspectLayout.DoChanged);
            }
            Debug.Log($"Success to Change DoChanged when Change LocalSize Of Parent Of Target!");
        }

        /// <summary>
        /// <seealso cref="AspectSizeFitter.DoChanged"/>
        /// <seealso cref="AspectSizeFitter.Target"/>
        /// </summary>
        [Test]
        public void DoChanged_WhenChangeTargetLocalSizeOrOffsetPasses()
        {
            var aspectLayout = new AspectSizeFitter();
            Assert.IsFalse(aspectLayout.DoChanged);

            var target = new LayoutTargetObject();
            target.UpdateLocalSize(Vector3.one * 10, Vector3.zero);

            aspectLayout.Target = target;

            {
                aspectLayout.UpdateLayout();
                Assert.IsFalse(aspectLayout.DoChanged);

                target.SetLocalSize(target.LocalSize + Vector3.one * 10f);
                Assert.IsTrue(aspectLayout.DoChanged);
            }
            Debug.Log($"Success to Change DoChanged when Change LocalSize Of Target!");

            {
                aspectLayout.UpdateLayout();
                Assert.IsFalse(aspectLayout.DoChanged);

                target.SetOffset(target.Offset + Vector3.one * 10f);
                Assert.IsTrue(aspectLayout.DoChanged);
            }
            Debug.Log($"Success to Change DoChanged when Change Offset Of Target!");
        }

        /// <summary>
        /// <seealso cref="AspectSizeFitter.DoChanged"/>
        /// <seealso cref="AspectSizeFitter.Target"/>
        /// </summary>
        [Test]
        public void DoChanged_WhenChangeTargetAnchorMinMaxPasses()
        {
            var aspectLayout = new AspectSizeFitter();
            Assert.IsFalse(aspectLayout.DoChanged);

            var target = new LayoutTargetObject();
            target.UpdateLocalSize(Vector3.one * 10, Vector3.zero);

            aspectLayout.Target = target;

            {
                aspectLayout.UpdateLayout();
                Assert.IsFalse(aspectLayout.DoChanged);

                target.SetAnchor(target.AnchorMin + Vector3.one * 0.1f, target.AnchorMax);
                Assert.IsTrue(aspectLayout.DoChanged);
            }
            Debug.Log($"Success to Change DoChanged when Change AnchorMin Of Target!");

            {
                aspectLayout.UpdateLayout();
                Assert.IsFalse(aspectLayout.DoChanged);

                target.SetAnchor(target.AnchorMin, target.AnchorMax + Vector3.one * 0.1f);
                Assert.IsTrue(aspectLayout.DoChanged);
            }
            Debug.Log($"Success to Change DoChanged when Change AnchorMax Of Target!");
        }
        #endregion

        #region AspectSizeFitter.UpdateLayout
        abstract class UpdateLayout_ABTestBase : IABTest
        {
            protected abstract void Validate();

            enum UseParamKind
            {
                UseParent,
                ParentLocalSize,
                AnchorMinMax,
                LocalSize,
                Offset,
                AspectRatio,
                LayoutOffset,
                FixedLength,
            }

            LayoutTargetObject _parent;
            AspectSizeFitter _aspectLayout;

            IABTest.UseParam<bool> _useParentParam = new UseParam<bool>("UseParant?", true);
            IABTest.UseParam<Vector3> _parentLocalSizeParam = new UseParam<Vector3>("ParentLocalSize", Vector3.one * 100f);
            IABTest.UseParam<(Vector3 min, Vector3 max)> _anchorMinMaxParam = new UseParam<(Vector3 min, Vector3 max)>("AnchorMin/Max", (Vector3.one * 0.5f, Vector3.one * 0.5f));
            IABTest.UseParam<Vector3> _localSizeParam = new UseParam<Vector3>("LocalSize", Vector3.one * 10f);
            IABTest.UseParam<Vector3> _offsetParam = new UseParam<Vector3>("Offset", Vector3.zero); 

            IABTest.UseParam<float> _aspectRatioParam = new UseParam<float>("AspectRatio", 1f);
            IABTest.UseParam<LayoutOffset> _layoutOffsetParam = new UseParam<LayoutOffset>("LayoutOffset", new LayoutOffset());
            IABTest.UseParam<float> _fixedLengthParam = new UseParam<float>("FixedLength", 100);

            protected LayoutTargetObject Parent { get => _parent; }
            protected AspectSizeFitter AspectLayout { get => _aspectLayout; }

            protected IABTest.UseParam<bool> UseParentParam { get => _useParentParam; }
            protected IABTest.UseParam<Vector3> ParentLocalSizeParam { get => _parentLocalSizeParam; }
            protected IABTest.UseParam<(Vector3 min, Vector3 max)> AnchorMinMaxParam { get => _anchorMinMaxParam; }
            protected IABTest.UseParam<Vector3> LocalSizeParam { get => _localSizeParam; }
            protected IABTest.UseParam<Vector3> OffsetParam { get => _offsetParam; }

            protected IABTest.UseParam<float> AspectRatioParam { get => _aspectRatioParam; }
            protected IABTest.UseParam<LayoutOffset> LayoutOffsetParam { get => _layoutOffsetParam; }
            protected IABTest.UseParam<float> FixedLengthParam { get => _fixedLengthParam; }

            public UpdateLayout_ABTestBase()
            {
                _parent = new LayoutTargetObject();
                var target = new LayoutTargetObject();
                _aspectLayout = new AspectSizeFitter();
                _aspectLayout.Target = target;
            }

            protected override (string name, string paramText)[] GetParamTexts()
            {
                return new (string name, string paramText)[]
                {
                    _useParentParam.ToText(),
                    _parentLocalSizeParam.ToText((v) => $"{v:F2}"),
                    _anchorMinMaxParam.ToText((v) => $"{v.min:86},{v.max:F8}"),
                    _localSizeParam.ToText((v) => $"{v:F2}"),
                    _offsetParam.ToText((v) => $"{v:F2}"),
                    _aspectRatioParam.ToText((v) => $"{v:F6}"),
                    _layoutOffsetParam.ToText((v) => {
                        return $"{v.CurrentUnit}:L={v.Left},R={v.Right},T={v.Top},B={v.Bottom}";
                    }),
                    _fixedLengthParam.ToText((v) => $"{v:F2}"),
                };
            }

            readonly static float SizeRange = 1000f;

            protected override void InitParams(System.Random rnd)
            {
                var paramCount = System.Enum.GetValues(typeof(UseParamKind)).Length;
                var useParamIndex = rnd.Next(paramCount);
                switch ((UseParamKind)useParamIndex)
                {
                    case UseParamKind.UseParent:
                        _useParentParam.Value = (rnd.Next() % 2) == 0;
                        break;
                    case UseParamKind.ParentLocalSize:
                        _parentLocalSizeParam.Value = new Vector3(
                            rnd.Range(-100f, SizeRange),
                            rnd.Range(-100f, SizeRange),
                            rnd.Range(-100f, SizeRange)
                        );
                        break;
                    case UseParamKind.AnchorMinMax:
                        _anchorMinMaxParam.Value = (
                            min: new Vector3(
                                rnd.Range(0f, 1f),
                                rnd.Range(0f, 1f),
                                rnd.Range(0f, 1f)
                            ),
                            max: new Vector3(
                                rnd.Range(0f, 1f),
                                rnd.Range(0f, 1f),
                                rnd.Range(0f, 1f)
                            )
                         );
                        break;
                    case UseParamKind.LocalSize:
                        _localSizeParam.Value = new Vector3(
                            rnd.Range(-100f, SizeRange),
                            rnd.Range(-100f, SizeRange),
                            rnd.Range(-100f, SizeRange)
                        );
                        break;
                    case UseParamKind.Offset:
                        var offsetRange = 1000f;
                        _offsetParam.Value = new Vector3(
                            rnd.Range(-offsetRange, offsetRange),
                            rnd.Range(-offsetRange, offsetRange),
                            rnd.Range(-offsetRange, offsetRange)
                        );
                        break;
                    case UseParamKind.AspectRatio:
                        _aspectRatioParam.Value = rnd.Range(0.1f, 10);
                        break;
                    case UseParamKind.LayoutOffset:
                        _layoutOffsetParam.DoUse = true;
                        _layoutOffsetParam.Value.CurrentUnit = (rnd.Next() % 2) == 0L
                            ? LayoutOffset.Unit.Pixel
                            : LayoutOffset.Unit.Ratio;

                        switch(_layoutOffsetParam.Value.CurrentUnit)
                        {
                            case LayoutOffset.Unit.Pixel:
                                var horizontalRange = _aspectLayout.Target.LocalSize.x * 0.75f;
                                var verticalRange = _aspectLayout.Target.LocalSize.y * 0.75f;
                                _layoutOffsetParam.Value.SetOffsets(
                                    rnd.Range(-horizontalRange, horizontalRange),
                                    rnd.Range(-horizontalRange, horizontalRange),
                                    rnd.Range(-verticalRange, verticalRange),
                                    rnd.Range(-verticalRange, verticalRange)
                                );
                                break;
                            case LayoutOffset.Unit.Ratio:
                                _layoutOffsetParam.Value.SetOffsets(
                                    rnd.Range(-0.75f, 0.75f),
                                    rnd.Range(-0.75f, 0.75f),
                                    rnd.Range(-0.75f, 0.75f),
                                    rnd.Range(-0.75f, 0.75f)
                                );
                                break;
                            default:
                                throw new System.NotImplementedException();
                        }
                        break;
                    case UseParamKind.FixedLength:
                        _fixedLengthParam.DoUse = true;
                        var size = _aspectLayout.CurrentMode == AspectSizeFitter.Mode.FixedWidth
                            ? _parent.LocalSize.x
                            : _parent.LocalSize.y;
                        _fixedLengthParam.Value = rnd.Range(-10f, size);
                        break;
                    default:
                        throw new System.NotImplementedException();
                }
            }

            protected override void TestMethod()
            {
                var target = _aspectLayout.Target as LayoutTargetObject;
                target.SetParent(_useParentParam.Value ? _parent : null);
                _parent.SetLocalSize(_parentLocalSizeParam.Value);

                target.SetAnchor(_anchorMinMaxParam.Value.min, _anchorMinMaxParam.Value.max);
                target.UpdateLocalSize(
                    _localSizeParam.Value,
                    _offsetParam.Value);

                _aspectLayout.AspectRatio = _aspectRatioParam.Value;
                _aspectLayout.Padding = _layoutOffsetParam.Value;
                _aspectLayout.FixedLength = _fixedLengthParam.Value;

                _aspectLayout.UpdateLayout();
                //検証処理をかく
                Validate();
            }

            protected (Vector3 size, Vector3 offset) CalSizeAndOffset(Vector3 baseSize, LayoutOffset padding)
            {
                if (AspectLayout.Target.Parent == null)
                {
                    var o = Vector3.zero;
                    o.z = AspectLayout.Target.Offset.z;
                    return (Vector3.zero, o);
                }

                Vector3 size = baseSize;
                Vector3 offset = Vector3.zero;
                switch (padding.CurrentUnit)
                {
                    case LayoutOffset.Unit.Pixel:
                        size.x -= padding.Left + padding.Right;
                        size.y -= padding.Top + padding.Bottom;

                        offset = new Vector3(
                            0.5f * (padding.Left - padding.Right),
                            0.5f * (padding.Bottom - padding.Top),
                            0
                        );
                        break;
                    case LayoutOffset.Unit.Ratio:
                        size.x -= baseSize.x * (padding.Left + padding.Right);
                        size.y -= baseSize.y * (padding.Top + padding.Bottom);

                        offset = new Vector3(
                            0.5f * baseSize.x * (padding.Left - padding.Right),
                            0.5f * baseSize.y * (padding.Bottom - padding.Top),
                            0
                        );
                        break;
                    default:
                        throw new System.NotImplementedException();
                }

                size = Vector3.Max(Vector3.zero, size);
                offset.z = AspectLayout.Target.Offset.z;
                return (size, offset);
            }

            protected Vector3 AdjustSize(Vector3 baseSize, float aspectRatio)
            {
                Vector3 size;
                switch (AspectLayout.CurrentMode)
                {
                    case AspectSizeFitter.Mode.ParentFit:
                    case AspectSizeFitter.Mode.AnchorFit:
                    case AspectSizeFitter.Mode.FixedWidth:
                        size = baseSize;
                        var height = baseSize.x * aspectRatio;
                        if (size.y < height)
                        {
                            size.x = size.y / Max(LayoutDefines.NUMBER_PRECISION, aspectRatio);
                        }
                        else
                        {
                            size.y = height;
                        }

                        if (AspectLayout.CurrentMode == AspectSizeFitter.Mode.FixedWidth)
                        {
                            if (AspectLayout.FixedLength <= size.x && AspectLayout.FixedLength * AspectLayout.AspectRatio <= size.y)
                            {
                                size.x = AspectLayout.FixedLength;
                                size.y = AspectLayout.FixedLength * AspectLayout.AspectRatio;
                            }
                        }
                        break;
                    case AspectSizeFitter.Mode.FixedHeight:
                        size = baseSize;
                        var width = baseSize.y * aspectRatio;
                        if (size.x < width)
                        {
                            size.y = size.x / Max(LayoutDefines.NUMBER_PRECISION, aspectRatio);
                        }
                        else
                        {
                            size.x = width;
                        }

                        if (AspectLayout.FixedLength <= size.y && AspectLayout.FixedLength * AspectLayout.AspectRatio <= size.x)
                        {
                            size.y = AspectLayout.FixedLength;
                            size.x = AspectLayout.FixedLength * AspectLayout.AspectRatio;
                        }
                        break;
                    default:
                        throw new System.NotImplementedException();
                }
                size.z = AspectLayout.Target.LocalSize.z;
                return Vector3.Max(size, Vector3.zero);
            }
        }

        class UpdateLayout_WhenParentFit_ABTestBase : UpdateLayout_ABTestBase
        {
            public UpdateLayout_WhenParentFit_ABTestBase() : base()
            {
                AspectLayout.CurrentMode = AspectSizeFitter.Mode.ParentFit;
            }

            protected override void Validate()
            {
                var (baseSize, offset) = CalSizeAndOffset(Parent.LayoutInfo.GetLayoutSize(Parent), AspectLayout.Padding);
                var size = AdjustSize(baseSize, AspectLayout.AspectRatio);

                var target = AspectLayout.Target;
                AssertionUtils.AreNearlyEqual(size, target.LocalSize, LayoutDefines.NUMBER_PRECISION, $"Fail Test... LocalSize");
                AssertionUtils.AreNearlyEqual(offset, target.Offset, LayoutDefines.NUMBER_PRECISION, $"Fail Test... Offset");
                Assert.IsTrue( baseSize.x >= target.LocalSize.x
                    && baseSize.y >= target.LocalSize.y,
                    $"Test Fail... the LocalSize of Target must Less or Equal BaseSize... baseSize={baseSize}, localSize={target.LocalSize}"
                );
            }
        }

        /// <summary>
        /// <seealso cref="AspectSizeFitter.UpdateLayout()"/>
        /// </summary>
        [Test]
        public void ABTest_UpdateLayout_WhenParentFitModePasses()
        {
            var ABTest = new UpdateLayout_WhenParentFit_ABTestBase();

            var settings = TestSettings.CreateOrGet();
            ABTest.RunTest(settings);
        }

        class UpdateLayout_WhenAnchorFit_ABTestBase : UpdateLayout_ABTestBase
        {
            public UpdateLayout_WhenAnchorFit_ABTestBase() : base()
            {
                AspectLayout.CurrentMode = AspectSizeFitter.Mode.AnchorFit;
            }

            protected override void Validate()
            {
                var target = AspectLayout.Target;
                var layoutSize = Parent.LayoutInfo.GetLayoutSize(Parent)
                    .Mul(target.AnchorMax - target.AnchorMin);

                var (baseSize, offset) = CalSizeAndOffset(layoutSize, AspectLayout.Padding);
                var size = AdjustSize(baseSize, AspectLayout.AspectRatio);

                AssertionUtils.AreNearlyEqual(size, target.LocalSize, LayoutDefines.NUMBER_PRECISION, $"Fail Test... LocalSize");
                AssertionUtils.AreNearlyEqual(offset, target.Offset, LayoutDefines.NUMBER_PRECISION, $"Fail Test... Offset");
                Assert.IsTrue(baseSize.x >= target.LocalSize.x
                    && baseSize.y >= target.LocalSize.y,
                    $"Test Fail... baseSize={baseSize}, localSize={target.LocalSize}"
                );
            }
        }

        /// <summary>
        /// <seealso cref="AspectSizeFitter.UpdateLayout()"/>
        /// </summary>
        [Test]
        public void ABTest_UpdateLayout_WhenAnchorFitModePasses()
        {
            var ABTest = new UpdateLayout_WhenAnchorFit_ABTestBase();

            var settings = TestSettings.CreateOrGet();
            ABTest.RunTest(settings);
        }

        class UpdateLayout_WhenFixedWidth_ABTestBase : UpdateLayout_ABTestBase
        {
            public UpdateLayout_WhenFixedWidth_ABTestBase() : base()
            {
                AspectLayout.CurrentMode = AspectSizeFitter.Mode.FixedWidth;
            }

            protected override void Validate()
            {
                var target = AspectLayout.Target;
                var layoutSize = Parent.LayoutInfo.GetLayoutSize(Parent)
                    .Mul(target.AnchorMax - target.AnchorMin);

                var (baseSize, offset) = CalSizeAndOffset(layoutSize, AspectLayout.Padding);
                var size = AdjustSize(baseSize, AspectLayout.AspectRatio);

                var fixedSize = Vector3.one * AspectLayout.FixedLength;
                fixedSize.z = target.LocalSize.z;
                fixedSize.y *= AspectLayout.AspectRatio;

                if(fixedSize.x <= size.x && fixedSize.y <= size.y)
                {
                    size.x = fixedSize.x;
                    size.y = fixedSize.y;
                }

                AssertionUtils.AreNearlyEqual(size, target.LocalSize, LayoutDefines.NUMBER_PRECISION, $"Fail Test... LocalSize");
                AssertionUtils.AreNearlyEqual(offset, target.Offset, LayoutDefines.NUMBER_PRECISION, $"Fail Test... Offset");
                Assert.IsTrue(baseSize.x >= target.LocalSize.x
                    && baseSize.y >= target.LocalSize.y,
                    $"Test Fail... baseSize={baseSize}, localSize={target.LocalSize}"
                );
            }
        }

        /// <summary>
        /// <seealso cref="AspectSizeFitter.UpdateLayout()"/>
        /// </summary>
        [Test]
        public void ABTest_UpdateLayout_WhenFixedWidthFitModePasses()
        {
            var ABTest = new UpdateLayout_WhenFixedWidth_ABTestBase();

            var settings = TestSettings.CreateOrGet();
            ABTest.RunTest(settings);
        }

        class UpdateLayout_WhenFixedHeight_ABTestBase : UpdateLayout_ABTestBase
        {
            public UpdateLayout_WhenFixedHeight_ABTestBase() : base()
            {
                AspectLayout.CurrentMode = AspectSizeFitter.Mode.FixedHeight;
            }

            protected override void Validate()
            {
                var target = AspectLayout.Target;
                var layoutSize = Parent.LayoutInfo.GetLayoutSize(Parent)
                    .Mul(target.AnchorMax - target.AnchorMin);

                var (baseSize, offset) = CalSizeAndOffset(layoutSize, AspectLayout.Padding);
                var size = AdjustSize(baseSize, AspectLayout.AspectRatio);

                var fixedSize = Vector3.one * AspectLayout.FixedLength;
                fixedSize.z = 0;
                fixedSize.x *= AspectLayout.AspectRatio;

                if (fixedSize.x <= size.x && fixedSize.y <= size.y)
                {
                    size.x = fixedSize.x;
                    size.y = fixedSize.y;
                }

                AssertionUtils.AreNearlyEqual(size, target.LocalSize, LayoutDefines.NUMBER_PRECISION, $"Fail Test... LocalSize");
                AssertionUtils.AreNearlyEqual(offset, target.Offset, LayoutDefines.NUMBER_PRECISION, $"Fail Test... Offset");
                Assert.IsTrue(baseSize.x >= target.LocalSize.x
                    && baseSize.y >= target.LocalSize.y,
                    $"Test Fail... baseSize={baseSize:F4}, localSize={target.LocalSize:F4}"
                );
            }
        }

        /// <summary>
        /// <seealso cref="AspectSizeFitter.UpdateLayout()"/>
        /// </summary>
        [Test]
        public void ABTest_UpdateLayout_WhenFixedHeightModePasses()
        {
            var ABTest = new UpdateLayout_WhenFixedHeight_ABTestBase();

            var settings = TestSettings.CreateOrGet();
            ABTest.RunTest(settings);
        }
        #endregion

        #region Validate
        class ChildControllLayout : LayoutBase
        {
            public override LayoutOperationTarget OperationTargetFlags
            {
                get => LayoutOperationTarget.Children_LocalSize | LayoutOperationTarget.Children_Offset;
            }

            public override void UpdateLayout()
            {
                throw new System.NotImplementedException();
            }

            public override bool Validate()
            {
                throw new System.NotImplementedException();
            }
        }
        /// <summary>
        /// <seealso cref="AspectSizeFitter.Validate()"/>
        /// </summary>
        [Test]
        public void ValidatePasses()
        {
            {
                var aspectSizeFitter = new AspectSizeFitter();
                Assert.IsNull(aspectSizeFitter.Target);
                Assert.IsFalse(aspectSizeFitter.Validate());
            }
            Debug.Log($"Success to Validate(Target is null)!");

            {
                var aspectSizeFitter = new AspectSizeFitter();
                var target = new LayoutTargetObject();
                //target.SetParent(new LayoutTargetObject());
                aspectSizeFitter.Target = target;

                Assert.IsNotNull(aspectSizeFitter.Target);
                Assert.IsNull(aspectSizeFitter.Target.Parent);
                Assert.IsFalse(aspectSizeFitter.Validate());
            }
            Debug.Log($"Success to Validate(Target's Parent is null)!");

            {
                var aspectSizeFitter = new AspectSizeFitter();
                var target = new LayoutTargetObject();
                target.SetParent(new LayoutTargetObject());
                aspectSizeFitter.Target = target;
                Assert.IsNotNull(aspectSizeFitter.Target);
                Assert.IsNotNull(aspectSizeFitter.Target.Parent);
                Assert.IsTrue(aspectSizeFitter.Validate());
            }
            Debug.Log($"Success to Validate(Target and it's Parent is not null)!");

            {
                var aspectSizeFitter = new AspectSizeFitter();
                aspectSizeFitter.OperationPriority = 0;

                var otherAspectSizeFitter = new AspectSizeFitter();
                otherAspectSizeFitter.OperationPriority = 100;

                var target = new LayoutTargetObject();
                target.SetParent(new LayoutTargetObject());

                otherAspectSizeFitter.Target = target;
                aspectSizeFitter.Target = target;
                Assert.IsNotNull(aspectSizeFitter.Target);
                Assert.IsNotNull(aspectSizeFitter.Target.Parent);
                Assert.IsFalse(aspectSizeFitter.Validate());
            }
            Debug.Log($"Success to Validate(Conflict Other Layout's OperationTargetFlags)!");

            {
                var aspectSizeFitter = new AspectSizeFitter();
                aspectSizeFitter.OperationPriority = 0;

                var parent = new LayoutTargetObject();
                var target = new LayoutTargetObject();
                target.SetParent(parent);
                parent.AddLayout(new ChildControllLayout());

                aspectSizeFitter.Target = target;

                Assert.IsNotNull(aspectSizeFitter.Target);
                Assert.IsNotNull(aspectSizeFitter.Target.Parent);
                Assert.IsFalse(aspectSizeFitter.Validate());
            }
            Debug.Log($"Success to Validate(Conflict Parent Layout's OperationTargetFlags(Children_XXX))!");
        }
        #endregion
    }
}

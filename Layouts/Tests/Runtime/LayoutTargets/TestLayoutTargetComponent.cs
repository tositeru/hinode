using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Tests;
using System.Linq;
using UnityEngine.UI;

namespace Hinode.Layouts.Tests
{
    /// <summary>
	/// <seealso cref="LayoutTargetComponent"/>
	/// </summary>
    public class TestLayoutTargetComponent : TestBase
    {
        static readonly float EPSILON = LayoutDefines.NUMBER_PRECISION;
        static readonly float EPSILON_POS = LayoutDefines.POS_NUMBER_PRECISION;

        /// <summary>
        /// LayoutTargetObjectのLocalPos、Offsetと
        /// RectTransformのpositionの相互変換のためオフセットを計算する。
        ///
        /// LayoutTargetObject -> RectTransformへのオフセットを表します。
        /// </summary>
        /// <param name="R"></param>
        /// <returns></returns>
        static Vector2 CalROffset(RectTransform R)
        {
            var parentR = R.parent as RectTransform;

            var anchorSize = parentR != null
                ? parentR.rect.size.Mul(R.anchorMax - R.anchorMin)
                : Vector2.zero;
            var ROffset = anchorSize.Mul(R.pivot - Vector2.one * 0.5f);
            var pivotOffset = R.rect.size.Mul(R.pivot - Vector2.one * 0.5f);
            ROffset += -pivotOffset;
            return ROffset;
        }

        static LayoutTargetComponent CreateInstnace(string name = "__obj")
        {
            var obj = new GameObject(name);
            return obj.AddComponent<LayoutTargetComponent>();
        }

        static LayoutTargetComponent CreateInstnaceWithRectTransform(string name = "__obj")
        {
            var obj = new GameObject(name);
            obj.AddComponent<RectTransform>();
            return obj.AddComponent<LayoutTargetComponent>();
        }

        #region R Property
        /// <summary>
        /// <seealso cref="LayoutTargetComponent.R"/>
        /// </summary>
        [UnityTest]
        public IEnumerator RPropertyPasses()
        {
            var obj = new GameObject("obj");
            obj.AddComponent<RectTransform>();
            var target = obj.AddComponent<LayoutTargetComponent>();

            Assert.AreSame(target.transform as RectTransform, target.R);
            yield break;
        }

        /// <summary>
        /// <seealso cref="LayoutTargetComponent.R"/>
        /// </summary>
        [UnityTest]
        public IEnumerator RPropertyFailsWhenNotAttarchRectTransform()
        {
            var obj = new GameObject();
            var target = obj.AddComponent<LayoutTargetComponent>();

            Assert.IsNull(target.R);
            yield break;
        }
        #endregion

        #region GetOrAdd
        /// <summary>
        /// <seealso cref="LayoutTargetComponent.GetOrAdd(GameObject)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator GetOrAddPasses()
        {
            {
                var obj = new GameObject();
                obj.AddComponent<RectTransform>();
                var layoutTarget = obj.AddComponent<LayoutTargetComponent>();

                Assert.AreSame(layoutTarget, LayoutTargetComponent.GetOrAdd(obj));
            }
            Debug.Log($"Success to already Attach LayoutTargetComponent!!");

            {
                var obj = new GameObject();
                obj.AddComponent<RectTransform>();
                var layoutTarget = LayoutTargetComponent.GetOrAdd(obj);
                Assert.IsNotNull(layoutTarget);
                Assert.AreSame(obj.transform as RectTransform, layoutTarget.R);
            }
            Debug.Log($"Success to Add LayoutTargetComponent!!");
            yield break;
        }
        #endregion

        #region AutoDetectUpdater
        /// <summary>
        /// <seealso cref="LayoutTargetComponent.AutoDetectUpdater()"/>
        /// <seealso cref="LayoutTargetComponent.Updater"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator AutoDetectUpdaterWhenDontMatchAnyPasses()
        {
            var target = CreateInstnace();

            target.AutoDetectUpdater();
            Assert.IsNull(target.Updater);
            yield break;
        }

        /// <summary>
        /// <seealso cref="LayoutTargetComponent.AutoDetectUpdater()"/>
        /// <seealso cref="LayoutTargetComponent.Updater"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator AutoDetectUpdaterWhenRectTransformPasses()
        {
            var target = CreateInstnaceWithRectTransform();

            target.AutoDetectUpdater();
            Assert.IsTrue(target.Updater is LayoutTargetComponent.RectTransformUpdater);
            yield break;
        }

        #endregion

        #region CopyToLayoutTarget By RectTransform
        class CopyToLayoutTargetByRectTransformABTest : IABTest
        {
            LayoutTargetComponent _target;

            (bool doUse, Vector3 value) _paramAnchoredPos3D = (false, Vector3.zero);
            (bool doUse, Vector2 value) _paramLocalSize = (false, Vector2.zero);
            (bool doUse, Vector2 min, Vector2 max) _paramAnchorMinMax = (false, Vector2.zero, Vector2.zero);
            (bool doUse, Vector2 value) _paramPivot = (false, Vector2.one * 0.5f);

            public CopyToLayoutTargetByRectTransformABTest()
            {
                var parent = CreateInstnaceWithRectTransform("parent");
                var canvas = parent.gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var canvasScaler = parent.gameObject.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(600, 600);

                _target = CreateInstnaceWithRectTransform("target");
                _target.LayoutTarget.SetParent(parent.LayoutTarget);

                _target.R.pivot = Vector2.one * 0.5f;
                _target.R.anchorMin = Vector2.one * 0.5f;
                _target.R.anchorMax = Vector2.one * 0.5f;
                _target.R.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100f);
                _target.R.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100f);
                _target.R.anchoredPosition3D = Vector3.zero;
            }

            protected override (string name, string paramText)[] GetParamTexts()
            {
                return new (string name, string paramText)[]
                {
                    ("AnchoredPos3D", $"{_paramAnchoredPos3D.doUse}:{_paramAnchoredPos3D.value:F2}"),
                    ("AnchorMin/Max", $"{_paramAnchorMinMax.doUse}:{_paramAnchorMinMax.min:F8},{_paramAnchorMinMax.max:F8}"),
                    ("Pivot", $"{_paramPivot.doUse}:{_paramPivot.value:F8}"),
                    ("LocalSize", $"{_paramLocalSize.doUse}:{_paramLocalSize.value:F2}"),
                };
            }

            enum UseParamKind
            {
                AnchoredPos3D,
                AnchorMinMax,
                Pivot,
                LocalSize,
            }

            protected override void InitParams(System.Random rnd)
            {
                _paramAnchoredPos3D.doUse = false;
                _paramAnchoredPos3D.value = _target.R.anchoredPosition3D;
                _paramAnchorMinMax.doUse = false;
                _paramAnchorMinMax.min = _target.R.anchorMin;
                _paramAnchorMinMax.max = _target.R.anchorMax;
                _paramPivot.doUse = false;
                _paramPivot.value = _target.R.pivot;
                _paramLocalSize.doUse = false;
                _paramLocalSize.value = _target.R.rect.size;

                var paramCount = 5;
                var useParamIndex = rnd.Next(paramCount);
                switch((UseParamKind)useParamIndex)
                {
                    case UseParamKind.AnchoredPos3D:
                        _paramAnchoredPos3D.doUse = true;
                        var posRange = 1000f;
                        _paramAnchoredPos3D.value = new Vector3(
                            rnd.Range(-posRange, posRange),
                            rnd.Range(-posRange, posRange),
                            rnd.Range(-posRange, posRange)
                        );
                        break;
                    case UseParamKind.AnchorMinMax:
                        _paramAnchorMinMax.doUse = true;
                        var anchorMin = 0f;
                        var anchorMax = 1f;
                        _paramAnchorMinMax.min = new Vector2(
                            rnd.Range(anchorMin, anchorMax),
                            rnd.Range(anchorMin, anchorMax)
                        );
                        _paramAnchorMinMax.max = new Vector2(
                            rnd.Range(anchorMin, anchorMax),
                            rnd.Range(anchorMin, anchorMax)
                        );
                        break;
                    case UseParamKind.Pivot:
                        _paramPivot.doUse = true;
                        _paramPivot.value = new Vector2(
                            rnd.Range(0f, 1f),
                            rnd.Range(0f, 1f)
                        );
                        break;
                    case UseParamKind.LocalSize:
                        _paramLocalSize.doUse = true;
                        var sizeRange = 10000f;
                        _paramLocalSize.value = new Vector2(
                            rnd.Range(0, sizeRange),
                            rnd.Range(0, sizeRange)
                        );
                        break;
                }
            }

            protected override void TestMethod()
            {
                _target.R.pivot = _paramPivot.value;
                _target.R.anchorMin = _paramAnchorMinMax.min;
                _target.R.anchorMax = _paramAnchorMinMax.max;
                _target.R.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _paramLocalSize.value.x);
                _target.R.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _paramLocalSize.value.y);
                _target.R.anchoredPosition3D = _paramAnchoredPos3D.value;

                _target.CopyToLayoutTarget();

                {//AnchoredPos3D/Offset
                    var localPos = _target.R.anchoredPosition3D - (Vector3)CalROffset(_target.R) + _target.LayoutTarget.Offset;
                    AssertionUtils.AreNearlyEqual(localPos, _target.LayoutTarget.LocalPos, EPSILON_POS, $"Don't Match LocalPos/Offset... layoutTarget localPos={_target.LayoutTarget.LocalPos}, offset={_target.LayoutTarget.Offset}");
                }

                {//AnchorMin/Max
                    var anchorMin = Vector3.Min(_paramAnchorMinMax.min, _paramAnchorMinMax.max);
                    var anchorMax = Vector3.Max(_paramAnchorMinMax.min, _paramAnchorMinMax.max);
                    AssertionUtils.AreNearlyEqual((Vector2)anchorMin, (Vector2)_target.LayoutTarget.AnchorMin, EPSILON, $"Don't Match AnchorMin...");
                    AssertionUtils.AreNearlyEqual((Vector2)anchorMax, (Vector2)_target.LayoutTarget.AnchorMax, EPSILON, $"Don't Match AnchorMax...");
                }

                {//LocalSize
                    AssertionUtils.AreNearlyEqual(_target.R.rect.size, (Vector2)_target.LayoutTarget.LocalSize, EPSILON, $"Don't Match LocalSize...");
                }
            }
        }

        /// <summary>
        /// <seealso cref="LayoutTargetComponent.CopyToLayoutTarget()"/>
        /// <seealso cref="LayoutTargetComponent.RectTransformUpdater"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator ABTestCopyToLayoutTargetByRectTransform()
        {
            var settings = TestSettings.CreateOrGet();
            var ABTest = new CopyToLayoutTargetByRectTransformABTest();
            yield return null;//wait initialize
            ABTest.RunTest(settings);
        }
        #endregion

        #region CopyToTransform by RectTransform
        class CopyToRectTransformABTest : IABTest
        {
            LayoutTargetComponent _target;

            (bool doUse, Vector3 value) _paramLocalPos = (false, Vector3.zero);
            (bool doUse, Vector3 value) _paramLocalSize = (false, Vector3.zero);
            (bool doUse, Vector3 min, Vector3 max) _paramAnchorMinMax = (false, Vector3.zero, Vector3.zero);
            (bool doUse, Vector3 value) _paramOffset = (false, Vector3.zero);

            public CopyToRectTransformABTest()
            {
                var parent = CreateInstnaceWithRectTransform("parent");
                var canvas = parent.gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var canvasScaler = parent.gameObject.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(600, 600);

                _target = CreateInstnaceWithRectTransform("target");
                _target.LayoutTarget.SetParent(parent.LayoutTarget);
            }

            protected override (string name, string paramText)[] GetParamTexts()
            {
                return new (string name, string paramText)[]
                {
                    ("LocalPos", $"{_paramLocalPos.doUse}:{_paramLocalPos.value:F2}"),
                    ("AnchorMin/Max", $"{_paramAnchorMinMax.doUse}:{_paramAnchorMinMax.min:F8},{_paramAnchorMinMax.max:F8}"),
                    ("Offset", $"{_paramOffset.doUse}:{_paramOffset.value:F2}"),
                    ("LocalSize", $"{_paramLocalSize.doUse}:{_paramLocalSize.value:F2}"),
                };
            }

            enum UseParamKind
            {
                LocalPos,
                AnchorMinMax,
                Offset,
                LocalSize,
            }

            protected override void InitParams(System.Random rnd)
            {
                _paramLocalPos.doUse = false;
                _paramAnchorMinMax.doUse = false;
                _paramOffset.doUse = false;
                _paramLocalSize.doUse = false;

                var paramCount = 5;
                var useParamIndex = rnd.Next(paramCount);
                switch ((UseParamKind)useParamIndex)
                {
                    case UseParamKind.LocalPos:
                        _paramLocalPos.doUse = true;
                        var posRange = 10000f;
                        _paramLocalPos.value = new Vector3(
                            rnd.Range(-posRange, posRange),
                            rnd.Range(-posRange, posRange),
                            rnd.Range(-posRange, posRange)
                        );
                        break;
                    case UseParamKind.AnchorMinMax:
                        _paramAnchorMinMax.doUse = true;
                        var anchorMin = 0f;
                        var anchorMax = 1f;
                        _paramAnchorMinMax.min = new Vector3(
                            rnd.Range(anchorMin, anchorMax),
                            rnd.Range(anchorMin, anchorMax),
                            rnd.Range(anchorMin, anchorMax)
                        );
                        _paramAnchorMinMax.max = new Vector3(
                            rnd.Range(anchorMin, anchorMax),
                            rnd.Range(anchorMin, anchorMax),
                            rnd.Range(anchorMin, anchorMax)
                        );
                        break;
                    case UseParamKind.Offset:
                        _paramOffset.doUse = true;
                        var offsetRange = 10000f;
                        _paramOffset.value = new Vector3(
                            rnd.Range(-offsetRange, offsetRange),
                            rnd.Range(-offsetRange, offsetRange),
                            rnd.Range(-offsetRange, offsetRange)
                        );
                        break;
                    case UseParamKind.LocalSize:
                        _paramLocalSize.doUse = true;
                        var sizeRange = 10000f;
                        _paramLocalSize.value = new Vector3(
                            rnd.Range(0, sizeRange),
                            rnd.Range(0, sizeRange),
                            rnd.Range(0, sizeRange)
                        );
                        break;
                }
            }

            protected override void TestMethod()
            {
                _target.LayoutTarget.SetAnchor(_paramAnchorMinMax.min, _paramAnchorMinMax.max);
                _target.LayoutTarget.UpdateLocalSize(
                    _paramLocalSize.value,
                    _paramOffset.value);
                _target.LayoutTarget.LocalPos = _paramLocalPos.value;

                _target.CopyToTransform();

                var layout = _target.LayoutTarget;
                var R = _target.R;

                {//LocalPos/Offset
                    //RectTransformのanchoredPosition3Dの計算がAnchorMin/MaxとPivot依存なため、
                    // 再現するのが大変なため、他のパラメータが変更されない時だけ判定するようにしています。
                    var localPos = (Vector3)CalROffset(_target.R) + _target.LayoutTarget.Offset + layout.LocalPos;
                    AssertionUtils.AreNearlyEqual(localPos, R.anchoredPosition3D, EPSILON_POS, $"Don't Match AnchoredPosition3D...");
                }

                {//AnchorMin/Max
                    var anchorMin = Vector3.Min(_paramAnchorMinMax.min, _paramAnchorMinMax.max);
                    var anchorMax = Vector3.Max(_paramAnchorMinMax.min, _paramAnchorMinMax.max);
                    AssertionUtils.AreNearlyEqual(R.anchorMin, (Vector2)anchorMin, EPSILON, $"Don't Match AnchorMin...");
                    AssertionUtils.AreNearlyEqual(R.anchorMax, (Vector2)anchorMax, EPSILON, $"Don't Match AnchorMax...");
                }

                {//LocalSize
                    AssertionUtils.AreNearlyEqual(R.rect.size, (Vector2)_target.LayoutTarget.LocalSize, EPSILON, $"Don't Match LocalSize...");
                }
            }
        }

        /// <summary>
        /// <seealso cref="LayoutTargetComponent.CopyToTransform()"/>
        /// <seealso cref="LayoutTargetComponent.RectTransformUpdater"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator ABTestCopyToRectTransform()
        {
            var settings = TestSettings.CreateOrGet();
            var ABTest = new CopyToRectTransformABTest();
            yield return null; // wait initialize
            ABTest.RunTest(settings);
        }
        #endregion

        #region UnityOnDestroy
        /// <summary>
        /// <seealso cref="LayoutTargetComponent.OnChangedParent"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator UnityOnDestroyPasses()
        {
            var component = CreateInstnaceWithRectTransform("target");

            var parent = CreateInstnaceWithRectTransform("parent");
            var children = new LayoutTargetComponent[] {
                CreateInstnaceWithRectTransform("child1"),
                CreateInstnaceWithRectTransform("child2"),
                CreateInstnaceWithRectTransform("child3"),
            };

            component.transform.SetParent(parent.transform);
            component.CopyToLayoutTarget();
            parent.CopyToLayoutTarget();
            component.LayoutTarget.SetParent(parent.LayoutTarget);
            foreach (var child in children)
            {
                child.transform.SetParent(component.transform);
                child.CopyToLayoutTarget();
                child.LayoutTarget.SetParent(component.LayoutTarget);
            }

            var callCounter = 0;
            component.LayoutTarget.OnDisposed.Add((_) => callCounter++);

            Object.Destroy(component); // <- test point
            yield return null;

            Assert.AreEqual(1, callCounter);
            Assert.IsNull(component.LayoutTarget.Parent);
            Assert.IsFalse(component.LayoutTarget.Children.Any());

            //親子階層のチェック
            Assert.IsFalse(parent.LayoutTarget.Children.Any());
            Assert.IsTrue(children.All(_c => _c.LayoutTarget.Parent == null));
        }
        #endregion
    }
}

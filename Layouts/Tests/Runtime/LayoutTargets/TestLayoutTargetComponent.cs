using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Tests;
using System.Linq;
using UnityEngine.UI;
using System.Reflection;

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

        static LayoutTargetComponent CreateInstanceWithRectTransform(string name = "__obj")
        {
            var obj = new GameObject(name);
            obj.AddComponent<RectTransform>();
            return obj.AddComponent<LayoutTargetComponent>();
        }

        /// <summary>
        /// RとlayoutTargetのパラメータが一致しているかどうか判定する
        /// </summary>
        /// <param name="R"></param>
        /// <param name="layoutTarget"></param>
        static void AssertLayoutTargetOfRectTransform(RectTransform R, ILayoutTarget layoutTarget)
        {
            Assert.IsNotNull(layoutTarget);
            AssertionUtils.AreNearlyEqual(R.anchorMin, (Vector2)layoutTarget.AnchorMin, LayoutDefines.NUMBER_PRECISION);
            AssertionUtils.AreNearlyEqual(R.anchorMax, (Vector2)layoutTarget.AnchorMax, LayoutDefines.NUMBER_PRECISION);
            AssertionUtils.AreNearlyEqual(R.rect.size, (Vector2)layoutTarget.LocalSize, LayoutDefines.NUMBER_PRECISION);
            var (offsetMin, offsetMax) = layoutTarget.AnchorOffsetMinMax();
            AssertionUtils.AreNearlyEqual(R.offsetMin * -1f, (Vector2)offsetMin, LayoutDefines.NUMBER_PRECISION);
            AssertionUtils.AreNearlyEqual(R.offsetMax, (Vector2)offsetMax, LayoutDefines.NUMBER_PRECISION);
            AssertionUtils.AreNearlyEqual(R.pivot, (Vector2)layoutTarget.Pivot, LayoutDefines.NUMBER_PRECISION);

            var localPos = R.anchoredPosition3D - (Vector3)CalROffset(R) + layoutTarget.Offset;
            AssertionUtils.AreNearlyEqual(localPos, layoutTarget.LocalPos, EPSILON_POS
                , $"Don't Match LocalPos/Offset... layoutTarget localPos={layoutTarget.LocalPos}, offset={layoutTarget.Offset}");
        }

        #region DisallowMultipleComponent
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void DisallowMultipleComponentPasses()
        {
            var disallowMultipleAttr = typeof(LayoutTargetComponent).GetCustomAttribute<DisallowMultipleComponent>();
            Assert.IsNotNull(disallowMultipleAttr);
        }
        #endregion

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
            var target = CreateInstanceWithRectTransform();

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
                var parent = CreateInstanceWithRectTransform("parent");
                parent.LayoutTarget.SetLocalSize(Vector3.one * 100f);

                var canvas = parent.gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var canvasScaler = parent.gameObject.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(600, 600);

                _target = CreateInstanceWithRectTransform("target");
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
                    var localPos = _target.R.anchoredPosition3D - _target.LayoutTarget.Offset;
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
                var parent = CreateInstanceWithRectTransform("parent");
                var canvas = parent.gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var canvasScaler = parent.gameObject.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(600, 600);

                _target = CreateInstanceWithRectTransform("target");
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
            var component = CreateInstanceWithRectTransform("target");

            var parent = CreateInstanceWithRectTransform("parent");
            var children = new LayoutTargetComponent[] {
                CreateInstanceWithRectTransform("child1"),
                CreateInstanceWithRectTransform("child2"),
                CreateInstanceWithRectTransform("child3"),
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

            var componentLayoutTarget = component.LayoutTarget; //Destroy後にcomponent.LayoutTargetにアクセスするためにキャッシュ
            Object.Destroy(component); // <- test point
            yield return null;

            Assert.AreEqual(1, callCounter);
            Assert.IsNull(componentLayoutTarget.Parent);
            Assert.IsFalse(componentLayoutTarget.Children.Any());

            //親子階層のチェック
            Assert.IsFalse(parent.LayoutTarget.Children.Any());
            Assert.IsTrue(children.All(_c => _c.LayoutTarget.Parent == null));
        }
        #endregion

        #region UpdateLayoutTargetHierachy
        /// <summary>
        /// <seealso cref="LayoutTargetComponent.UpdateLayoutTargetHierachy()"/>
        /// <seealso cref="LayoutTargetComponent.GetDummyLayoutTarget(GameObject)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator UpdateLayoutTargetHierachy_WhenParentPasses()
        {
            {
                var component = CreateInstanceWithRectTransform("target");
                var parent = new GameObject();
                component.transform.SetParent(parent.transform);

                component.UpdateLayoutTargetHierachy();

                Assert.IsNull(component.LayoutTarget.Parent);
                Assert.IsNull(component.GetDummyLayoutTarget(parent.transform));
            }
            Debug.Log($"Success When Parent!");

            {
                var component = CreateInstanceWithRectTransform("target2");
                var parent = new GameObject().AddComponent<LayoutTargetComponent>();
                component.transform.SetParent(parent.transform);

                component.UpdateLayoutTargetHierachy();

                Assert.AreSame(parent.LayoutTarget, component.LayoutTarget.Parent);
                Assert.IsNull(component.GetDummyLayoutTarget(parent.transform));
            }
            Debug.Log($"Success When Parent attached LayoutTargetComponent!");

            {
                var component = CreateInstanceWithRectTransform("target3");
                var parent = new GameObject().AddComponent<RectTransform>();
                component.transform.SetParent(parent);

                component.UpdateLayoutTargetHierachy();

                Assert.IsNotNull(component.LayoutTarget.Parent);
                Assert.IsNotNull(component.GetDummyLayoutTarget(parent.transform));
                Assert.AreSame(component.LayoutTarget.Parent, component.GetDummyLayoutTarget(parent.transform));
                AssertLayoutTargetOfRectTransform(parent, component.LayoutTarget.Parent);
            }
            Debug.Log($"Success When Parent having RectTransform!");

            {
                var component = CreateInstanceWithRectTransform("target4");
                var parent = new GameObject().AddComponent<RectTransform>()
                    .gameObject.AddComponent<LayoutTargetComponent>();
                component.transform.SetParent(parent.transform);

                component.UpdateLayoutTargetHierachy();

                Assert.IsNull(component.GetDummyLayoutTarget(parent.transform));
                Assert.AreSame(parent.LayoutTarget, component.LayoutTarget.Parent);
            }
            Debug.Log($"Success When Parent having RectTransform and attached LayoutTargetComponent!");

            yield return null;
        }

        /// <summary>
        /// <seealso cref="LayoutTargetComponent.UpdateLayoutTargetHierachy()"/>
        /// <seealso cref="LayoutTargetComponent.GetDummyLayoutTarget(GameObject)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest, Description("Transform#parentが変更された時の挙動テスト")]
        public IEnumerator UpdateLayoutTargetHierachy_WhenChangeParentPasses()
        {
            {
                var tag = $"Null -> Transform";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var parent = new GameObject("parent " + tag);

                component.UpdateLayoutTargetHierachy();
                Assert.IsNull(component.LayoutTarget.Parent);

                component.transform.SetParent(parent.transform);
                component.UpdateLayoutTargetHierachy(); // test point

                Assert.IsNull(component.LayoutTarget.Parent);
                Assert.IsNull(component.GetDummyLayoutTarget(parent.transform));
            }
            Debug.Log($"Success to Change Parent! Null -> Transform!");

            {
                var tag = $"Null -> LayoutTargetComponent";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var parent = new GameObject("parent " + tag).AddComponent<LayoutTargetComponent>();

                component.UpdateLayoutTargetHierachy();
                Assert.IsNull(component.LayoutTarget.Parent);

                component.transform.SetParent(parent.transform);
                component.UpdateLayoutTargetHierachy(); // test point

                Assert.AreSame(parent.LayoutTarget, component.LayoutTarget.Parent);
                Assert.IsNull(component.GetDummyLayoutTarget(parent.transform));
            }
            Debug.Log($"Success to Change Parent! Null -> LayoutTargetComponent!");

            {
                var tag = $"Null -> RectTransform";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var parent = new GameObject("parent " + tag).AddComponent<RectTransform>();

                component.UpdateLayoutTargetHierachy();
                Assert.IsNull(component.LayoutTarget.Parent);

                component.transform.SetParent(parent.transform);
                component.UpdateLayoutTargetHierachy(); // test point

                Assert.IsNotNull(component.GetDummyLayoutTarget(parent.transform));
                Assert.AreSame(component.LayoutTarget.Parent, component.GetDummyLayoutTarget(parent.transform));
                AssertLayoutTargetOfRectTransform(parent, component.LayoutTarget.Parent);
            }
            Debug.Log($"Success to Change Parent! Null -> RectTransform!");

            {
                var tag = $"Null -> LayoutTargetComponent with RectTransform";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var parent = new GameObject("parent " + tag).AddComponent<RectTransform>()
                    .gameObject.AddComponent<LayoutTargetComponent>();

                component.UpdateLayoutTargetHierachy();
                Assert.IsNull(component.LayoutTarget.Parent);

                component.transform.SetParent(parent.transform);
                component.UpdateLayoutTargetHierachy(); // test point

                Assert.AreSame(parent.LayoutTarget, component.LayoutTarget.Parent);
                Assert.IsNull(component.GetDummyLayoutTarget(parent.transform));
            }
            Debug.Log($"Success to Change Parent! Null -> LayoutTargetComponent with RectTransform!");

            {
                var tag = $"LayoutTargetComponent -> Null";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var parent = new GameObject("parent " + tag).AddComponent<LayoutTargetComponent>();
                component.transform.SetParent(parent.transform);
                component.UpdateLayoutTargetHierachy();

                component.transform.SetParent(null);
                component.UpdateLayoutTargetHierachy(); // test point

                Assert.IsNull(component.LayoutTarget.Parent);
                Assert.IsNull(component.GetDummyLayoutTarget(parent.transform));
            }
            Debug.Log($"Success to Change Parent! LayoutTargetComponent -> Null!");

            {
                var tag = $"LayoutTargetComponent -> Transform";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var parent = new GameObject("parent " + tag).AddComponent<LayoutTargetComponent>();
                component.transform.SetParent(parent.transform);
                component.UpdateLayoutTargetHierachy();

                var parentTransform = parent.transform;
                Object.Destroy(parent);
                yield return null; // wait to destroy firstParent
                component.UpdateLayoutTargetHierachy(); // test point

                Assert.IsNull(component.LayoutTarget.Parent);
                Assert.IsNull(component.GetDummyLayoutTarget(parentTransform));
            }
            Debug.Log($"Success to Change Parent! LayoutTargetComponent -> Transform!");

            {
                var tag = $"LayoutTargetComponent -> RectTransform";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var parent = new GameObject("parent " + tag).AddComponent<LayoutTargetComponent>();
                component.transform.SetParent(parent.transform);
                component.UpdateLayoutTargetHierachy();

                var R = parent.gameObject.AddComponent<RectTransform>();
                Object.Destroy(parent);
                yield return null; // wait to destroy parent's LayoutTargetComponent
                component.UpdateLayoutTargetHierachy(); // test point

                Assert.IsNotNull(component.GetDummyLayoutTarget(R));
                Assert.AreSame(component.LayoutTarget.Parent, component.GetDummyLayoutTarget(R));
                AssertLayoutTargetOfRectTransform(R, component.LayoutTarget.Parent);
            }
            Debug.Log($"Success to Change Parent! LayoutTargetComponent -> RectTransform!");

            {
                var tag = $"LayoutTargetComponent -> Other LayoutTargetComponent";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var parent = new GameObject("parent " + tag).AddComponent<LayoutTargetComponent>();
                component.transform.SetParent(parent.transform);
                component.UpdateLayoutTargetHierachy();

                var otherParent = new GameObject("parent " + tag).AddComponent<LayoutTargetComponent>();
                component.transform.SetParent(otherParent.transform);
                component.UpdateLayoutTargetHierachy(); // test point

                Assert.AreSame(otherParent.LayoutTarget, component.LayoutTarget.Parent);
                Assert.IsNull(component.GetDummyLayoutTarget(parent.transform));
                Assert.IsNull(component.GetDummyLayoutTarget(otherParent.transform));
            }
            Debug.Log($"Success to Change Parent! LayoutTargetComponent -> Other LayoutTargetComponent!");

            {
                var tag = $"RectTransform -> Null";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var parent = new GameObject("parent " + tag).AddComponent<RectTransform>();
                component.transform.SetParent(parent.transform);
                component.UpdateLayoutTargetHierachy();

                component.transform.SetParent(null);
                component.UpdateLayoutTargetHierachy(); // test point

                Assert.IsNull(component.LayoutTarget.Parent);
                Assert.IsNull(component.GetDummyLayoutTarget(parent.transform));
            }
            Debug.Log($"Success to Change Parent! RectTransform -> Null!");

#if false // ~ Unity 2019.4.9) RectTransformのみの削除はできない仕様みたいなので、テストから省いています。
            {
                var tag = $"RectTransform -> Transform";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var parent = new GameObject("parent " + tag).AddComponent<RectTransform>();
                component.transform.SetParent(parent.transform);
                component.UpdateLayoutTargetHierachy();

                var parentTransform = parent.transform;
                Object.Destroy(parent);
                yield return null;
                component.UpdateLayoutTargetHierachy(); // test point

                Assert.IsNull(component.LayoutTarget);
                Assert.IsNull(component.GetDummyLayoutTarget(parentTransform));
            }
            Debug.Log($"Success to Change Parent! RectTransform -> Transform!");

            {
                var tag = $"RectTransform -> LayoutTargetComponent";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var parent = new GameObject("parent " + tag).AddComponent<RectTransform>();
                component.transform.SetParent(parent.transform);
                component.UpdateLayoutTargetHierachy();

                var parentLayoutTarget = parent.gameObject.AddComponent<LayoutTargetComponent>();
                Object.Destroy(parent);
                yield return null;
                component.UpdateLayoutTargetHierachy(); // test point

                Assert.AreSame(parentLayoutTarget.LayoutTarget, component.LayoutTarget);
                Assert.IsNull(component.GetDummyLayoutTarget(parentLayoutTarget.transform));
            }
            Debug.Log($"Success to Change Parent! RectTransform -> LayoutTargetComponent!");

            {
                var tag = $"RectTransform -> LayoutTargetComponent with RectTransform";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var parent = new GameObject("parent " + tag).AddComponent<RectTransform>();
                component.transform.SetParent(parent.transform);
                component.UpdateLayoutTargetHierachy();

                var parentLayoutTarget = parent.gameObject.AddComponent<LayoutTargetComponent>();
                component.UpdateLayoutTargetHierachy(); // test point

                Assert.AreSame(parentLayoutTarget.LayoutTarget, component.LayoutTarget);
                Assert.IsNull(component.GetDummyLayoutTarget(parent.transform));
            }
            Debug.Log($"Success to Change Parent! RectTransform -> LayoutTargetComponent with RectTransform!");
#endif
            yield return null;
        }

        /// <summary>
        /// <seealso cref="LayoutTargetComponent.UpdateLayoutTargetHierachy()"/>
        /// <seealso cref="LayoutTargetComponent.GetDummyLayoutTarget(GameObject)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator UpdateLayoutTargetHierachy_WhenChildrenPasses()
        {
            {
                var component = CreateInstanceWithRectTransform("target");
                var childTransform = new GameObject("child Transform");
                childTransform.transform.SetParent(component.transform);
                var childLayoutTarget = new GameObject("child Transform").AddComponent<LayoutTargetComponent>();
                childLayoutTarget.transform.SetParent(component.transform);
                var childRectTransform = new GameObject("child RectTransform").AddComponent<RectTransform>();
                childRectTransform.transform.SetParent(component.transform);
                var childRectTransformLayoutTarget = new GameObject("child RectTransform").AddComponent<RectTransform>()
                    .gameObject.AddComponent<LayoutTargetComponent>();
                childRectTransformLayoutTarget.transform.SetParent(component.transform);

                component.UpdateLayoutTargetHierachy();

                Assert.IsNull(component.GetDummyLayoutTarget(childTransform.transform));
                Assert.IsNull(component.GetDummyLayoutTarget(childLayoutTarget.transform));
                Assert.IsNotNull(component.GetDummyLayoutTarget(childRectTransform.transform));
                Assert.IsNull(component.GetDummyLayoutTarget(childRectTransformLayoutTarget.transform));
                AssertionUtils.AssertEnumerableByUnordered(
                    new ILayoutTarget[] {
                        childLayoutTarget.LayoutTarget,
                        component.GetDummyLayoutTarget(childRectTransform.transform),
                        childRectTransformLayoutTarget.LayoutTarget,
                    }
                    , component.LayoutTarget.Children
                    , ""
                );
            }
            Debug.Log($"Success When Children");
            yield return null;
        }

        /// <summary>
        /// <seealso cref="LayoutTargetComponent.UpdateLayoutTargetHierachy()"/>
        /// <seealso cref="LayoutTargetComponent.GetDummyLayoutTarget(GameObject)"/>
        /// </summary>
        /// <returns></returns>
        [UnityTest, Description("Transform#childrenが変更された時の挙動テスト")]
        public IEnumerator UpdateLayoutTargetHierachy_WhenChangeChildrenPasses()
        {
            {
                var tag = $"Null -> Transform";
                var component = CreateInstanceWithRectTransform("target " + tag);
                component.UpdateLayoutTargetHierachy();

                var child = new GameObject("child Transform " + tag);
                child.transform.SetParent(component.transform);
                Assert.IsNull(component.GetDummyLayoutTarget(child.transform));

                component.UpdateLayoutTargetHierachy(); // test point

                Assert.IsNull(component.GetDummyLayoutTarget(child.transform));
                Assert.AreEqual(0, component.LayoutTarget.ChildCount);
            }
            Debug.Log($"Success to Change Child! Null -> Transform!");

            {
                var tag = $"Null -> LayoutTargetComponent";
                var component = CreateInstanceWithRectTransform("target " + tag);
                component.UpdateLayoutTargetHierachy();

                var child = new GameObject("child Transform " + tag).AddComponent<LayoutTargetComponent>();
                child.transform.SetParent(component.transform);
                Assert.IsNull(component.GetDummyLayoutTarget(child.transform));

                component.UpdateLayoutTargetHierachy(); // test point

                Assert.IsNull(component.GetDummyLayoutTarget(child.transform));
                Assert.IsTrue(component.LayoutTarget.Children.Any(_c => _c == child.LayoutTarget));
            }
            Debug.Log($"Success to Change Child! Null -> LayoutTargetComponent!");

            {
                var tag = $"Null -> RectTransform";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var child= new GameObject("child" + tag).AddComponent<RectTransform>();
                child.transform.SetParent(component.transform);
                Assert.IsNull(component.GetDummyLayoutTarget(child.transform));

                component.UpdateLayoutTargetHierachy(); // test point

                var dummyLayout = component.GetDummyLayoutTarget(child.transform);
                Assert.IsNotNull(dummyLayout);
                Assert.IsTrue(component.LayoutTarget.Children.Any(_c => _c == dummyLayout));
            }
            Debug.Log($"Success to Change Child! Null -> RectTransform!");

            {
                var tag = $"Null -> LayoutTargetComponent with RectTransform";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var child = new GameObject("child " + tag).AddComponent<RectTransform>()
                    .gameObject.AddComponent<LayoutTargetComponent>();
                child.transform.SetParent(component.transform);
                Assert.IsNull(component.GetDummyLayoutTarget(child.transform));

                component.UpdateLayoutTargetHierachy(); // test point

                Assert.IsNull(component.GetDummyLayoutTarget(child.transform));
                Assert.IsTrue(component.LayoutTarget.Children.Any(_c => _c == child.LayoutTarget));
            }
            Debug.Log($"Success to Change Child! Null -> LayoutTargetComponent with RectTransform!");

            {
                var tag = $"LayoutTargetComponent -> Null";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var child = new GameObject("child " + tag).AddComponent<LayoutTargetComponent>();
                child.transform.SetParent(component.transform);
                component.UpdateLayoutTargetHierachy();
                Assert.IsNull(component.GetDummyLayoutTarget(child.transform));
                Assert.IsTrue(component.LayoutTarget.Children.Any(_c => _c == child.LayoutTarget));

                var childLayoutTarget = child.LayoutTarget;
                var childTransform = child.transform;
                Object.Destroy(child);
                yield return null;
                component.UpdateLayoutTargetHierachy(); // test point

                Assert.IsNull(component.GetDummyLayoutTarget(childTransform));
                Assert.IsFalse(component.LayoutTarget.Children.Any(_c => _c == childLayoutTarget));
            }
            Debug.Log($"Success to Change Child! LayoutTargetComponent -> Null!");

            {
                var tag = $"LayoutTargetComponent -> Null(Change Parent)";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var child = new GameObject("child " + tag).AddComponent<LayoutTargetComponent>();
                child.transform.SetParent(component.transform);
                component.UpdateLayoutTargetHierachy();
                Assert.IsNull(component.GetDummyLayoutTarget(child.transform));
                Assert.IsTrue(component.LayoutTarget.Children.Any(_c => _c == child.LayoutTarget));

                child.transform.SetParent(null);
                component.UpdateLayoutTargetHierachy(); // test point

                Assert.IsNull(component.GetDummyLayoutTarget(child.transform));
                Assert.IsFalse(component.LayoutTarget.Children.Any(_c => _c == child.LayoutTarget));
            }
            Debug.Log($"Success to Change Child! LayoutTargetComponent -> Null(Change Parent)!");

            {
                var tag = $"LayoutTargetComponent -> Transform";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var child = new GameObject("child" + tag).AddComponent<LayoutTargetComponent>();
                child.transform.SetParent(component.transform);
                component.UpdateLayoutTargetHierachy();
                Assert.IsNull(component.GetDummyLayoutTarget(child.transform));

                var childGameObject = child.gameObject;
                Object.Destroy(child);
                yield return null; // wait to destroy firstParent
                component.UpdateLayoutTargetHierachy(); // test point

                Assert.IsNull(component.GetDummyLayoutTarget(childGameObject.transform));
                Assert.AreEqual(0, component.LayoutTarget.ChildCount);
            }
            Debug.Log($"Success to Change Child! LayoutTargetComponent -> Transform!");

            {
                var tag = $"LayoutTargetComponent -> RectTransform";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var child = new GameObject("child" + tag).AddComponent<LayoutTargetComponent>();
                child.transform.SetParent(component.transform);
                component.UpdateLayoutTargetHierachy();
                Assert.IsNull(component.GetDummyLayoutTarget(child.transform));

                var childR = child.gameObject.AddComponent<RectTransform>();
                Object.Destroy(child);
                yield return null; // wait to Destroy child's LayoutTargetComponent
                component.UpdateLayoutTargetHierachy(); // test point

                var dummyChildLayoutObj = component.GetDummyLayoutTarget(childR);
                Assert.IsNotNull(dummyChildLayoutObj);
                Assert.IsTrue(component.LayoutTarget.Children.Any(_c => _c == dummyChildLayoutObj));
                AssertLayoutTargetOfRectTransform(childR, dummyChildLayoutObj);
            }
            Debug.Log($"Success to Change Child! LayoutTargetComponent -> RectTransform!");

#if false // ~ Unity 2019.4.9) RectTransformのみの削除はできない仕様みたいなので、テストから省いています。
            {
                var tag = $"RectTransform -> Null";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var child = new GameObject("child " + tag).AddComponent<RectTransform>();
                child.transform.SetParent(component.transform);
                component.UpdateLayoutTargetHierachy();
                Assert.IsNotNull(component.GetDummyLayoutTarget(child.transform));

                child.transform.SetParent(null);
                component.UpdateLayoutTargetHierachy(); // test point

                Assert.IsNull(component.GetDummyLayoutTarget(child.transform));
                Assert.AreEqual(0, component.LayoutTarget.ChildCount);
            }
            Debug.Log($"Success to Change Child! RectTransform -> Null!");

            {
                var tag = $"RectTransform -> Transform";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var child = new GameObject("child " + tag).AddComponent<RectTransform>();
                child.transform.SetParent(component.transform);
                component.UpdateLayoutTargetHierachy();
                Assert.IsNotNull(component.GetDummyLayoutTarget(child.transform));

                var childGameObject = child.gameObject;
                Object.Destroy(child);
                yield return null;
                component.UpdateLayoutTargetHierachy(); // test point

                Assert.IsNull(component.GetDummyLayoutTarget(childGameObject.transform));
                Assert.AreEqual(0, component.LayoutTarget.ChildCount);
            }
            Debug.Log($"Success to Change Child! RectTransform -> Transform!");

            {
                var tag = $"RectTransform -> LayoutTargetComponent";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var child = new GameObject("child " + tag).AddComponent<RectTransform>();
                child.transform.SetParent(component.transform);
                component.UpdateLayoutTargetHierachy();
                Assert.IsNotNull(component.GetDummyLayoutTarget(child.transform));

                var childLayoutTarget = child.gameObject.AddComponent<LayoutTargetComponent>();
                Object.Destroy(child);
                yield return null;
                component.UpdateLayoutTargetHierachy(); // test point

                var dummyLayoutObj = component.GetDummyLayoutTarget(childLayoutTarget.transform);
                Assert.IsNotNull(dummyLayoutObj);
                Assert.IsTrue(component.LayoutTarget.Children.Any(_c => _c == dummyLayoutObj));
            }
            Debug.Log($"Success to Change Child! RectTransform -> LayoutTargetComponent!");
#endif
            {
                var tag = $"RectTransform -> LayoutTargetComponent with RectTransform";
                var component = CreateInstanceWithRectTransform("target " + tag);
                var child = new GameObject("child " + tag).AddComponent<RectTransform>();
                child.transform.SetParent(component.transform);
                component.UpdateLayoutTargetHierachy();
                Assert.IsNotNull(component.GetDummyLayoutTarget(child.transform));

                var childLayoutTarget = child.gameObject.AddComponent<LayoutTargetComponent>();
                component.UpdateLayoutTargetHierachy(); // test point

                Assert.IsNull(component.GetDummyLayoutTarget(childLayoutTarget.transform));
                AssertionUtils.AssertEnumerable(
                    new ILayoutTarget[] {
                        childLayoutTarget.LayoutTarget
                    }
                    , component.LayoutTarget.Children
                    , ""
                );
            }
            Debug.Log($"Success to Change Child! RectTransform -> LayoutTargetComponent with RectTransform!");
            yield return null;
        }
#endregion
    }
}

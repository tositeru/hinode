using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Hinode.Tests.MVC.Views
{
    /// <summary>
    /// <seealso cref="HVLayoutGroupViewObject"/>
    /// </summary>
    public class TestHVLayoutGroupViewObject : TestBase
    {
        [UnityTest]
        public IEnumerator FixedParamBinderUpdatePasses()
        {
            var layoutGroup = HVLayoutGroupViewObject.Create();
            {//Direction
                var paramBinder = new HVLayoutGroupViewObject.FixedParamBinder();
                paramBinder.Direction = DirectionType.Vertical;
                paramBinder.Update(null, layoutGroup);
                yield return null;
                Assert.AreEqual(paramBinder.Direction, layoutGroup.Direction);
                Assert.IsTrue(layoutGroup.GetComponent<HorizontalOrVerticalLayoutGroup>() is VerticalLayoutGroup);
            }

            {//ChildAligment
                var paramBinder = new HVLayoutGroupViewObject.FixedParamBinder();
                paramBinder.ChildAlignment = TextAnchor.MiddleLeft;
                paramBinder.Update(null, layoutGroup);
                yield return null;
                var layout = layoutGroup.GetComponent<HorizontalOrVerticalLayoutGroup>();
                Assert.AreEqual(paramBinder.ChildAlignment, layout.childAlignment);
            }
            {//Spacing
                var paramBinder = new HVLayoutGroupViewObject.FixedParamBinder();
                paramBinder.Spacing = 123f;
                paramBinder.Update(null, layoutGroup);
                yield return null;
                var layout = layoutGroup.GetComponent<HorizontalOrVerticalLayoutGroup>();
                Assert.AreEqual(paramBinder.Spacing, layout.spacing);
            }
            {//PaddingX
                var paramBinder = new HVLayoutGroupViewObject.FixedParamBinder();
                paramBinder.PaddingX = new Vector2Int(45, 67);
                paramBinder.Update(null, layoutGroup);
                yield return null;
                var layout = layoutGroup.GetComponent<HorizontalOrVerticalLayoutGroup>();
                Assert.AreEqual(paramBinder.PaddingX, new Vector2Int(layout.padding.left, layout.padding.right));
            }
            {//PaddingX
                var paramBinder = new HVLayoutGroupViewObject.FixedParamBinder();
                paramBinder.PaddingY = new Vector2Int(89, 12);
                paramBinder.Update(null, layoutGroup);
                yield return null;
                var layout = layoutGroup.GetComponent<HorizontalOrVerticalLayoutGroup>();
                Assert.AreEqual(paramBinder.PaddingY, new Vector2Int(layout.padding.top, layout.padding.bottom));
            }
            {//ControllChildWidth
                var paramBinder = new HVLayoutGroupViewObject.FixedParamBinder();
                paramBinder.ControllChildWidth = true;
                paramBinder.Update(null, layoutGroup);
                yield return null;
                var layout = layoutGroup.GetComponent<HorizontalOrVerticalLayoutGroup>();
                Assert.AreEqual(paramBinder.ControllChildWidth, layout.childControlWidth);
            }
            {//ControllChildHeight
                var paramBinder = new HVLayoutGroupViewObject.FixedParamBinder();
                paramBinder.ControllChildHeight = true;
                paramBinder.Update(null, layoutGroup);
                yield return null;
                var layout = layoutGroup.GetComponent<HorizontalOrVerticalLayoutGroup>();
                Assert.AreEqual(paramBinder.ControllChildHeight, layout.childControlHeight);
            }
            {//UseChildScaleX
                var paramBinder = new HVLayoutGroupViewObject.FixedParamBinder();
                paramBinder.UseChildScaleX = true;
                paramBinder.Update(null, layoutGroup);
                yield return null;
                var layout = layoutGroup.GetComponent<HorizontalOrVerticalLayoutGroup>();
                Assert.AreEqual(paramBinder.UseChildScaleX, layout.childScaleWidth);
            }
            {//UseChildScaleY
                var paramBinder = new HVLayoutGroupViewObject.FixedParamBinder();
                paramBinder.UseChildScaleY = true;
                paramBinder.Update(null, layoutGroup);
                yield return null;
                var layout = layoutGroup.GetComponent<HorizontalOrVerticalLayoutGroup>();
                Assert.AreEqual(paramBinder.UseChildScaleY, layout.childScaleHeight);
            }
            {//ChildForceExpandWidth
                var paramBinder = new HVLayoutGroupViewObject.FixedParamBinder();
                paramBinder.ChildForceExpandWidth = true;
                paramBinder.Update(null, layoutGroup);
                yield return null;
                var layout = layoutGroup.GetComponent<HorizontalOrVerticalLayoutGroup>();
                Assert.AreEqual(paramBinder.ChildForceExpandWidth, layout.childForceExpandWidth);
            }
            {//ChildForceExpandHeight 
                var paramBinder = new HVLayoutGroupViewObject.FixedParamBinder();
                paramBinder.ChildForceExpandHeight = true;
                paramBinder.Update(null, layoutGroup);
                yield return null;
                var layout = layoutGroup.GetComponent<HorizontalOrVerticalLayoutGroup>();
                Assert.AreEqual(paramBinder.ChildForceExpandHeight, layout.childForceExpandHeight);
            }
        }

        [UnityTest, Description("Directionが切り替わった時に以前のLayoutGroupのパラメータを引き継ぐかどうかのテスト")]
        public IEnumerator HoldPrevParamOnChangedDirectionPasses()
        {
            var layoutGroup = HVLayoutGroupViewObject.Create();
            var paramBinder = new HVLayoutGroupViewObject.FixedParamBinder();
            paramBinder.ChildAlignment = TextAnchor.MiddleLeft;
            paramBinder.Spacing = 123f;
            paramBinder.ControllChildWidth = true;
            paramBinder.ControllChildHeight = true;
            paramBinder.UseChildScaleX = true;
            paramBinder.UseChildScaleY = true;
            paramBinder.ChildForceExpandWidth = true;
            paramBinder.ChildForceExpandHeight = true;
            paramBinder.Update(null, layoutGroup);
            yield return null;

            var prev = layoutGroup.GetComponent<HorizontalOrVerticalLayoutGroup>();
            var padding = prev.padding;
            var spacing = prev.spacing;
            var childAlignment = prev.childAlignment;
            var childControlWidth = prev.childControlWidth;
            var childControlHeight = prev.childControlHeight;
            var childScaleWidth = prev.childScaleWidth;
            var childScaleHeight = prev.childScaleHeight;
            var childForceExpandWidth = prev.childForceExpandWidth;
            var childForceExpandHeight = prev.childForceExpandHeight;

            paramBinder.Direction = layoutGroup.Direction == DirectionType.Horizontal
                ? DirectionType.Vertical
                : DirectionType.Horizontal;
            paramBinder.Update(null, layoutGroup);

            yield return null;
            var cur = layoutGroup.GetComponent<HorizontalOrVerticalLayoutGroup>();
            Assert.AreEqual(paramBinder.Direction, layoutGroup.Direction);
            Assert.AreEqual(padding, cur.padding);
            Assert.AreEqual(spacing, cur.spacing);
            Assert.AreEqual(childAlignment, cur.childAlignment);
            Assert.AreEqual(childControlWidth, cur.childControlWidth);
            Assert.AreEqual(childControlHeight, cur.childControlHeight);
            Assert.AreEqual(childScaleWidth, cur.childScaleWidth);
            Assert.AreEqual(childScaleHeight, cur.childControlHeight);
            Assert.AreEqual(childForceExpandWidth, cur.childForceExpandWidth);
            Assert.AreEqual(childForceExpandHeight, cur.childForceExpandHeight);
        }
    }
}

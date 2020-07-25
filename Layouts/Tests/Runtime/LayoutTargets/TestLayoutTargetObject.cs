using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hinode.Layouts;
using System.Linq;

namespace Hinode.Tests.Layouts
{
    /// <summary>
	/// <seealso cref="LayoutTargetObject"/>
	/// </summary>
    public class TestLayoutTargetObject
    {
        readonly float EPSILON = LayoutDefines.NUMBER_PRECISION;

        #region Dispose
        /// <summary>
		/// <seealso cref="LayoutTargetObject.Dispose()"/>
		/// </summary>
        [Test]
        public void DisposePasses()
        {
            var self = new LayoutTargetObject();
            var parent = new LayoutTargetObject();
            var children = new LayoutTargetObject[] {
                new LayoutTargetObject(),
                new LayoutTargetObject(),
                new LayoutTargetObject(),
            };
            self.SetParent(parent);
            foreach(var c in children)
            {
                c.SetParent(self);
            }

            parent.Dispose();

            Assert.IsFalse(parent.Children.Any());

            Assert.IsNull(self.Parent);
            Assert.IsFalse(self.Children.Any());

            Assert.IsTrue(children.All(_c => _c.Parent == null));
        }

        /// <summary>
		/// <seealso cref="LayoutTargetObject.Dispose()"/>
        /// <seealso cref="LayoutTargetObject.OnDisposed(ILayoutTarget self)"/>
        /// </summary>
        [Test]
        public void OnDisposedPasses()
        {
            var layout = new LayoutTargetObject();
            var callCounter = 0;
            ILayoutTarget recievedSelf = null;
            layout.OnDisposed.Add((_self) => { callCounter++; recievedSelf = _self; });

            layout.Dispose();
            Assert.AreEqual(1, callCounter);
            Assert.AreEqual(layout, recievedSelf);
        }

        /// <summary>
		/// <seealso cref="LayoutTargetObject.Dispose()"/>
        /// <seealso cref="LayoutTargetObject.OnDisposed(ILayoutTarget self)"/>
        /// </summary>
        [Test]
        public void OnDisposedWhenAfterDisposePasses()
        {
            var layout = new LayoutTargetObject();
            var callCounter = 0;
            layout.OnDisposed.Add((_self) => { callCounter++; });
            layout.Dispose();

            callCounter = 0;
            layout.Dispose(); // <- test point
            Assert.AreEqual(0, callCounter);
        }

        /// <summary>
		/// <seealso cref="LayoutTargetObject.Dispose()"/>
		/// <seealso cref="LayoutTargetObject.OnChangedParent"/>
		/// </summary>
        [Test]
        public void ClearOnChangedParentInDisposePasses()
        {
            var layout = new LayoutTargetObject();
            var callCounter = 0;
            layout.OnChangedParent.Add((_self, _parent) => { callCounter++; });

            callCounter = 0;
            {// <- test point
                layout.Dispose();
                layout.SetParent(new LayoutTargetObject());
            }
            Assert.AreEqual(0, callCounter);
        }

        /// <summary>
		/// <seealso cref="LayoutTargetObject.Dispose()"/>
		/// <seealso cref="LayoutTargetObject.OnChangedChildren"/>
		/// </summary>
        [Test]
        public void ClearOnChangedChildrenInDisposePasses()
        {
            var layout = new LayoutTargetObject();
            var callCounter = 0;
            layout.OnChangedChildren.Add((_self, _child, _mode) => { callCounter++; });

            callCounter = 0;
            {// <- test point
                layout.Dispose();
                var child = new LayoutTargetObject();
                child.SetParent(layout); // test point
            }
            Assert.AreEqual(0, callCounter);
        }

        /// <summary>
		/// <seealso cref="LayoutTargetObject.Dispose()"/>
		/// <seealso cref="LayoutTargetObject.OnChangedLocalPos"/>
		/// </summary>
        [Test]
        public void ClearOnChangedLocalPosInDisposePasses()
        {
            var layout = new LayoutTargetObject();
            var callCounter = 0;
            layout.OnChangedLocalPos.Add((_self) => { callCounter++; });

            callCounter = 0;
            {// <- test point
                layout.Dispose();
                layout.LocalPos = new Vector3(10, 20, 30);
            }
            Assert.AreEqual(0, callCounter);
        }

        /// <summary>
		/// <seealso cref="LayoutTargetObject.Dispose()"/>
		/// <seealso cref="LayoutTargetObject.OnChangedLocalSize"/>
		/// </summary>
        [Test]
        public void ClearOnChangedLocalSizeInDisposePasses()
        {
            var layout = new LayoutTargetObject();
            var callCounter = 0;
            layout.OnChangedLocalSize.Add((_self) => { callCounter++; });

            callCounter = 0;
            {// <- test point
                layout.Dispose();
                layout.SetLocalSize(new Vector3(10, 20, 30));
            }
            Assert.AreEqual(0, callCounter);
        }
        #endregion

        #region SetParent/Parent
        /// <summary>
        /// <seealso cref="LayoutTargetObject.SetParent(ILayoutTarget parent)"/>
        /// </summary>
        [Test]
        public void SetParentPasses()
        {
            var layout = new LayoutTargetObject();
            Assert.IsNull(layout.Parent);

            var parent = new LayoutTargetObject();
            layout.SetParent(parent);
            Assert.AreSame(parent, layout.Parent);
            Assert.IsTrue(parent.Children.Contains(layout));
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.SetParent(ILayoutTarget parent)"/>
        /// </summary>
        [Test]
        public void SetParentNullPasses()
        {
            var layout = new LayoutTargetObject();
            var parent = new LayoutTargetObject();
            layout.SetParent(parent);

            layout.SetParent(null); // <- test point
            Assert.IsNull(layout.Parent);
            Assert.IsFalse(parent.Children.Contains(layout));
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.SetParent(ILayoutTarget parent)"/>
        /// </summary>
        [Test]
        public void SetParentSwapPasses()
        {
            var layout = new LayoutTargetObject();
            var removeParent = new LayoutTargetObject();
            var addParent = new LayoutTargetObject();
            layout.SetParent(removeParent);

            layout.SetParent(addParent); // <- test point

            Assert.AreSame(addParent, layout.Parent);
            Assert.IsFalse(removeParent.Children.Contains(layout));
            Assert.IsTrue(addParent.Children.Contains(layout));
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.SetParent(ILayoutTarget parent)"/>
		/// <seealso cref="LayoutTargetObject.OnChangedParent"/>
        /// </summary>
        [Test]
        public void OnChangedParentInSetParentPasses()
        {
            var layout = new LayoutTargetObject();
            int callCounter = 0;
            (ILayoutTarget self, ILayoutTarget parent) recievedData = default;
            layout.OnChangedParent.Add((_self, _parent) => {
                callCounter++;
                recievedData = (_self, _parent);
            });

            var parent = new LayoutTargetObject();
            layout.SetParent(parent); // <- test point 

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(layout, recievedData.self);
            Assert.AreSame(parent, recievedData.parent);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.SetParent(ILayoutTarget parent)"/>
        /// <seealso cref="LayoutTargetObject.OnChangedParent"/>
        /// </summary>
        [Test]
        public void OnChangedParentInSetParentWhenThrowExceptionPasses()
        {
            var layout = new LayoutTargetObject();
            layout.OnChangedParent.Add((_self, _parent) => {
                throw new System.Exception();
            });

            var parent = new LayoutTargetObject();
            layout.SetParent(parent); // <- test point 

            Assert.AreSame(parent, layout.Parent);
            Assert.IsTrue(parent.Children.Contains(layout));
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.SetParent(ILayoutTarget parent)"/>
		/// <seealso cref="LayoutTargetObject.OnChangedParent"/>
		/// </summary>
        [Test]
        public void OnChangedParentInSetParentNullPasses()
        {
            var layout = new LayoutTargetObject();
            var parent = new LayoutTargetObject();
            layout.SetParent(parent);

            int callCounter = 0;
            (ILayoutTarget self, ILayoutTarget parent) recievedData = default;
            layout.OnChangedParent.Add((_self, _parent) => {
                callCounter++;
                recievedData = (_self, _parent);
            });

            layout.SetParent(null); // <- test point 

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(layout, recievedData.self);
            Assert.IsNull(recievedData.parent);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.SetParent(ILayoutTarget parent)"/>
        /// <seealso cref="LayoutTargetObject.OnChangedParent"/>
        /// </summary>
        [Test]
        public void OnChangedParentInSetParentSwapPasses()
        {
            var layout = new LayoutTargetObject();
            var parent = new LayoutTargetObject();
            layout.SetParent(parent);

            int callCounter = 0;
            (ILayoutTarget self, ILayoutTarget parent) recievedData = default;
            layout.OnChangedParent.Add((_self, _parent) => {
                callCounter++;
                recievedData = (_self, _parent);
            });

            var parent2 = new LayoutTargetObject();
            layout.SetParent(parent2); // <- test point 

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(layout, recievedData.self);
            Assert.AreSame(parent2, recievedData.parent);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.SetParent(ILayoutTarget parent)"/>
		/// <seealso cref="LayoutTargetObject.OnChangedChildren"/>
		/// </summary>
        [Test]
        public void OnChangedChildInSetParentPasses()
        {
            var layout = new LayoutTargetObject();
            var parent = new LayoutTargetObject();
            int callCounter = 0;
            (ILayoutTarget self, ILayoutTarget child, ILayoutTargetOnChangedChildMode mode) recievedData = default;
            parent.OnChangedChildren.Add((_self, _child, _mode) => {
                callCounter++;
                recievedData = (_self, _child, _mode);
            });

            layout.SetParent(parent); // <- test point 

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(parent, recievedData.self);
            Assert.AreSame(layout, recievedData.child);
            Assert.AreEqual(ILayoutTargetOnChangedChildMode.Add, recievedData.mode);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.SetParent(ILayoutTarget parent)"/>
        /// <seealso cref="LayoutTargetObject.OnChangedChildren"/>
        /// </summary>
        [Test]
        public void OnChangedChildInSetParentWhenThrowExceptionPasses()
        {
            var layout = new LayoutTargetObject();
            var parent = new LayoutTargetObject();
            parent.OnChangedChildren.Add((_self, _child, _mode) => {
                throw new System.Exception();
            });

            layout.SetParent(parent); // <- test point 

            Assert.AreSame(parent, layout.Parent);
            Assert.IsTrue(parent.Children.Contains(layout));
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.SetParent(ILayoutTarget parent)"/>
        /// <seealso cref="LayoutTargetObject.OnChangedChildren"/>
        /// </summary>
        [Test]
        public void OnChangedChildInSetParentNullPasses()
        {
            var layout = new LayoutTargetObject();
            var parent = new LayoutTargetObject();
            layout.SetParent(parent);

            int callCounter = 0;
            (ILayoutTarget self, ILayoutTarget child, ILayoutTargetOnChangedChildMode mode) recievedData = default;
            parent.OnChangedChildren.Add((_self, _child, _mode) => {
                callCounter++;
                recievedData = (_self, _child, _mode);
            });

            layout.SetParent(null); // <- test point 

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(parent, recievedData.self);
            Assert.AreSame(layout, recievedData.child);
            Assert.AreEqual(ILayoutTargetOnChangedChildMode.Remove, recievedData.mode);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.SetParent(ILayoutTarget parent)"/>
		/// <seealso cref="LayoutTargetObject.OnChangedChildren"/>
		/// </summary>
        [Test]
        public void OnChangedChildInSetParentWhenSwapParentPasses()
        {
            var layout = new LayoutTargetObject();
            var removeParent = new LayoutTargetObject();
            var addParent = new LayoutTargetObject();
            layout.SetParent(removeParent);

            int removeCallCounter = 0;
            (ILayoutTarget self, ILayoutTarget child, ILayoutTargetOnChangedChildMode mode) removeRecievedData = default;
            removeParent.OnChangedChildren.Add((_self, _child, _mode) => {
                removeCallCounter++;
                removeRecievedData = (_self, _child, _mode);
            });
            int addCallCounter = 0;
            (ILayoutTarget self, ILayoutTarget child, ILayoutTargetOnChangedChildMode mode) addRecievedData = default;
            addParent.OnChangedChildren.Add((_self, _child, _mode) => {
                addCallCounter++;
                addRecievedData = (_self, _child, _mode);
            });

            layout.SetParent(addParent); // <- test point 

            {//prev parent
                Assert.AreEqual(1, removeCallCounter);
                Assert.AreSame(removeParent, removeRecievedData.self);
                Assert.AreSame(layout, removeRecievedData.child);
                Assert.AreEqual(ILayoutTargetOnChangedChildMode.Remove, removeRecievedData.mode);
            }

            {//new parent
                Assert.AreEqual(1, addCallCounter);
                Assert.AreSame(addParent, addRecievedData.self);
                Assert.AreSame(layout, addRecievedData.child);
                Assert.AreEqual(ILayoutTargetOnChangedChildMode.Add, addRecievedData.mode);
            }
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.SetParent(ILayoutTarget parent)"/>
		/// <seealso cref="LayoutTargetObject.OnChangedChildren"/>
		/// </summary>
        [Test]
        public void OnChangedChildInSetParentSwapWhenThrowExceptionPrevParentPasses()
        {
            var layout = new LayoutTargetObject();
            var removeParent = new LayoutTargetObject();
            var addParent = new LayoutTargetObject();
            layout.SetParent(removeParent);

            removeParent.OnChangedChildren.Add((_self, _child, _mode) =>
            {
                throw new System.Exception();
            });
            int addCallCounter = 0;
            (ILayoutTarget self, ILayoutTarget child, ILayoutTargetOnChangedChildMode mode) addRecievedData = default;
            addParent.OnChangedChildren.Add((_self, _child, _mode) =>
            {
                addCallCounter++;
                addRecievedData = (_self, _child, _mode);
            });

            layout.SetParent(addParent); // <- test point 

            {//prev parent
                Assert.IsFalse(removeParent.Children.Contains(layout));
            }

            {//new parent
                Assert.AreSame(addParent, layout.Parent);
                Assert.IsTrue(addParent.Children.Contains(layout));

                Assert.AreEqual(1, addCallCounter);
                Assert.AreSame(addParent, addRecievedData.self);
                Assert.AreSame(layout, addRecievedData.child);
                Assert.AreEqual(ILayoutTargetOnChangedChildMode.Add, addRecievedData.mode);
            }
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.SetParent(ILayoutTarget parent)"/>
        /// <seealso cref="LayoutTargetObject.OnChangedChildren"/>
        /// </summary>
        [Test]
        public void OnChangedChildInSetParentSwapWhenThrowExceptionNewParentPasses()
        {
            var layout = new LayoutTargetObject();
            var removeParent = new LayoutTargetObject();
            var addParent = new LayoutTargetObject();
            layout.SetParent(removeParent);

            int removeCallCounter = 0;
            (ILayoutTarget self, ILayoutTarget child, ILayoutTargetOnChangedChildMode mode) removeRecievedData = default;
            removeParent.OnChangedChildren.Add((_self, _child, _mode) => {
                removeCallCounter++;
                removeRecievedData = (_self, _child, _mode);
            });
            addParent.OnChangedChildren.Add((_self, _child, _mode) => {
                throw new System.Exception();
            });

            layout.SetParent(addParent); // <- test point 

            {//prev parent
                Assert.IsFalse(removeParent.Children.Contains(layout));

                Assert.AreEqual(1, removeCallCounter);
                Assert.AreSame(removeParent, removeRecievedData.self);
                Assert.AreSame(layout, removeRecievedData.child);
                Assert.AreEqual(ILayoutTargetOnChangedChildMode.Remove, removeRecievedData.mode);
            }

            {//new parent
                Assert.AreSame(addParent, layout.Parent);
                Assert.IsTrue(addParent.Children.Contains(layout));
            }
        }

        #endregion

        #region Children
        /// <summary>
        /// <seealso cref="LayoutTargetObject.Children"/>
        /// </summary>
        [Test]
        public void ChildrenPasses()
        {
            var parent = new LayoutTargetObject();

            var children = new LayoutTargetObject[]
            {
                new LayoutTargetObject(),
                new LayoutTargetObject(),
                new LayoutTargetObject(),
            };

            foreach (var c in children)
            {
                c.SetParent(parent);
            }

            AssertionUtils.AssertEnumerableByUnordered(
                children,
                parent.Children,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.Children"/>
        /// </summary>
        [Test]
        public void ChildrenPasses2()
        {
            var parent = new LayoutTargetObject();

            var children = new LayoutTargetObject[]
            {
                new LayoutTargetObject(),
                new LayoutTargetObject(),
                new LayoutTargetObject(),
            };

            var removeChild = new LayoutTargetObject();
            removeChild.SetParent(parent);

            foreach (var c in children)
            {
                c.SetParent(parent);
            }

            removeChild.SetParent(null);
            children[0].SetParent(null);

            AssertionUtils.AssertEnumerableByUnordered(
                new LayoutTargetObject[] {
                    children[1],
                    children[2],
                },
                parent.Children,
                ""
            );
        }
        #endregion

        #region LocalPos
        /// <summary>
		/// <seealso cref="LayoutTargetObject.LocalPos"/>
		/// </summary>
        [Test]
        public void LocalPosPasses()
        {
            var self = new LayoutTargetObject();

            var localPos = new Vector3(1, 2, 3);
            self.LocalPos = localPos;

            Assert.AreEqual(localPos, self.LocalPos);
        }

        /// <summary>
		/// <seealso cref="LayoutTargetObject.LocalPos"/>
		/// <seealso cref="LayoutTargetObject.OnChangedLocalPos"/>
		/// </summary>
        [Test]
        public void OnChangedLocalPosPasses()
        {
            var self = new LayoutTargetObject();

            var callCounter = 0;
            ILayoutTarget recievedData = null;
            self.OnChangedLocalPos.Add((_self) => { callCounter++; recievedData = _self; });

            var localPos = new Vector3(1, 2, 3);
            self.LocalPos = localPos;

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(self, recievedData);
        }

        /// <summary>
		/// <seealso cref="LayoutTargetObject.LocalPos"/>
		/// <seealso cref="LayoutTargetObject.OnChangedLocalPos"/>
		/// </summary>
        [Test]
        public void OnChangedLocalPosWhenThrowExceptionPasses()
        {
            var self = new LayoutTargetObject();
            self.OnChangedLocalPos.Add((_) => throw new System.Exception());

            var localPos = new Vector3(1, 2, 3);
            self.LocalPos = localPos;

            Assert.AreEqual(localPos, self.LocalPos);
        }
        #endregion

        #region LocalSize
        /// <summary>
		/// <seealso cref="LayoutTargetObject.LocalSize"/>
		/// </summary>
        [Test]
        public void LocalSizePasses()
        {
            var self = new LayoutTargetObject();
            var localSize = new Vector3(10, 20, 30);
            self.SetLocalSize(localSize);

            Assert.AreEqual(localSize, self.LocalSize);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.LocalSize"/>
		/// <seealso cref="LayoutTargetObject.OnChangedLocalSize"/>
        /// </summary>
        [Test]
        public void OnChangedLocalSizePasses()
        {
            var self = new LayoutTargetObject();

            var callCounter = 0;
            ILayoutTarget recievedData = null;
            self.OnChangedLocalSize.Add((_self) => {
                callCounter++;
                recievedData = self;
            });
            var localSize = new Vector3(10, 20, 30);
            self.SetLocalSize(localSize);

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(self, recievedData);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.LocalSize"/>
        /// <seealso cref="LayoutTargetObject.OnChangedLocalSize"/>
        /// </summary>
        [Test]
        public void OnChangedLocalSizeWhenThrowExceptionPasses()
        {
            var self = new LayoutTargetObject();
            self.OnChangedLocalSize.Add((_) => {
                throw new System.Exception();
            });

            var localSize = new Vector3(10, 20, 30);
            self.SetLocalSize(localSize);

            Assert.AreEqual(localSize, self.LocalSize);
        }
        #endregion

        #region AnchorMin
        /// <summary>
		/// <seealso cref="LayoutTargetObject.AnchorMin"/>
		/// </summary>
        [Test]
        public void AnchorMinPasses()
        {
            var self = new LayoutTargetObject();
            var anchorMin = new Vector3(0.5f, 0.5f, 0.5f);
            self.SetAnchor(anchorMin, Vector3.one);

            Assert.AreEqual(anchorMin, self.AnchorMin);
        }
        #endregion

        #region AnchorMax
        /// <summary>
        /// <seealso cref="LayoutTargetObject.AnchorMax"/>
        /// </summary>
        [Test]
        public void AnchorMaxPasses()
        {
            var self = new LayoutTargetObject();
            var anchorMax = new Vector3(0.5f, 0.5f, 0.5f);
            self.SetAnchor(Vector3.zero, anchorMax);

            Assert.AreEqual(anchorMax, self.AnchorMax);
        }
        #endregion

        #region AnchorOffsetMin
        /// <summary>
		/// <seealso cref="LayoutTargetObject.AnchorOffsetMin"/>
		/// </summary>
        [Test]
        public void AnchorOffsetMinPasses()
        {
            var self = new LayoutTargetObject();
            var anchorOffsetMin = new Vector3(0.5f, 0.5f, 0.5f);
            self.SetAnchorOffset(anchorOffsetMin, self.AnchorOffsetMax);

            Assert.AreEqual(anchorOffsetMin, self.AnchorOffsetMin);
        }
        #endregion

        #region AnchorOffsetMax
        /// <summary>
        /// <seealso cref="LayoutTargetObject.AnchorOffsetMax"/>
        /// </summary>
        [Test]
        public void AnchorOffsetMaxPasses()
        {
            var self = new LayoutTargetObject();
            var anchorOffsetMax = new Vector3(0.5f, 0.5f, 0.5f);
            self.SetAnchorOffset(self.AnchorOffsetMin, anchorOffsetMax);

            Assert.AreEqual(anchorOffsetMax, self.AnchorOffsetMax);
        }
        #endregion

        #region UpdateLocalSizeWithAnchorParam
        /// <summary>
        /// <seealso cref="LayoutTargetObject.UpdateLocalSizeWithAnchorParam(Vector3, Vector3, Vector3, Vector3)"/>
        /// </summary>
        [Test]
        public void UpdateLocalSizeWithAnchorParamPasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 0));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            var anchorMin = new Vector3(0, 0.5f, 0f);
            var anchorMax = new Vector3(1, 0.5f, 0f);
            var offsetMin = new Vector3(1, 2, 0f);
            var offsetMax = new Vector3(10f, 20f, 0f);
            self.UpdateLocalSizeWithAnchorParam(anchorMin, anchorMax, offsetMin, offsetMax);

            var localSize = parent.LocalSize.Mul(anchorMax - anchorMin) + (offsetMin + offsetMax);
            AssertionUtils.AreNearlyEqual(localSize, self.LocalSize, EPSILON);
            AssertionUtils.AreNearlyEqual(anchorMin, self.AnchorMin, EPSILON);
            AssertionUtils.AreNearlyEqual(anchorMax, self.AnchorMax, EPSILON);
            AssertionUtils.AreNearlyEqual(offsetMin, self.AnchorOffsetMin, EPSILON);
            AssertionUtils.AreNearlyEqual(offsetMax, self.AnchorOffsetMax, EPSILON);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.UpdateLocalSizeWithAnchorParam(Vector3, Vector3, Vector3, Vector3)"/>
        /// </summary>
        [Test, Description("計算の結果LocalSizeの要素が0以下になる時のテスト。LocalSizeが0になるようにしてください。")]
        public void UpdateLocalSizeWithAnchorParamWhenInvalidLocalSizePasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(10, 10, 10));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            var anchorMin = new Vector3(0, 0f, 0f);
            var anchorMax = new Vector3(1, 1, 1f);
            var offsetMin = new Vector3(-100, -100, -100f);
            var offsetMax = new Vector3(-100, -100f, -100f);
            self.UpdateLocalSizeWithAnchorParam(anchorMin, anchorMax, offsetMin, offsetMax);

            var anchorAreaSize = parent.LocalSize.Mul(anchorMax - anchorMin);
            AssertionUtils.AreNearlyEqual(Vector3.zero, self.LocalSize, EPSILON);
            AssertionUtils.AreNearlyEqual(anchorMin, self.AnchorMin, EPSILON);
            AssertionUtils.AreNearlyEqual(anchorMax, self.AnchorMax, EPSILON);

            //LocalSizeが0になるようにしてください。
            AssertionUtils.AreNearlyEqual(anchorAreaSize * -0.5f, self.AnchorOffsetMin, EPSILON);
            AssertionUtils.AreNearlyEqual(anchorAreaSize * -0.5f, self.AnchorOffsetMax, EPSILON);
        }

        /// <summary>
		/// <seealso cref="LayoutTargetObject.UpdateLocalSizeWithAnchorParam(Vector3, Vector3, Vector3, Vector3)"/>
		/// <seealso cref="LayoutTargetObject.OnChangedLocalSize"/>
		/// </summary>
        [Test]
        public void OnChangedLocalSizeInUpdateLocalSizeWithAnchorParamPasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 0));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            var callCounter = 0;
            ILayoutTarget recievedData = null;
            self.OnChangedLocalSize.Add((_self) => {
                callCounter++;
                recievedData = _self;
            });

            var anchorMin = new Vector3(0, 0.5f, 0f);
            var anchorMax = new Vector3(1, 0.5f, 0f);
            var offsetMin = new Vector3(1, 2, 0f);
            var offsetMax = new Vector3(10f, 20f, 0f);
            self.UpdateLocalSizeWithAnchorParam(anchorMin, anchorMax, offsetMin, offsetMax);

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(self, recievedData);
        }

        /// <summary>
		/// <seealso cref="LayoutTargetObject.UpdateLocalSizeWithAnchorParam(Vector3, Vector3, Vector3, Vector3)"/>
		/// <seealso cref="LayoutTargetObject.OnChangedLocalSize"/>
		/// </summary>
        [Test]
        public void OnChangedLocalSizeInUpdateLocalSizeWithAnchorParamWhenThrowExcptionPasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 0));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            self.OnChangedLocalSize.Add((_self) => {
                throw new System.Exception();
            });

            var anchorMin = new Vector3(0, 0.5f, 0f);
            var anchorMax = new Vector3(1, 0.5f, 0f);
            var offsetMin = new Vector3(1, 2, 0f);
            var offsetMax = new Vector3(10f, 20f, 0f);
            self.UpdateLocalSizeWithAnchorParam(anchorMin, anchorMax, offsetMin, offsetMax);

            var localSize = parent.LocalSize.Mul(anchorMax - anchorMin) + (offsetMin + offsetMax);
            AssertionUtils.AreNearlyEqual(localSize, self.LocalSize, EPSILON);
            AssertionUtils.AreNearlyEqual(anchorMin, self.AnchorMin, EPSILON);
            AssertionUtils.AreNearlyEqual(anchorMax, self.AnchorMax, EPSILON);
            AssertionUtils.AreNearlyEqual(offsetMin, self.AnchorOffsetMin, EPSILON);
            AssertionUtils.AreNearlyEqual(offsetMax, self.AnchorOffsetMax, EPSILON);
        }
        #endregion

        #region UpdateLocalSizeWithSizeAndAnchorParam
        /// <summary>
        /// <seealso cref="LayoutTargetObject.UpdateLocalSizeWithSizeAndAnchorParam(Vector3, Vector3, Vector3, Vector3, Vector3)"/>
        /// </summary>
        [Test]
        public void UpdateLocalSizeWithSizeAndAnchorParamPasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 0));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            var localSize = new Vector3(20, 40);
            var anchorMin = new Vector3(0, 0.5f, 0f);
            var anchorMax = new Vector3(1, 0.5f, 0f);
            var offset = new Vector3(10, 20, 30);
            self.UpdateLocalSizeWithSizeAndAnchorParam(localSize, anchorMin, anchorMax, offset);

            AssertionUtils.AreNearlyEqual(localSize, self.LocalSize, EPSILON);
            AssertionUtils.AreNearlyEqual(anchorMin, self.AnchorMin, EPSILON);
            AssertionUtils.AreNearlyEqual(anchorMax, self.AnchorMax, EPSILON);

            var offsetMin = -offset;
            var offsetMax = offset;
            AssertionUtils.AreNearlyEqual(offsetMin, self.AnchorOffsetMin, EPSILON);
            AssertionUtils.AreNearlyEqual(offsetMax, self.AnchorOffsetMax, EPSILON);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.UpdateLocalSizeWithSizeAndAnchorParam(Vector3, Vector3, Vector3, Vector3, Vector3)"/>
        /// </summary>
        [Test, Description("LocalSizeの各要素が0より下回る時のテスト。その要素が0になるようにしてください。")]
        public void UpdateLocalSizeWithSizeAndAnchorParamWhenInvalidLocalSizePasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 0));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            var localSize = new Vector3(-10, -10,-10);
            var anchorMin = new Vector3(0, 0.5f, 0f);
            var anchorMax = new Vector3(1, 0.5f, 0f);
            var offset = new Vector3(10, 20, 30);
            self.UpdateLocalSizeWithSizeAndAnchorParam(localSize, anchorMin, anchorMax, offset);

            AssertionUtils.AreNearlyEqual(Vector3.zero, self.LocalSize, EPSILON);
            AssertionUtils.AreNearlyEqual(anchorMin, self.AnchorMin, EPSILON);
            AssertionUtils.AreNearlyEqual(anchorMax, self.AnchorMax, EPSILON);

            var halfAnchorAreaSize = self.AnchorAreaSize() * 0.5f;
            AssertionUtils.AreNearlyEqual(-halfAnchorAreaSize, self.AnchorOffsetMin, EPSILON);
            AssertionUtils.AreNearlyEqual(-halfAnchorAreaSize, self.AnchorOffsetMax, EPSILON);
        }

        /// <summary>
		/// <seealso cref="LayoutTargetObject.UpdateLocalSizeWithSizeAndAnchorParam(Vector3, Vector3, Vector3, Vector3)"/>
		/// <seealso cref="LayoutTargetObject.OnChangedLocalSize"/>
		/// </summary>
        [Test]
        public void OnChangedLocalSizeInUpdateLocalSizeWithSizeAndAnchorParamPasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 0));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            var callCounter = 0;
            ILayoutTarget recievedData = null;
            self.OnChangedLocalSize.Add((_self) => {
                callCounter++;
                recievedData = _self;
            });

            var localSize = new Vector3(20, 40);
            var anchorMin = new Vector3(0, 0.5f, 0f);
            var anchorMax = new Vector3(1, 0.5f, 0f);
            var offset = new Vector3(10, 20, 30);
            self.UpdateLocalSizeWithSizeAndAnchorParam(localSize, anchorMin, anchorMax, offset);

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(self, recievedData);
        }

        /// <summary>
		/// <seealso cref="LayoutTargetObject.UpdateLocalSizeWithSizeAndAnchorParam(Vector3, Vector3, Vector3, Vector3)"/>
		/// <seealso cref="LayoutTargetObject.OnChangedLocalSize"/>
		/// </summary>
        [Test]
        public void OnChangedLocalSizeInUpdateLocalSizeWithSizeAndAnchorParamWhenThrowExcptionPasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 0));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            self.OnChangedLocalSize.Add((_self) => {
                throw new System.Exception();
            });

            var localSize = new Vector3(20, 40);
            var anchorMin = new Vector3(0, 0.5f, 0f);
            var anchorMax = new Vector3(1, 0.5f, 0f);
            var offset = new Vector3(10, 20, 30);
            self.UpdateLocalSizeWithSizeAndAnchorParam(localSize, anchorMin, anchorMax, offset);

            AssertionUtils.AreNearlyEqual(localSize, self.LocalSize, EPSILON);
            AssertionUtils.AreNearlyEqual(anchorMin, self.AnchorMin, EPSILON);
            AssertionUtils.AreNearlyEqual(anchorMax, self.AnchorMax, EPSILON);

            var offsetMin = -offset;
            var offsetMax = offset;
            AssertionUtils.AreNearlyEqual(offsetMin, self.AnchorOffsetMin, EPSILON);
            AssertionUtils.AreNearlyEqual(offsetMax, self.AnchorOffsetMax, EPSILON);
        }
        #endregion

        #region Follow Parent Area(Anchor)
        /// <summary>
		/// <seealso cref="LayoutTargetObject.LocalSize"/>
		/// <seealso cref="LayoutTargetObject.AnchorMin"/>
		/// <seealso cref="LayoutTargetObject.AnchorMax"/>
		/// <seealso cref="LayoutTargetObject.AnchorOffsetMin"/>
		/// <seealso cref="LayoutTargetObject.AnchorOffsetMax"/>
		/// </summary>
        [Test]
        public void AnchorFollowedParentAreaPasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 100));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            {
                var offsetMin = Vector3.one * 10;
                var offsetMax = Vector3.one * 20;
                self.UpdateLocalSizeWithAnchorParam(Vector3.zero, Vector3.one, offsetMin, offsetMax);

                parent.SetLocalSize(Vector3.one * 20f); // <- test point

                var errorMessage = $"AnchorMin/Max, AnchorOffsetMin/MaxはParentの領域に追従してSelfの領域が変更されても変更されないようにしてください。";
                Assert.AreEqual(parent.LocalSize + self.AnchorOffsetMin + self.AnchorOffsetMax, self.LocalSize, errorMessage);
                Assert.AreEqual(Vector3.zero, self.AnchorMin);
                Assert.AreEqual(Vector3.one, self.AnchorMax);
                AssertionUtils.AreNearlyEqual(offsetMin, self.AnchorOffsetMin);
                AssertionUtils.AreNearlyEqual(offsetMax, self.AnchorOffsetMax);
            }
        }
        #endregion
    }
}

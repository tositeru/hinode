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
	/// <seealso cref="LayoutTargetObject"/>
	/// </summary>
    public class TestLayoutTargetObject
    {
        static readonly float EPSILON = LayoutDefines.NUMBER_PRECISION;
        static readonly float EPSILON_POS = LayoutDefines.POS_NUMBER_PRECISION;

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

            self.Dispose();

            Assert.IsFalse(parent.Children.Any());

            Assert.IsNull(self.Parent);
            Assert.IsTrue(children.All(_c => _c.Parent == null));
            Assert.IsFalse(self.Children.Any(), $"child count={self.ChildCount}");
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
        public void ClearOnChangedParent_InDisposePasses()
        {
            var layout = new LayoutTargetObject();
            var callCounter = 0;
            layout.OnChangedParent.Add((_self, _parent, _prevParent) => { callCounter++; });

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
        public void ClearOnChangedChildren_InDisposePasses()
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
        public void ClearOnChangedLocalPos_InDisposePasses()
        {
            var layout = new LayoutTargetObject();
            var callCounter = 0;
            layout.OnChangedLocalPos.Add((_self, _) => { callCounter++; });

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
        public void ClearOnChangedLocalSize_InDisposePasses()
        {
            var layout = new LayoutTargetObject();
            var callCounter = 0;
            layout.OnChangedLocalSize.Add((_self, _) => { callCounter++; });

            callCounter = 0;
            {// <- test point
                layout.Dispose();
                layout.SetLocalSize(new Vector3(10, 20, 30));
            }
            Assert.AreEqual(0, callCounter);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.Dispose()"/>
        /// <seealso cref="LayoutTargetObject.OnChangedOffset"/>
        /// </summary>
        [Test]
        public void ClearOnChangedOffset_InDisposePasses()
        {
            var layout = new LayoutTargetObject();
            var callCounter = 0;
            layout.OnChangedOffset.Add((_self, _) => { callCounter++; });

            callCounter = 0;
            {// <- test point
                layout.Dispose();
                layout.SetOffset(new Vector3(10, 20, 30));
            }
            Assert.AreEqual(0, callCounter);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.Dispose()"/>
        /// <seealso cref="LayoutTargetObject.OnChangedPivot"/>
        /// </summary>
        [Test]
        public void ClearOnChangedPivot_InDisposePasses()
        {
            var layout = new LayoutTargetObject();
            var callCounter = 0;
            layout.OnChangedPivot.Add((_self, _) => { callCounter++; });

            callCounter = 0;
            {// <- test point
                layout.Dispose();
                layout.Pivot = new Vector3(10, 20, 30);
            }
            Assert.AreEqual(0, callCounter);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.Dispose()"/>
        /// <seealso cref="LayoutTargetObject.OnChangedLayoutInfo"/>
        /// </summary>
        [Test]
        public void ClearOnChangedLayoutInfo_InDisposePasses()
        {
            var layout = new LayoutTargetObject();
            var callCounter = 0;
            layout.OnChangedLayoutInfo.Add((_self, _) => { callCounter++; });

            callCounter = 0;
            {// <- test point
                layout.Dispose();

                layout.LayoutInfo.LayoutSize = Vector3.one * 234f;
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
        public void SetParent_NullPasses()
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
        public void SetParent_SwapPasses()
        {
            var layout = new LayoutTargetObject();
            var removeParent = new LayoutTargetObject();
            removeParent.SetLocalSize(Vector3.one * 100);
            var addParent = new LayoutTargetObject();
            addParent.SetLocalSize(Vector3.one * 200);

            var offsetMin = Vector3.one * 10f;
            var offsetMax = Vector3.one * 20f;
            layout.UpdateAnchorParam(Vector3.zero, Vector3.one, offsetMin, offsetMax);
            layout.SetParent(removeParent);

            layout.SetParent(addParent); // <- test point

            Assert.AreSame(addParent, layout.Parent);
            Assert.IsFalse(removeParent.Children.Contains(layout));
            Assert.IsTrue(addParent.Children.Contains(layout));

            {
                var errorMessage = $"親が切り替わってもAnchorOffsetMin/Maxは変更されないようにしてください。";
                var (_offsetMin, _offsetMax) = layout.AnchorOffsetMinMax();
                AssertionUtils.AreNearlyEqual(offsetMin, _offsetMin, LayoutDefines.NUMBER_PRECISION, errorMessage);
                AssertionUtils.AreNearlyEqual(offsetMax, _offsetMax, LayoutDefines.NUMBER_PRECISION, errorMessage);
                AssertionUtils.AreNearlyEqual(layout.Parent.LayoutSize() + offsetMin + offsetMax, layout.LocalSize, LayoutDefines.NUMBER_PRECISION, errorMessage);
            }
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.SetParent(ILayoutTarget parent)"/>
		/// <seealso cref="LayoutTargetObject.OnChangedParent"/>
        /// </summary>
        [Test]
        public void OnChangedParent_InSetParentPasses()
        {
            var layout = new LayoutTargetObject();
            int callCounter = 0;
            (ILayoutTarget self, ILayoutTarget parent, ILayoutTarget prevParent) recievedData = default;
            layout.OnChangedParent.Add((_self, _parent, _prevParent) => {
                callCounter++;
                recievedData = (_self, _parent, _prevParent);
            });

            var parent = new LayoutTargetObject();
            layout.SetParent(parent); // <- test point 

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(layout, recievedData.self);
            Assert.AreSame(parent, recievedData.parent);
            Assert.AreSame(null, recievedData.prevParent);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.SetParent(ILayoutTarget parent)"/>
        /// <seealso cref="LayoutTargetObject.OnChangedParent"/>
        /// </summary>
        [Test]
        public void OnChangedParentInSetParentWhenThrowExceptionPasses()
        {
            var layout = new LayoutTargetObject();
            layout.OnChangedParent.Add((_self, _parent, _prevParent) => {
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
            (ILayoutTarget self, ILayoutTarget parent, ILayoutTarget prevParent) recievedData = default;
            layout.OnChangedParent.Add((_self, _parent, _prevParent) => {
                callCounter++;
                recievedData = (_self, _parent, _prevParent);
            });

            layout.SetParent(null); // <- test point 

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(layout, recievedData.self);
            Assert.IsNull(recievedData.parent);
            Assert.AreSame(parent, recievedData.prevParent);
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
            (ILayoutTarget self, ILayoutTarget parent, ILayoutTarget prevParent) recievedData = default;
            layout.OnChangedParent.Add((_self, _parent, _prevParent) => {
                callCounter++;
                recievedData = (_self, _parent, _prevParent);
            });

            var parent2 = new LayoutTargetObject();
            layout.SetParent(parent2); // <- test point 

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(layout, recievedData.self);
            Assert.AreSame(parent2, recievedData.parent);
            Assert.AreSame(parent, recievedData.prevParent);
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
            (ILayoutTarget self, Vector3 prevPos) recievedData = default;
            self.OnChangedLocalPos.Add((_self, _prevPos) => { callCounter++; recievedData = (_self, _prevPos); });

            var prevLocalPos = self.LocalPos;
            var localPos = new Vector3(1, 2, 3);
            self.LocalPos = localPos;

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(self, recievedData.self);
            Assert.AreEqual(prevLocalPos, recievedData.prevPos);
        }

        /// <summary>
		/// <seealso cref="LayoutTargetObject.LocalPos"/>
		/// <seealso cref="LayoutTargetObject.OnChangedLocalPos"/>
		/// </summary>
        [Test]
        public void OnChangedLocalPosWhenThrowExceptionPasses()
        {
            var self = new LayoutTargetObject();
            self.OnChangedLocalPos.Add((_, __) => throw new System.Exception());

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
            (ILayoutTarget self, Vector3 localSize)recievedData = default;
            self.OnChangedLocalSize.Add((_self, _prevSize) => {
                callCounter++;
                recievedData = (_self, _prevSize);
            });
            var prevLocalSize = self.LocalSize;
            var localSize = new Vector3(10, 20, 30);
            self.SetLocalSize(localSize);

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(self, recievedData.self);
            Assert.AreEqual(prevLocalSize, recievedData.localSize);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.LocalSize"/>
        /// <seealso cref="LayoutTargetObject.OnChangedLocalSize"/>
        /// </summary>
        [Test]
        public void OnChangedLocalSizeWhenThrowExceptionPasses()
        {
            var self = new LayoutTargetObject();
            self.OnChangedLocalSize.Add((_, __) => {
                throw new System.Exception();
            });

            var localSize = new Vector3(10, 20, 30);
            self.SetLocalSize(localSize);

            Assert.AreEqual(localSize, self.LocalSize);
        }
        #endregion

        #region AnchorMin/Max
        /// <summary>
		/// <seealso cref="LayoutTargetObject.AnchorMin"/>
		/// </summary>
        [Test]
        public void AnchorMinPasses()
        {
            var self = new LayoutTargetObject();
            var callCounter = 0;
            (ILayoutTarget self, Vector3 prevMin, Vector3 prevMax) recievedValues = default;
            self.OnChangedAnchorMinMax.Add((_self, _prevMin, _prevMax) => {
                callCounter++;
                recievedValues = (_self, _prevMin, _prevMax);
            });

            var prevAnchorMin = self.AnchorMin;
            var prevAnchorMax = self.AnchorMax;
            var anchorMin = new Vector3(0.5f, 0.5f, 0.5f);
            self.SetAnchor(anchorMin, Vector3.one);

            Assert.AreEqual(anchorMin, self.AnchorMin);
            Assert.AreEqual(1, callCounter);
            Assert.AreSame(self, recievedValues.self);
            AssertionUtils.AreNearlyEqual(prevAnchorMin, recievedValues.prevMin, float.Epsilon);
            AssertionUtils.AreNearlyEqual(prevAnchorMax, recievedValues.prevMax, float.Epsilon);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.AnchorMax"/>
        /// <seealso cref="LayoutTargetObject.OnChangedAnchorMinMax"/>
        /// </summary>
        [Test]
        public void AnchorMaxPasses()
        {
            var self = new LayoutTargetObject();
            var callCounter = 0;
            (ILayoutTarget self, Vector3 prevMin, Vector3 prevMax) recievedValues = default;
            self.OnChangedAnchorMinMax.Add((_self, _prevMin, _prevMax) => {
                callCounter++;
                recievedValues = (_self, _prevMin, _prevMax);
            });

            var prevAnchorMin = self.AnchorMin;
            var prevAnchorMax = self.AnchorMax;
            var anchorMax = new Vector3(0.5f, 0.5f, 0.5f);
            self.SetAnchor(Vector3.zero, anchorMax);

            Assert.AreEqual(anchorMax, self.AnchorMax);
            Assert.AreEqual(1, callCounter);
            Assert.AreSame(self, recievedValues.self);
            AssertionUtils.AreNearlyEqual(prevAnchorMin, recievedValues.prevMin, float.Epsilon);
            AssertionUtils.AreNearlyEqual(prevAnchorMax, recievedValues.prevMax, float.Epsilon);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.OnChangedAnchorMinMax"/>
        /// </summary>
        [Test]
        public void OnChangedAnchorMinMax_WhenThrowException()
        {
            var self = new LayoutTargetObject();
            self.OnChangedAnchorMinMax.Add((_self, _prevMin, _prevMax) => {
                throw new System.Exception();
            });

            var anchorMin = new Vector3(0.5f, 0.5f, 0.5f);
            self.SetAnchor(anchorMin, Vector3.one);

            Assert.AreEqual(anchorMin, self.AnchorMin);
        }
        #endregion

        #region Offset
        /// <summary>
        /// <seealso cref="LayoutTargetObject.Offset"/>
        /// </summary>
        [Test]
        public void OffsetPasses()
        {
            var self = new LayoutTargetObject();
            var anchorOffsetMax = new Vector3(0.5f, 0.5f, 0.5f);
            var offset = new Vector3(10, 20, 30);
            self.UpdateLocalSize(self.LocalSize, offset);

            Assert.AreEqual(offset, self.Offset);
        }
        #endregion

        #region Layouts
        class LayoutClass : LayoutBase
        {
            public LayoutClass(int priority)
            {
                OperationPriority = priority;
            }

            public override LayoutOperationTarget OperationTargetFlags { get => 0; }

            public override void UpdateLayout() {}
            public override bool Validate() => true;

            public override string ToString()
            {
                return $"LayoutClass priority={OperationPriority}";
            }
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.Layouts"/>
        /// <seealso cref="LayoutTargetObject.AddLayout(ILayout)"/>
        /// </summary>
        [Test]
        public void AddLayoutPasses()
        {
            var layoutObjs = new LayoutClass[]
            {
                new LayoutClass(100),
                new LayoutClass(200),
                new LayoutClass(-100),
            };

            var layoutTarget = new LayoutTargetObject();

            foreach(var obj in layoutObjs)
            {
                layoutTarget.AddLayout(obj);
            }

            AssertionUtils.AssertEnumerable(
                new ILayout[]
                {
                    layoutObjs[2],
                    layoutObjs[0],
                    layoutObjs[1],
                },
                layoutTarget.Layouts,
                ""
            );
            Assert.IsTrue(layoutObjs.All(_l => _l.Target == layoutTarget));
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.Layouts"/>
        /// <seealso cref="LayoutTargetObject.RemoveLayout(ILayout)"/>
        /// </summary>
        [Test]
        public void RemoveLayoutPasses()
        {
            var layoutObjs = new LayoutClass[]
            {
                new LayoutClass(100),
                new LayoutClass(0),
                new LayoutClass(-100),
            };

            var layoutTarget = new LayoutTargetObject();
            foreach (var obj in layoutObjs)
            {
                layoutTarget.AddLayout(obj);
            }

            layoutTarget.RemoveLayout(layoutObjs[2]); // test point
            Assert.IsNull(layoutObjs[2].Target);
            AssertionUtils.AssertEnumerable(
                new ILayout[]
                {
                    layoutObjs[1],
                    layoutObjs[0],
                },
                layoutTarget.Layouts,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.Layouts"/>
        /// <seealso cref="LayoutTargetObject.AddLayout(ILayout)"/>
        /// </summary>
        [Test]
        public void ILayoutOnChangedOperationPriorityPasses()
        {
            var layoutObjs = new LayoutClass[]
            {
                new LayoutClass(0),
                new LayoutClass(100),
                new LayoutClass(-100),
            };

            var layoutTarget = new LayoutTargetObject();

            foreach (var obj in layoutObjs)
            {
                layoutTarget.AddLayout(obj);
            }

            layoutObjs[0].OperationPriority = layoutObjs[1].OperationPriority + 1;

            AssertionUtils.AssertEnumerable(
                new ILayout[]
                {
                    layoutObjs[2],
                    layoutObjs[1],
                    layoutObjs[0],
                },
                layoutTarget.Layouts,
                ""
            );
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.Layouts"/>
        /// <seealso cref="LayoutTargetObject.AddLayout(ILayout)"/>
        /// </summary>
        [Test]
        public void AddRemoveLayout_SetCallbacksPasses()
        {
            var layoutObjs = new LayoutClass(0);
            Assert.AreEqual(0, layoutObjs.OnDisposed.RegistedDelegateCount);
            Assert.AreEqual(0, layoutObjs.OnChangedOperationPriority.RegistedDelegateCount);

            var layoutTarget = new LayoutTargetObject();
            {
                layoutTarget.AddLayout(layoutObjs);

                Assert.AreEqual(1, layoutObjs.OnDisposed.RegistedDelegateCount);
                Assert.AreEqual(1, layoutObjs.OnChangedOperationPriority.RegistedDelegateCount);
            }

            {
                layoutTarget.RemoveLayout(layoutObjs);
                Assert.AreEqual(0, layoutObjs.OnDisposed.RegistedDelegateCount);
                Assert.AreEqual(0, layoutObjs.OnChangedOperationPriority.RegistedDelegateCount);
            }
        }

        #endregion

        #region UpdateLocalSizeWithAnchorParam
        /// <summary>
        /// <seealso cref="LayoutTargetObject.UpdateAnchorParam(Vector3, Vector3, Vector3, Vector3)"/>
        /// </summary>
        [Test]
        public void UpdateLocalSizeWithAnchorParamPasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 0));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            var valueRange = parent.LocalSize.x;
            var rnd = new System.Random();
            for(var i=0; i<1000; ++i)
            {
                var anchorMin = new Vector3(
                    rnd.Range(0, 1),
                    rnd.Range(0, 1),
                    rnd.Range(0, 1));
                var anchorMax = new Vector3(
                    rnd.Range(0, 1),
                    rnd.Range(0, 1),
                    rnd.Range(0, 1));
                var offsetMin = new Vector3(
                    rnd.Range(-valueRange, valueRange),
                    rnd.Range(-valueRange, valueRange),
                    rnd.Range(-valueRange, valueRange));
                var offsetMax = new Vector3(
                    rnd.Range(-valueRange, valueRange),
                    rnd.Range(-valueRange, valueRange),
                    rnd.Range(-valueRange, valueRange));
                self.UpdateAnchorParam(anchorMin, anchorMax, offsetMin, offsetMax);

                var localSize = parent.LocalSize.Mul(anchorMax - anchorMin) + (offsetMin + offsetMax);
                localSize = Vector3.Max(Vector3.zero, localSize);
                AssertionUtils.AreNearlyEqual(localSize, self.LocalSize, EPSILON);
                AssertionUtils.AreNearlyEqual(anchorMin, self.AnchorMin, EPSILON);
                AssertionUtils.AreNearlyEqual(anchorMax, self.AnchorMax, EPSILON);

                var (localMinPos, localMaxPos) = self.LocalAreaMinMaxPos();
                AssertionUtils.AreNearlyEqual((localMaxPos + localMinPos) * 0.5f, self.Offset, EPSILON);
            }
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.UpdateAnchorParam(Vector3, Vector3, Vector3, Vector3)"/>
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
            self.UpdateAnchorParam(anchorMin, anchorMax, offsetMin, offsetMax);

            var anchorAreaSize = parent.LocalSize.Mul(anchorMax - anchorMin);
            //LocalSizeが0になるようにしてください。
            AssertionUtils.AreNearlyEqual(Vector3.zero, self.LocalSize, EPSILON);
            AssertionUtils.AreNearlyEqual(anchorMin, self.AnchorMin, EPSILON);
            AssertionUtils.AreNearlyEqual(anchorMax, self.AnchorMax, EPSILON);

            AssertionUtils.AreNearlyEqual(Vector3.zero, self.Offset, EPSILON);
        }

        /// <summary>
		/// <seealso cref="LayoutTargetObject.UpdateAnchorParam(Vector3, Vector3, Vector3, Vector3)"/>
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
            (ILayoutTarget self, Vector3 prevSize) recievedData = default;
            self.OnChangedLocalSize.Add((_self, _prevSize) => {
                callCounter++;
                recievedData = (_self, _prevSize);
            });

            var prevLocalSize = self.LocalSize;
            var anchorMin = new Vector3(0, 0.5f, 0f);
            var anchorMax = new Vector3(1, 0.5f, 0f);
            var offsetMin = new Vector3(1, 2, 0f);
            var offsetMax = new Vector3(10f, 20f, 0f);
            self.UpdateAnchorParam(anchorMin, anchorMax, offsetMin, offsetMax);

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(self, recievedData.self);
            Assert.AreEqual(prevLocalSize, recievedData.prevSize);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.UpdateAnchorParam(Vector3, Vector3, Vector3, Vector3)"/>
        /// <seealso cref="LayoutTargetObject.OnChangedLocalSize"/>
        /// </summary>
        [Test, Description("LocalSizeが変更されないケースでOnChangedLocalSizeコールバックが呼び出されないかどうかのテスト")]
        public void OnChangedLocalSizeInUpdateLocalSizeWithAnchorParamWhenNotChangeLocalSizePasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 0));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            var prevLocalSize = self.LocalSize;
            var anchorMin = new Vector3(0, 0.5f, 0f);
            var anchorMax = new Vector3(1, 0.5f, 0f);
            var offsetMin = new Vector3(1, 2, 0f);
            var offsetMax = new Vector3(10f, 20f, 0f);
            self.UpdateAnchorParam(anchorMin, anchorMax, offsetMin, offsetMax);

            {
                var callCounter = 0;
                self.OnChangedLocalSize.Add((_self, _prevSize) => {
                    callCounter++;
                });

                //test point : Case not change LocalSize
                self.UpdateAnchorParam(anchorMin, anchorMax, offsetMin, offsetMax);

                Assert.AreEqual(0, callCounter);
            }
        }

        /// <summary>
		/// <seealso cref="LayoutTargetObject.UpdateAnchorParam(Vector3, Vector3, Vector3, Vector3)"/>
		/// <seealso cref="LayoutTargetObject.OnChangedLocalSize"/>
		/// </summary>
        [Test]
        public void OnChangedLocalSizeInUpdateLocalSizeWithAnchorParamWhenThrowExcptionPasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 0));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            self.OnChangedLocalSize.Add((_self, __) => {
                throw new System.Exception();
            });

            var anchorMin = new Vector3(0, 0.5f, 0f);
            var anchorMax = new Vector3(1, 0.5f, 0f);
            var offsetMin = new Vector3(1, 2, 0f);
            var offsetMax = new Vector3(10f, 20f, 0f);
            self.UpdateAnchorParam(anchorMin, anchorMax, offsetMin, offsetMax);

            var localSize = parent.LocalSize.Mul(anchorMax - anchorMin) + (offsetMin + offsetMax);
            AssertionUtils.AreNearlyEqual(localSize, self.LocalSize, EPSILON);
            AssertionUtils.AreNearlyEqual(anchorMin, self.AnchorMin, EPSILON);
            AssertionUtils.AreNearlyEqual(anchorMax, self.AnchorMax, EPSILON);

            var (localMinPos, localMaxPos) = self.LocalAreaMinMaxPos();
            AssertionUtils.AreNearlyEqual((localMaxPos + localMinPos) * 0.5f, self.Offset, EPSILON);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.UpdateAnchorParam(Vector3, Vector3, Vector3, Vector3)"/>
        /// <seealso cref="LayoutTargetObject.OnChangedOffset"/>
        /// </summary>
        [Test, Description("LocalSizeが変更されないケースでOnChangedOffsetコールバックが呼び出されないかどうかのテスト")]
        public void OnChangedOffsetInUpdateLocalSizeWithAnchorParamWhenNotChangeOffsetPasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 0));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            var prevLocalSize = self.LocalSize;
            var anchorMin = new Vector3(0, 0.5f, 0f);
            var anchorMax = new Vector3(1, 0.5f, 0f);
            var offsetMin = new Vector3(1, 2, 0f);
            var offsetMax = new Vector3(10f, 20f, 0f);
            self.UpdateAnchorParam(anchorMin, anchorMax, offsetMin, offsetMax);

            {
                var callCounter = 0;
                self.OnChangedOffset.Add((_self, __) => {
                    callCounter++;
                });

                //test point : Case not change Offset
                var offset = Vector3.one * 1f;
                self.UpdateAnchorParam(anchorMin, anchorMax, offsetMin + offset, offsetMax + offset);

                Assert.AreEqual(0, callCounter);
            }
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.UpdateAnchorParam(Vector3, Vector3, Vector3, Vector3)"/>
        /// <seealso cref="LayoutTargetObject.OnChangedOffset"/>
        /// </summary>
        [Test]
        public void OnChangedOffsetInUpdateLocalSizeWithAnchorParamPasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 0));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            var callCounter = 0;
            (ILayoutTarget self, Vector3 prevOffset) recievedData = default;
            self.OnChangedOffset.Add((_self, _prevOffset) => {
                callCounter++;
                recievedData = (_self, _prevOffset);
            });

            var prevOffset = self.Offset;
            var anchorMin = new Vector3(0, 0.5f, 0f);
            var anchorMax = new Vector3(1, 0.5f, 0f);
            var offsetMin = new Vector3(1, 2, 0f);
            var offsetMax = new Vector3(10f, 20f, 0f);
            self.UpdateAnchorParam(anchorMin, anchorMax, offsetMin, offsetMax);

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(self, recievedData.self);
            Assert.AreEqual(prevOffset, recievedData.prevOffset);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.UpdateAnchorParam(Vector3, Vector3, Vector3, Vector3)"/>
        /// <seealso cref="LayoutTargetObject.OnChangedOffset"/>
        /// </summary>
        [Test]
        public void OnChangedOffsetInUpdateLocalSizeWithAnchorParamWhenThrowExcptionPasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 0));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            self.OnChangedOffset.Add((_self, __) => {
                throw new System.Exception();
            });

            var anchorMin = new Vector3(0, 0.5f, 0f);
            var anchorMax = new Vector3(1, 0.5f, 0f);
            var offsetMin = new Vector3(1, 2, 0f);
            var offsetMax = new Vector3(10f, 20f, 0f);
            self.UpdateAnchorParam(anchorMin, anchorMax, offsetMin, offsetMax);

            var localSize = parent.LocalSize.Mul(anchorMax - anchorMin) + (offsetMin + offsetMax);
            AssertionUtils.AreNearlyEqual(localSize, self.LocalSize, EPSILON);
            AssertionUtils.AreNearlyEqual(anchorMin, self.AnchorMin, EPSILON);
            AssertionUtils.AreNearlyEqual(anchorMax, self.AnchorMax, EPSILON);

            var (localMinPos, localMaxPos) = self.LocalAreaMinMaxPos();
            AssertionUtils.AreNearlyEqual((localMaxPos + localMinPos) * 0.5f, self.Offset, EPSILON);
        }
        #endregion

        #region UpdateLocalSizeWithSizeAndAnchorParam
        /// <summary>
        /// <seealso cref="LayoutTargetObject.UpdateLocalSize(Vector3, Vector3)"/>
        /// </summary>
        [Test]
        public void UpdateLocalSizeWithSizeAndAnchorParamPasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 0));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            var localSize = new Vector3(20, 40);
            var offset = new Vector3(10, 20, 30);
            self.UpdateLocalSize(localSize, offset);

            AssertionUtils.AreNearlyEqual(localSize, self.LocalSize, EPSILON);

            AssertionUtils.AreNearlyEqual(offset, self.Offset, EPSILON);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.UpdateLocalSize(Vector3, Vector3)"/>
        /// </summary>
        [Test, Description("LocalSizeの各要素が0より下回る時のテスト。その要素が0になるようにしてください。")]
        public void UpdateLocalSizeWithSizeAndAnchorParamWhenInvalidLocalSizePasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 0));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            var localSize = new Vector3(-10, -10,-10);
            var offset = new Vector3(10, 20, 30);
            self.UpdateLocalSize(localSize, offset);

            AssertionUtils.AreNearlyEqual(Vector3.zero, self.LocalSize, EPSILON);

            AssertionUtils.AreNearlyEqual(offset, self.Offset, EPSILON);
        }

        /// <summary>
		/// <seealso cref="LayoutTargetObject.UpdateLocalSize(Vector3, Vector3)"/>
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
            (ILayoutTarget self, Vector3 prevSize) recievedData = default;
            self.OnChangedLocalSize.Add((_self, _prevSize) => {
                callCounter++;
                recievedData = (_self, _prevSize);
            });

            var prevLocalSize = self.LocalSize;
            var localSize = new Vector3(20, 40);
            var offset = new Vector3(10, 20, 30);
            self.UpdateLocalSize(localSize, offset);

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(self, recievedData.self);
            Assert.AreEqual(prevLocalSize, recievedData.prevSize);
        }

        /// <summary>
		/// <seealso cref="LayoutTargetObject.UpdateLocalSize(Vector3, Vector3)"/>
		/// <seealso cref="LayoutTargetObject.OnChangedLocalSize"/>
		/// </summary>
        [Test]
        public void OnChangedLocalSizeInUpdateLocalSizeWithSizeAndAnchorParamWhenThrowExcptionPasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 0));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            self.OnChangedLocalSize.Add((_self, __) => {
                throw new System.Exception();
            });

            var localSize = new Vector3(20, 40);
            var offset = new Vector3(10, 20, 30);
            self.UpdateLocalSize(localSize, offset);

            AssertionUtils.AreNearlyEqual(localSize, self.LocalSize, EPSILON);
            AssertionUtils.AreNearlyEqual(offset, self.Offset, EPSILON);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.UpdateLocalSize(Vector3, Vector3)"/>
        /// <seealso cref="LayoutTargetObject.OnChangedLocalSize"/>
        /// </summary>
        [Test, Description("LocalSizeが変更されないケースでOnChangedLocalSizeコールバックが呼び出されないかどうかのテスト")]
        public void OnChangedLocalSizeInUpdateLocalSizeWithSizeAndAnchorParamWhenNotChangeLocalSizePasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 0));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            var localSize = new Vector3(20, 40);
            var offset = new Vector3(10, 20, 30);
            self.UpdateLocalSize(localSize, offset);

            {
                var callCounter = 0;
                self.OnChangedLocalSize.Add((_self, _prevSize) => {
                    callCounter++;
                });

                //test point : Case not change LocalSize
                self.UpdateLocalSize(localSize, offset);

                Assert.AreEqual(0, callCounter);
            }
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.UpdateLocalSize(Vector3, Vector3)"/>
        /// <seealso cref="LayoutTargetObject.OnChangedOffset"/>
        /// </summary>
        [Test]
        public void OnChangedOffsetInUpdateLocalSizeWithSizeAndAnchorParamPasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 0));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            var callCounter = 0;
            (ILayoutTarget self, Vector3 prevOffset) recievedData = default;
            self.OnChangedOffset.Add((_self, _prevOffset) => {
                callCounter++;
                recievedData = (_self, _prevOffset);
            });

            var prevOffset = self.Offset;
            var localSize = new Vector3(20, 40);
            var offset = new Vector3(10, 20, 30);
            self.UpdateLocalSize(localSize, offset);

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(self, recievedData.self);
            Assert.AreEqual(prevOffset, recievedData.prevOffset);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.UpdateLocalSize(Vector3, Vector3)"/>
        /// <seealso cref="LayoutTargetObject.OnChangedOffset"/>
        /// </summary>
        [Test]
        public void OnChangedOffsetInUpdateLocalSizeWithSizeAndAnchorParamWhenThrowExcptionPasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 0));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            self.OnChangedOffset.Add((_self, __) => {
                throw new System.Exception();
            });

            var localSize = new Vector3(20, 40);
            var offset = new Vector3(10, 20, 30);
            self.UpdateLocalSize(localSize, offset);

            AssertionUtils.AreNearlyEqual(localSize, self.LocalSize, EPSILON);
            AssertionUtils.AreNearlyEqual(offset, self.Offset, EPSILON);

            var (localMinPos, localMaxPos) = self.LocalAreaMinMaxPos();
            AssertionUtils.AreNearlyEqual((localMaxPos + localMinPos) * 0.5f, self.Offset, EPSILON);
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.UpdateLocalSize(Vector3, Vector3)"/>
        /// <seealso cref="LayoutTargetObject.OnChangedLocalOffset"/>
        /// </summary>
        [Test, Description("LocalOffsetが変更されないケースでOnChangedLocalOffsetコールバックが呼び出されないかどうかのテスト")]
        public void OnChangedOffsetInUpdateLocalSizeWithSizeAndAnchorParamWhenNotChangeLocalSizePasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 0));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            var localSize = new Vector3(20, 40);
            var offset = new Vector3(10, 20, 30);
            self.UpdateLocalSize(localSize, offset);

            {
                var callCounter = 0;
                self.OnChangedOffset.Add((_self, _prevSize) => {
                    callCounter++;
                });

                //test point : Case not change Offset
                self.UpdateLocalSize(localSize, offset);

                Assert.AreEqual(0, callCounter);
            }
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
                self.UpdateAnchorParam(Vector3.zero, Vector3.one, offsetMin, offsetMax);

                parent.SetLocalSize(Vector3.one * 20f); // <- test point

                var errorMessage = $"AnchorMin/Max, AnchorOffsetMin/MaxはParentの領域に追従してSelfの領域が変更されても変更されないようにしてください。";
                var (_offsetMin, _offsetMax) = self.AnchorOffsetMinMax();
                AssertionUtils.AreNearlyEqual(parent.LocalSize + offsetMin + offsetMax, self.LocalSize, EPSILON, errorMessage);
                AssertionUtils.AreNearlyEqual(Vector3.zero, self.AnchorMin, EPSILON, errorMessage);
                AssertionUtils.AreNearlyEqual(Vector3.one, self.AnchorMax, EPSILON, errorMessage);
                AssertionUtils.AreNearlyEqual(offsetMin, _offsetMin, EPSILON, errorMessage);
                AssertionUtils.AreNearlyEqual(offsetMax, _offsetMax, EPSILON, errorMessage);
            }
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.IsAutoUpdate"/>
        /// </summary>
        [Test]
        public void IsAutoUpdatePasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(new Vector3(100, 100, 100));
            var self = new LayoutTargetObject();
            self.SetParent(parent);

            Assert.IsTrue(self.IsAutoUpdate, "Default値はtureにしてください。");

            {
                var offsetMin = Vector3.one * 10;
                var offsetMax = Vector3.one * 20;
                self.UpdateAnchorParam(Vector3.zero, Vector3.one, offsetMin, offsetMax);

                var prevLocalSize = self.LocalSize;
                var prevOffset = self.Offset;

                self.IsAutoUpdate = false;
                parent.SetLocalSize(Vector3.one * 20f); // <- test point

                var errorMessage = $"LayoutTargetObject#IsAutoUpdateがfalseの時はParentの領域に追従しないようにしてください。";
                var (_offsetMin, _offsetMax) = self.AnchorOffsetMinMax();
                AssertionUtils.AreNearlyEqual(prevLocalSize, self.LocalSize, EPSILON, errorMessage);
                AssertionUtils.AreNearlyEqual(prevOffset, self.Offset, EPSILON, errorMessage);
                AssertionUtils.AreNearlyEqual(offsetMin, _offsetMin, EPSILON, errorMessage);
                AssertionUtils.AreNearlyEqual(offsetMax, _offsetMax, EPSILON, errorMessage);
            }
        }
        #endregion

        #region FollowParent
        /// <summary>
        /// <seealso cref="LayoutTargetObject.FollowParent()"/>
        /// </summary>
        [Test]
        public void FollowParentPasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(Vector3.one * 20f);

            var self = new LayoutTargetObject();
            self.IsAutoUpdate = false;
            self.SetParent(parent);

            {
                var offsetMin = Vector3.one * 10;
                var offsetMax = Vector3.one * 20;
                self.UpdateAnchorParam(Vector3.zero, Vector3.one, offsetMin, offsetMax);

                self.FollowParent(); // test point

                var errorMessage = $"Test Fail...";
                var (_offsetMin, _offsetMax) = self.AnchorOffsetMinMax();
                AssertionUtils.AreNearlyEqual(parent.LocalSize + offsetMin + offsetMax, self.LocalSize, EPSILON, errorMessage);
                AssertionUtils.AreNearlyEqual(Vector3.zero, self.AnchorMin, EPSILON, errorMessage);
                AssertionUtils.AreNearlyEqual(Vector3.one, self.AnchorMax, EPSILON, errorMessage);
                AssertionUtils.AreNearlyEqual(offsetMin, _offsetMin, EPSILON, errorMessage);
                AssertionUtils.AreNearlyEqual(offsetMax, _offsetMax, EPSILON, errorMessage);
            }
        }
        #endregion

        #region LayoutInfo Property
        /// <summary>
        /// <seealso cref="LayoutTargetObject.LayoutInfo"/>
        /// </summary>
        [Test]
        public void LayoutInfoPropertyPasses()
        {
            var target = new LayoutTargetObject();
            var other = new LayoutInfo();
            target.LayoutInfo.MaxSize = other.MaxSize = Vector3.one * 100f; // Callbackの呼び出しのみをテストするので、MinSizeより下回らないようにしています。

            var callCounter = 0;
            (ILayoutTarget self, LayoutInfo.ValueKind kinds) recievedValues = default;
            target.OnChangedLayoutInfo.Add((_self, _kinds) => {
                callCounter++;
                recievedValues = (_self, _kinds);
            });
            //例外が発生しても他のコールバックは実行されるようにしてください。
            target.OnChangedLayoutInfo.Add((_, __) => throw new System.Exception());

            var flagCombination = IndexCombinationEnumerable.GetFlagEnumCombination(
                System.Enum.GetValues(typeof(LayoutInfo.ValueKind)).OfType<LayoutInfo.ValueKind>()
            );
            foreach (var kinds in flagCombination)
            {
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
                    other.SizeGrowInGroup = other.SizeGrowInGroup + 1f;
                if (0 != (kinds & LayoutInfo.ValueKind.OrderInGroup))
                    other.OrderInGroup = other.OrderInGroup + 1;

                callCounter = 0;
                recievedValues = default;

                target.LayoutInfo.Assign(other); //test point

                Assert.AreSame(target, recievedValues.self, errorMessage);
                Assert.AreEqual(kinds, recievedValues.kinds, errorMessage);
            }
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.LocalSize"/>
        /// <seealso cref="LayoutInfo.MinSize"/>
        /// <seealso cref="LayoutInfo.MaxSize"/>
        /// </summary>
        [Test, Description("LayoutInfo#MinSize/MaxSizeが指定されている時はILayoutTarget#LocalSizeに制限がかかるようにするテスト")]
        public void LayoutInfoMinMaxSizePasses()
        {
            var layoutTarget = new LayoutTargetObject();
            layoutTarget.SetLocalSize(Vector3.one * 100f);

            var minValue = -10f;
            var maxValue = 1000f;
            var rnd = new System.Random();
            for(var i=0; i<1000; ++i)
            {
                var min = layoutTarget.LayoutInfo.MinSize;
                var max = layoutTarget.LayoutInfo.MaxSize;
                var localSize = layoutTarget.LocalSize;
                if ((rnd.Next() % 2) == 0)
                {
                    min = new Vector3(
                        rnd.Range(minValue, maxValue),
                        rnd.Range(minValue, maxValue),
                        rnd.Range(minValue, maxValue)
                    );
                    max = new Vector3(
                        rnd.Range(minValue, maxValue),
                        rnd.Range(minValue, maxValue),
                        rnd.Range(minValue, maxValue)
                    );
                    var tmp = Vector3.Min(min, max);
                    max = Vector3.Max(min, max);
                    min = tmp;

                    layoutTarget.LayoutInfo.SetMinMaxSize(min, max);
                }
                else
                {
                    localSize = new Vector3(
                        rnd.Range(0, maxValue),
                        rnd.Range(0, maxValue),
                        rnd.Range(0, maxValue)
                    );
                    layoutTarget.SetLocalSize(localSize);
                }

                //LayoutInfo#UNFIXED_VALUEを考慮に入れた処理
                var correctLocalSize = localSize;
                if (min.x >= 0) correctLocalSize.x = Max(correctLocalSize.x, min.x);
                if (min.y >= 0) correctLocalSize.y = Max(correctLocalSize.y, min.y);
                if (min.z >= 0) correctLocalSize.z = Max(correctLocalSize.z, min.z);

                if (max.x >= 0) correctLocalSize.x = Min(correctLocalSize.x, max.x);
                if (max.y >= 0) correctLocalSize.y = Min(correctLocalSize.y, max.y);
                if (max.z >= 0) correctLocalSize.z = Min(correctLocalSize.z, max.z);

                var errorMessage = $"Fail Test... LocalSize={localSize:F4}, MinSize={layoutTarget.LayoutInfo.MinSize:F4}, MaxSize={layoutTarget.LayoutInfo.MaxSize:F4}";
                AssertionUtils.AreNearlyEqual(correctLocalSize, layoutTarget.LocalSize, LayoutDefines.POS_NUMBER_PRECISION, errorMessage);
            }
        }

        /// <summary>
        /// <seealso cref="ILayoutTarget.Parent"/>
        /// <seealso cref="ILayoutTarget.LocalSize"/>
        /// <seealso cref="LayoutInfo.MinSize"/>
        /// <seealso cref="LayoutInfo.MaxSize"/>
        /// </summary>
        [Test, Description("親のLayoutInfo#MinSize/MaxSizeが指定されている時のAnchorArea周りの挙動テスト")]
        public void LayoutInfoLayoutSize_InParentPasses()
        {
            var parent = new LayoutTargetObject();
            parent.SetLocalSize(Vector3.one * 100f);

            var layoutTarget = new LayoutTargetObject();
            layoutTarget.SetParent(parent);
            layoutTarget.UpdateAnchorParam(Vector3.zero, Vector3.one, Vector3.zero, Vector3.zero);

            var minValue = -10f;
            var maxValue = 1000f;
            var rnd = new System.Random();
            for (var i = 0; i < 1000; ++i)
            {
                var layoutSize = parent.LayoutInfo.LayoutSize;
                var min = parent.LayoutInfo.MinSize;
                var max = parent.LayoutInfo.MaxSize;
                var localSize = parent.LocalSize;

                var offset = layoutTarget.AnchorOffsetMinMax();
                var index = rnd.Next() % 3;
                if (index == 0)
                {
                    min = new Vector3(
                        rnd.Range(minValue, maxValue),
                        rnd.Range(minValue, maxValue),
                        rnd.Range(minValue, maxValue)
                    );
                    max = new Vector3(
                        rnd.Range(minValue, maxValue),
                        rnd.Range(minValue, maxValue),
                        rnd.Range(minValue, maxValue)
                    );
                    var tmp = Vector3.Min(min, max);
                    max = Vector3.Max(min, max);
                    min = tmp;

                    parent.LayoutInfo.SetMinMaxSize(min, max);
                }
                else if(index == 1)
                {
                    localSize = new Vector3(
                        rnd.Range(0, maxValue),
                        rnd.Range(0, maxValue),
                        rnd.Range(0, maxValue)
                    );
                    parent.SetLocalSize(localSize);
                }
                else
                {
                    layoutSize = new Vector3(
                        rnd.Range(minValue, maxValue),
                        rnd.Range(minValue, maxValue),
                        rnd.Range(minValue, maxValue)
                    );
                    parent.LayoutInfo.LayoutSize = layoutSize;
                }

                //LayoutInfo#UNFIXED_VALUEを考慮に入れた処理
                var correctLocalSize = parent.LocalSize;
                if (min.x >= 0) correctLocalSize.x = Max(correctLocalSize.x, min.x);
                if (min.y >= 0) correctLocalSize.y = Max(correctLocalSize.y, min.y);
                if (min.z >= 0) correctLocalSize.z = Max(correctLocalSize.z, min.z);

                if (layoutSize.x >= 0) correctLocalSize.x = Min(correctLocalSize.x, layoutSize.x);
                if (layoutSize.y >= 0) correctLocalSize.y = Min(correctLocalSize.y, layoutSize.y);
                if (layoutSize.z >= 0) correctLocalSize.z = Min(correctLocalSize.z, layoutSize.z);

                var newLine = System.Environment.NewLine;
                var errorMessage = $"Fail Test{i}...{index}{newLine}"
                    + $"-- parent LocalSize={localSize:F4},{newLine}"
                    + $"-- parent LayoutSize={parent.LayoutInfo.LayoutSize:F4},{newLine}"
                    + $"-- parent MinSize={parent.LayoutInfo.MinSize:F4},{newLine}"
                    + $"-- parent MaxSize={parent.LayoutInfo.MaxSize:F4},{newLine}"
                    + $"-- parent UseLayoutSize={parent.LayoutInfo.GetLayoutSize(parent):F4},{newLine}"
                    + $"-- prev offset={offset:F4},{newLine}"
                    + $"-- offsetMin={layoutTarget.AnchorOffsetMinMax().offsetMin:F4},{newLine}"
                    + $"-- offsetMax={layoutTarget.AnchorOffsetMinMax().offsetMax:F4},{newLine}";
                AssertionUtils.AreNearlyEqual(correctLocalSize, layoutTarget.LocalSize, LayoutDefines.POS_NUMBER_PRECISION, errorMessage);
            }
        }
        #endregion

        #region Pivot Property
        /// <summary>
        /// <seealso cref="LayoutTargetObject.Pivot"/>
        /// <seealso cref="LayoutTargetObject.OnChangedPivot"/>
        /// </summary>
        [Test]
        public void PivotPropertyPasses()
        {
            var target = new LayoutTargetObject();
            var correctLocalSize = Vector3.one * 100f;
            target.UpdateLocalSize(correctLocalSize, Vector3.zero);

            var callCounter = 0;
            (ILayoutTarget self, Vector3 prevPivot) recievedValues = default;
            target.OnChangedPivot.Add((_s, _p) => {
                callCounter++;
                recievedValues = (_s, _p);
            });

            var rnd = new System.Random();
            for(var i=0; i<1000; ++i)
            {
                var prevPivot = target.Pivot;

                // test point
                var pivot = new Vector3(
                    rnd.Range(0f, 1f),
                    rnd.Range(0f, 1f),
                    rnd.Range(0f, 1f)
                );

                callCounter = 0;
                recievedValues = default;

                var (offsetMin, offsetMax) = target.AnchorOffsetMinMax();
                //test point
                Assert.DoesNotThrow(() => target.Pivot = pivot);

                var errorMessage = $"{System.Environment.NewLine}-- Fail test... pivot={pivot.ToString("F4")} prevPivot={prevPivot.ToString("F4")}";

                Assert.AreEqual(1, callCounter, errorMessage);
                Assert.AreSame(target, recievedValues.self, errorMessage);
                AssertionUtils.AreNearlyEqual(prevPivot, recievedValues.prevPivot, EPSILON_POS, errorMessage);

                AssertionUtils.AreNearlyEqual(pivot, target.Pivot, EPSILON_POS, errorMessage);

                AssertionUtils.AreNearlyEqual(correctLocalSize, target.LocalSize, LayoutDefines.POS_NUMBER_PRECISION, errorMessage + $" localSize={target.LocalSize.ToString("F4")}{System.Environment.NewLine} Not Change LocalSize...");

                var correctOffset = -offsetMin.Mul(Vector3.one - pivot) + offsetMax.Mul(pivot);
                AssertionUtils.AreNearlyEqual(correctOffset, target.Offset, EPSILON_POS, errorMessage + $" offsetMin={offsetMin.ToString("F4")} offsetMax={offsetMax.ToString("F4")}{System.Environment.NewLine} Adjest Anchor OffsetMin/Max...");
            }
        }

        /// <summary>
        /// <seealso cref="LayoutTargetObject.OnChangedPivot"/>
        /// </summary>
        [Test]
        public void OnChangedPivot_WhenThowExceptionPasses()
        {
            var target = new LayoutTargetObject();
            target.OnChangedPivot.Add((_s, _p) => throw new System.Exception());

            var pivot = Vector3.one;
            Assert.DoesNotThrow(() => target.Pivot = pivot);

            AssertionUtils.AreNearlyEqual(pivot, target.Pivot, EPSILON_POS);
        }
        #endregion
    }
}

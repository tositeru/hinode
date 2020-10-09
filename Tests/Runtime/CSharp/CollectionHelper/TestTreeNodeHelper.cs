using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.CSharp.CollectionHelper
{
    /// <summary>
    /// テスト一覧
    /// ## Constructer
    /// - defualt
    /// - with value
    /// ## Children
    /// - indexer
    /// - property
    /// - ChildCount
    /// - OnAdded
    /// - OnRemoved
    /// - OnMoved
    /// - OnChagendCount
    /// - OnCleared
    /// ## Parent
    /// - property
    /// - OnChangedParent callback
    /// - SiblingIndex
    /// ## Value
    /// - property
    /// ## IEnumerable
    /// - IEnumerable interface
    /// - GetValueEnumerable()
    /// - GetParentEnumerable()
    /// <seealso cref="TreeNodeHelper"/>
    /// </summary>
    public class TestTreeNodeHelper
    {
        const int ORDER_CONSTRUCTOR = 0;
        const int ORDER_CHILDREN = ORDER_CONSTRUCTOR + 100;
        const int ORDER_CHILDREN_CALLBACK = ORDER_CHILDREN + 50;
        const int ORDER_PARENT_PROPERTY = ORDER_CONSTRUCTOR + 100;
        const int ORDER_VALUE_PROPERTY = ORDER_CONSTRUCTOR + 100;
        const int ORDER_IENUMERABLE = ORDER_CONSTRUCTOR + 100;

        #region Constructer
        [Test, Order(ORDER_CONSTRUCTOR), Description("Constructorのテスト")]
        public void Constructor_Passes()
        {
            var node = new TreeNodeHelper<int>();

            Assert.IsNull(node.Parent);
            Assert.IsFalse(node.Children.Any());
            Assert.AreEqual(0, node.ChildCount);
            Assert.AreEqual(default(int), node.Value);
        }

        [Test, Order(ORDER_CONSTRUCTOR), Description("Constructor(T value)のテスト")]
        public void Constructor_WithValue_Passes()
        {
            var value = 100;
            var node = new TreeNodeHelper<int>(value);

            Assert.IsNull(node.Parent);
            Assert.IsFalse(node.Children.Any());
            Assert.AreEqual(0, node.ChildCount);
            Assert.AreEqual(value, node.Value);
        }
        #endregion

        #region Children
        [Test, Order(ORDER_CHILDREN), Description("Childrenプロパティのテスト")]
        public void Children_Property_Passes()
        {
            var node = new TreeNodeHelper<int>();
            {
                node.Children.Add(
                    TreeNodeHelper<int>.Create(100)
                    , TreeNodeHelper<int>.Create(200)
                    , TreeNodeHelper<int>.Create(300));

                AssertionUtils.AssertEnumerable(
                    new int[] {
                        100, 200, 300
                    }
                    , node.Children.Select(_c => _c.Value)
                    , ""
                );
                Assert.IsTrue(node.Children.All(_c => _c.Parent == node));
            }

            {
                node.Children.RemoveAt(1);
                AssertionUtils.AssertEnumerable(
                    new int[] {
                                    100, 300
                    }
                    , node.Children.Select(_c => _c.Value)
                    , ""
                );
                Assert.IsTrue(node.Children.All(_c => _c.Parent == node));
            }
        }

        [Test, Order(ORDER_CHILDREN), Description("this[int index]のテスト")]
        public void Children_Indexer_Passes()
        {
            var node = new TreeNodeHelper<int>();
            node.Children.Add(
                TreeNodeHelper<int>.Create(100)
                , TreeNodeHelper<int>.Create(200)
                , TreeNodeHelper<int>.Create(300));

            for(var i=0; i<node.Children.Count; ++i)
            {
                Assert.AreSame(node.Children[i], node[i]);
            }

            node[1] = TreeNodeHelper<int>.Create(-100);
            Assert.AreEqual(-100, node[1].Value);
        }

        [Test, Order(ORDER_CHILDREN), Description("ChildCountプロパティのテスト")]
        public void Children_ChildCount_Passes()
        {
            var node = new TreeNodeHelper<int>();

            for(var i=0; i<5; ++i)
            {
                node.Children.Add(TreeNodeHelper<int>.Create(0));

                Assert.AreEqual(i+1, node.ChildCount);
                Assert.AreEqual(node.Children.Count, node.ChildCount);
            }
        }

        [Test, Order(ORDER_CHILDREN_CALLBACK), Description("OnAddedChildコールバックのテスト")]
        public void Children_OnAddedChild_Passes()
        {
            var node = new TreeNodeHelper<int>();
            int callCounter = 0;
            (TreeNodeHelper<int> self, TreeNodeHelper<int> child, int index) recievedValues = default;
            node.OnAddedChild.Add((_s, _c, _i) => {
                callCounter++;
                recievedValues = (_s, _c, _i);
            });
            node.OnAddedChild.Add((_, __, ___) => throw new System.Exception());

            Assert.DoesNotThrow(() => node.Children.Add(node.CreateNode(0)));

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(node, recievedValues.self);
            Assert.AreEqual(0, recievedValues.index);
            Assert.AreSame(node[0], recievedValues.child);
        }

        [Test, Order(ORDER_CHILDREN_CALLBACK), Description("OnRemovedChildコールバックのテスト")]
        public void Children_OnRemovedChild_Passes()
        {
            var node = new TreeNodeHelper<int>();
            node.Children.Add(node.CreateNode(0), node.CreateNode(1));

            int callCounter = 0;
            (TreeNodeHelper<int> self, TreeNodeHelper<int> child, int index) recievedValues = default;
            node.OnRemovedChild.Add((_s, _c, _i) => {
                callCounter++;
                recievedValues = (_s, _c, _i);
            });
            node.OnRemovedChild.Add((_, __, ___) => throw new System.Exception());

            var removeIndex = node.ChildCount-1;
            var removeChild = node[removeIndex];
            Assert.DoesNotThrow(() => node.Children.RemoveAt(removeIndex));

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(node, recievedValues.self);
            Assert.AreEqual(removeIndex, recievedValues.index);
            Assert.AreSame(removeChild, recievedValues.child);
        }

        [Test, Order(ORDER_CHILDREN_CALLBACK), Description("OnMovedChildコールバックのテスト")]
        public void Children_OnMovedChild_Passes()
        {
            var node = new TreeNodeHelper<int>();
            node.Children.Add(
                node.CreateNode(0)
                , node.CreateNode(1)
                , node.CreateNode(2));

            int callCounter = 0;
            (TreeNodeHelper<int> self, TreeNodeHelper<int> child, int fromIndex, int toIndex) recievedValues = default;
            node.OnMovedChild.Add((_s, _c, _from, _to) => {
                callCounter++;
                recievedValues = (_s, _c, _from, _to);
            });
            node.OnMovedChild.Add((_, __, ___, ____) => throw new System.Exception());

            Debug.Log($"test -- childCount={node.ChildCount}");
            var moveFromIndex = 1;
            var moveToIndex = 2;
            var fromChild = node[moveFromIndex];
            Assert.DoesNotThrow(() => node.Children.MoveTo(moveFromIndex, moveToIndex));

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(node, recievedValues.self);
            Assert.AreEqual(moveFromIndex, recievedValues.fromIndex);
            Assert.AreEqual(moveToIndex, recievedValues.toIndex);
            Assert.AreSame(node[moveToIndex], recievedValues.child);
            Assert.AreSame(fromChild, recievedValues.child);
        }

        [Test, Order(ORDER_CHILDREN_CALLBACK), Description("OnChangedChildCountコールバックのテスト")]
        public void Children_OnChangedChildCount_Passes()
        {
            var node = new TreeNodeHelper<int>();

            int callCounter = 0;
            (TreeNodeHelper<int> self, IReadOnlyListHelper<TreeNodeHelper<int>> children, int count) recievedValues = default;
            node.OnChangedChildCount.Add((_s, _children, _c) => {
                callCounter++;
                recievedValues = (_s, _children, _c);
            });
            node.OnChangedChildCount.Add((_, __, ___) => throw new System.Exception());

            for (var i=0; i<5; ++i)
            {
                callCounter = 0;
                recievedValues = default;

                Assert.DoesNotThrow(() => node.Children.Add(node.CreateNode(0)));

                Assert.AreEqual(1, callCounter);
                Assert.AreSame(node, recievedValues.self);
                Assert.AreSame(node.Children, recievedValues.children);
                Assert.AreEqual(i+1, recievedValues.count);
            }

            for (var i = 0; node.ChildCount > 0; ++i)
            {
                callCounter = 0;
                recievedValues = default;

                Assert.DoesNotThrow(() => node.Children.RemoveAt(0));

                Assert.AreEqual(1, callCounter);
                Assert.AreSame(node, recievedValues.self);
                Assert.AreSame(node.Children, recievedValues.children);
                Assert.AreEqual(node.Children.Count, recievedValues.count);
            }
        }

        [Test, Order(ORDER_CHILDREN_CALLBACK), Description("OnClearedChildrenコールバックのテスト")]
        public void Children_OnClearedChildren_Passes()
        {
            var node = new TreeNodeHelper<int>();

            int callCounter = 0;
            TreeNodeHelper<int> recievedValues = default;
            node.OnClearedChildren.Add((_s) => {
                callCounter++;
                recievedValues = _s;
            });
            node.OnClearedChildren.Add((_) => throw new System.Exception());

            node.Children.Add(node.CreateNode(0), node.CreateNode(1));
            Assert.DoesNotThrow(() => node.Children.Clear());

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(node, recievedValues);
        }
        #endregion

        #region Parent
        [Test, Order(ORDER_PARENT_PROPERTY), Description("Parentプロパティのテスト")]
        public void Parent_Property_Passes()
        {
            var node = new TreeNodeHelper<int>();
            Assert.IsNull(node.Parent);

            var parent = node.CreateNode(100);
            node.Parent = parent;
            Assert.AreSame(parent, node.Parent);

            node.Parent = null;
            Assert.IsNull(node.Parent);
        }

        delegate void Callback(int arg);
        [Test, Order(ORDER_PARENT_PROPERTY+1), Description("OnChangedParentコールバックが呼び出されるかのテスト")]
        public void Parent_OnChangedParent_Passes()
        {
            {
                var callbacks = new SmartDelegate<Callback>();
                var c = 0;
                callbacks.Add((_) => c++);
                callbacks.Add((_) => { });
                callbacks.SafeDynamicInvoke(0, () => "test fail...");
            }

            var node = new TreeNodeHelper<int>();

            int callCounter = 0;
            (TreeNodeHelper<int> self, TreeNodeHelper<int> parent, TreeNodeHelper<int> prevParent) recievedValue = default;
            node.OnChangedParent.Add((_s, _p, _prev) => {
                callCounter++;
                recievedValue = (_s, _p, _prev);
            });
            node.OnChangedParent.Add((_, __, ___) => throw new System.Exception());

            var parent = node.CreateNode(100);
            Assert.DoesNotThrow(() => {
                node.Parent = parent;
            });

            Assert.AreEqual(1, callCounter);
            Assert.AreSame(node, recievedValue.self);
            Assert.AreSame(parent, recievedValue.parent);
            Assert.IsNull(recievedValue.prevParent);
        }

        [Test, Order(ORDER_PARENT_PROPERTY + 1), Description("SiblingIndexプロパティのテスト")]
        public void SiblingIndex_Passes()
        {
            var parent = new TreeNodeHelper<int>();

            parent.Children.Add(
                parent.CreateNode(100),
                parent.CreateNode(200),
                parent.CreateNode(300)
            );

            for(var i=0; i<parent.ChildCount; ++i)
            {
                Assert.AreEqual(i, parent[i].SiblingIndex);
            }
        }
        #endregion

        #region Value
        [Test, Order(ORDER_VALUE_PROPERTY), Description("Valueプロパティのテスト")]
        public void Value_Property_Passes()
        {
            var node = new TreeNodeHelper<int>(100);

            Assert.AreEqual(100, node.Value);
            node.Value = 200;
            Assert.AreEqual(200, node.Value);
        }
        #endregion

        #region IEnumerable
        [Test, Order(ORDER_IENUMERABLE), Description("デフォルトのEnumerableのテスト。自身とその階層に所属する全ての子ノードを辿るEnumerableになります")]
        public void IEnumerable_Passes()
        {
            var node = new TreeNodeHelper<int>();
            node.Children.Add(
                node.CreateNode(10)
                , node.CreateNode(20)
                , node.CreateNode(30));

            node[0].Children.Add(
                node.CreateNode(100)
                , node.CreateNode(200));

            node[2].Children.Add(
                node.CreateNode(100));

            AssertionUtils.AssertEnumerable(
                new TreeNodeHelper<int>[] {
                    node,
                    node.Children[0], node.Children[0][0], node.Children[0][1],
                    node.Children[1],
                    node.Children[2], node.Children[2][0]
                }
                , node
                , ""
            );
        }

        [Test, Order(ORDER_IENUMERABLE+1), Description("Valueプロパティを直接返すEnumerableのテスト")]
        public void IEnumerable_GetValueEnueramble_Passes()
        {
            var node = new TreeNodeHelper<int>();
            node.Children.Add(
                node.CreateNode(10
                    , node.CreateNode(100)
                    , node.CreateNode(200)
                )
                , node.CreateNode(20)
                , node.CreateNode(30
                    , node.CreateNode(100)
                )
            );

            AssertionUtils.AssertEnumerable(
                new TreeNodeHelper<int>[] {
                    node,
                    node.Children[0], node.Children[0][0], node.Children[0][1],
                    node.Children[1],
                    node.Children[2], node.Children[2][0]
                }.Select(_n => _n.Value)
                , node.GetValueEnumerable()
                , ""
            );
        }

        [Test, Order(ORDER_IENUMERABLE + 1), Description("ルートノードまでたどり着くまで親を辿っていくEnumerableのテスト")]
        public void IEnumerable_GetParentEnueramble_Passes()
        {
            var node = new TreeNodeHelper<int>();
            node.Parent = new TreeNodeHelper<int>(10);
            node.Parent.Parent = new TreeNodeHelper<int>(100);
            node.Parent.Parent.Parent = new TreeNodeHelper<int>(1000);

            AssertionUtils.AssertEnumerable(
                new TreeNodeHelper<int>[] {
                    node.Parent,
                    node.Parent.Parent,
                    node.Parent.Parent.Parent
                }
                , node.GetParentEnumerable()
                , ""
            );
        }
        #endregion
    }
}

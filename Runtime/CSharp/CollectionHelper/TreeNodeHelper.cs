using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode
{
    public static class TreeNodeHelperCallback<T>
    {
        public delegate void OnChangedParent(TreeNodeHelper<T> self, TreeNodeHelper<T> parent, TreeNodeHelper<T> prevParent);

        public delegate void OnAddedChild(TreeNodeHelper<T> self, TreeNodeHelper<T> child, int index);
        public delegate void OnMovedChild(TreeNodeHelper<T> self, TreeNodeHelper<T> child, int fromIndex, int toIndex);
        public delegate void OnRemovedChild(TreeNodeHelper<T> self, TreeNodeHelper<T> child, int index);
        public delegate void OnClearedChildren(TreeNodeHelper<T> self);

        public delegate void OnChangedChildCount(TreeNodeHelper<T> self, IReadOnlyListHelper<TreeNodeHelper<T>> children, int count);
    }

    /// <summary>
    /// Treeデータ構造を表すクラス。
    ///
    /// IEnumerableでの探索順は自身を先頭にした深さ優先の子要素になります。
    /// 
    /// このクラスを継承する際は以下の点に注意してください。
    /// - Valueに自身の参照を設定すること
    ///
    /// <code>
    /// class Test : TreeNodeHelper<Test>
    /// {
    ///     public Test()
    ///     {
    ///         base.Value = this; // <- Self reference must set to TreeNodeHelper<T>#Value!!!
    ///     }
    ///     // ....
    /// }
    /// </code>
    /// <seealso cref="Hinode.Tests.CSharp.CollectionHelper"/>
    /// </summary>
    public class TreeNodeHelper<T> : IEnumerable<TreeNodeHelper<T>>, IEnumerable
    {
        public static TreeNodeHelper<T> Create(T value, params TreeNodeHelper<T>[] children)
            => Create(value, children.AsEnumerable());
        public static TreeNodeHelper<T> Create(T value, IEnumerable<TreeNodeHelper<T>> children)
        {
            var node = new TreeNodeHelper<T>(value);
            node.Children.Add(children);
            return node;
        }

        public TreeNodeHelper<T> CreateNode(T value, params TreeNodeHelper<T>[] children)
            => Create(value, children.AsEnumerable());
        public TreeNodeHelper<T> CreateNode(T value, IEnumerable<TreeNodeHelper<T>> children)
            => Create(value, children);

        SmartDelegate<TreeNodeHelperCallback<T>.OnChangedParent> _onChangedParent = new SmartDelegate<TreeNodeHelperCallback<T>.OnChangedParent>();
        SmartDelegate<TreeNodeHelperCallback<T>.OnAddedChild> _onAddedChild = new SmartDelegate<TreeNodeHelperCallback<T>.OnAddedChild>();
        SmartDelegate<TreeNodeHelperCallback<T>.OnMovedChild> _onMovedChild = new SmartDelegate<TreeNodeHelperCallback<T>.OnMovedChild>();
        SmartDelegate<TreeNodeHelperCallback<T>.OnRemovedChild> _onRemovedChild = new SmartDelegate<TreeNodeHelperCallback<T>.OnRemovedChild>();
        SmartDelegate<TreeNodeHelperCallback<T>.OnClearedChildren> _onClearedChildren = new SmartDelegate<TreeNodeHelperCallback<T>.OnClearedChildren>();
        SmartDelegate<TreeNodeHelperCallback<T>.OnChangedChildCount> _onChangedChildCount = new SmartDelegate<TreeNodeHelperCallback<T>.OnChangedChildCount>();

        TreeNodeHelper<T> _parent = default;
        ListHelper<TreeNodeHelper<T>> _children = new ListHelper<TreeNodeHelper<T>>();

        public NotInvokableDelegate<TreeNodeHelperCallback<T>.OnChangedParent> OnChangedParent { get => _onChangedParent; }
        public NotInvokableDelegate<TreeNodeHelperCallback<T>.OnAddedChild> OnAddedChild { get => _onAddedChild; }
        public NotInvokableDelegate<TreeNodeHelperCallback<T>.OnMovedChild> OnMovedChild { get => _onMovedChild; }
        public NotInvokableDelegate<TreeNodeHelperCallback<T>.OnRemovedChild> OnRemovedChild { get => _onRemovedChild; }
        public NotInvokableDelegate<TreeNodeHelperCallback<T>.OnClearedChildren> OnClearedChildren { get => _onClearedChildren; }
        public NotInvokableDelegate<TreeNodeHelperCallback<T>.OnChangedChildCount> OnChangedChildCount { get => _onChangedChildCount; }

        T _value;
        public T Value
        {
            get => _value;
            set
            {
                _value = value;
            }
        }

        public TreeNodeHelper<T> Parent
        {
            get => _parent;
            set
            {
                if (_parent == value) return;

                var prev = _parent;
                _parent = value;
                if (prev != null)
                {
                    prev._children.Remove(this);
                }
                if (_parent != null)
                {
                    if(!_parent._children.Contains(this))
                    {
                        _parent._children.Add(this);
                    }
                }
                _onChangedParent.SafeDynamicInvoke(this, _parent, prev, () => "Fail in set Parent...");
            }
        }

        public ListHelper<TreeNodeHelper<T>> Children { get => _children; }
        public int ChildCount { get => _children.Count; }
        public int SiblingIndex
        {
            get
            {
                if (Parent == null) return -1;
                return Parent.Children.IndexOf(this);
            }
            set
            {
                if (Parent == null) return;

                var index = Parent.Children.IndexOf(this);
                if (0 <= value && index == value) return;
                Parent.Children.MoveTo(index, value);
            }
        }
        public TreeNodeHelper<T> this[int index]
        {
            get => _children[index];
            set => _children[index] = value;
        }

        #region Constructer
        public TreeNodeHelper()
        {
            _children.OnAdded.Add((_c, _index) => {
                if (_c.Parent != this)
                {
                    _c.Parent = this;
                    _onAddedChild.SafeDynamicInvoke(this, _c, _index, () => "Fail in Children#OnAdded...");
                }
            });
            _children.OnRemoved.Add((_c, _index) => {
                if (_c.Parent == this)
                {
                    _c.Parent = null;
                    _onRemovedChild.SafeDynamicInvoke(this, _c, _index, () => "Fail in Children#OnRemoved...");
                }
            });
            _children.OnMoved.Add((_c, _from, _to) => {
                _onMovedChild.SafeDynamicInvoke(this, _c, _from, _to, () => $"Fail in Children#OnMoved... from={_from}, to={_to}");
            });
            _children.OnChangedCount.Add((_self, _count) => {
                _onChangedChildCount.SafeDynamicInvoke(this, _self, _count, () => "Fail in Chldren#OnChangedCount...");
            });
            _children.OnCleared.Add(() => {
                _onClearedChildren.SafeDynamicInvoke(this, () => "Fail in Chldren#Clear...");
            });
        }

        public TreeNodeHelper(T value)
            : this()
        {
            Value = value;
        }
        #endregion

        #region IEnuerable<>
        public IEnumerable<T> GetValueEnumerable()
        {
            return new HierachyEnumerable(this).Select(_n => _n.Value);
        }

        public IEnumerator<TreeNodeHelper<T>> GetEnumerator()
        {
            return new HierachyEnumerable(this).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
             => GetEnumerator();

        class HierachyEnumerable : IEnumerable<TreeNodeHelper<T>>, IEnumerable
        {
            TreeNodeHelper<T> _target;
            public HierachyEnumerable(TreeNodeHelper<T> target)
            {
                Assert.IsNotNull(target);
                _target = target;
            }

            public IEnumerator<TreeNodeHelper<T>> GetEnumerator()
            {
                var it = _target;
                while (it != null)
                {
                    Debug.Log($"test -- current={it.Value} HasParent={it.Parent != null}");
                    yield return it;
                    it = GetNext(it);
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            TreeNodeHelper<T> GetNext(TreeNodeHelper<T> now, int nextChildIndex = 0)
            {
                if (nextChildIndex >= now.ChildCount)
                {
                    return now == _target
                        ? null
                        : GetNext(now.Parent, now.SiblingIndex + 1);
                }
                if (now.ChildCount > 0)
                {
                    return now[nextChildIndex];
                }
                if (now == _target)
                {
                    return null;
                }
                if (now.SiblingIndex < now.Parent.ChildCount - 1)
                {
                    return now.Parent[now.SiblingIndex + 1];
                }
                return GetNext(now.Parent, now.SiblingIndex + 1);
            }
        }

        public IEnumerable<TreeNodeHelper<T>> GetParentEnumerable()
        {
            return new ParentEnumerable(this);
        }

        class ParentEnumerable : IEnumerable<TreeNodeHelper<T>>, IEnumerable
        {
            TreeNodeHelper<T> _target;
            public ParentEnumerable(TreeNodeHelper<T> target)
            {
                Assert.IsNotNull(target);
                _target = target;
            }

            public IEnumerator<TreeNodeHelper<T>> GetEnumerator()
            {
                var p = _target.Parent;
                while(p != null)
                {
                    yield return p;
                    p = p.Parent;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }

        #endregion
    }
}

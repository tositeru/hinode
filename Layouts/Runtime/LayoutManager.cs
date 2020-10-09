using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Hinode.Layouts
{
    /// <summary>
	/// <seealso cref="ILayout"/>
	/// <seealso cref="ILayoutTarget"/>
	/// </summary>
    public class LayoutManager
    {
        ListHelper<Group> _groups = new ListHelper<Group>();
        public HashSetHelper<ILayoutTarget> LayoutTargets { get; } = new HashSetHelper<ILayoutTarget>();

        public IReadOnlyListHelper<Group> Groups { get => _groups; }

        public LayoutManager()
        {
            _groups.OnAdded.Add((_, __) => {
                _groups.Sort(Group.Comparer.Default);
            });
            _groups.OnRemoved.Add((_, __) => {
                _groups.Sort(Group.Comparer.Default);
            });

            LayoutTargets.OnAdded.Add((item) => {
                item.OnDisposed.Add(ILayoutOnDisposed);
            });
            LayoutTargets.OnRemoved.Add((item) => {
                item.OnDisposed.Remove(ILayoutOnDisposed);
            });
        }

        void ILayoutOnDisposed(ILayoutTarget layout)
        {
            LayoutTargets.Remove(layout);
        }

        public Group Entry(ILayoutTarget target, int priority=0)
        {
            var group = Groups.FirstOrDefault(
                _g => _g.ContainsInRootHierachy(target)
            );
            if(group == null)
            {
                group = new Group(target)
                {
                    Priority = priority
                };
                _groups.Add(group);
                group.OnDisposed.Add(GroupOnDisposed);
                group.OnChangedPriority.Add(GroupOnChangedPriority);
            }
            else
            {
                group.Add(target);
            }

            target.OnChangedParent.Add(ILayoutTargetOnChangedParent);
            return group;
        }

        public Group Entry(ILayoutTarget target, Group parentGroup)
        {
            var group = Entry(target, parentGroup.Priority+1);
            group.ParentGroup = parentGroup;
            return group;
        }

        public LayoutManager Exit(ILayoutTarget target)
        {
            var group = _groups.FirstOrDefault(_g => _g.Targets.Contains(target));
            if (group == null) return this;

            group.Remove(target);
            target.OnChangedParent.Remove(ILayoutTargetOnChangedParent);
            return this;
        }

        public void CaluculateLayouts()
        {
            foreach (var l in LayoutTargets)
            {
                foreach(var layout in l.Layouts.Where(_l => _l.DoChanged))
                {
                    layout.UpdateLayout();
                }
            }
        }

        #region Group Callbacks
        void GroupOnDisposed(Group group)
        {
            foreach (var t in group.Targets)
            {
                t.OnChangedParent.Remove(ILayoutTargetOnChangedParent);
            }
            _groups.Remove(group);
        }

        void GroupOnChangedPriority(Group group, int prevPriority)
        {
            _groups.Sort(Group.Comparer.Default);
        }
        #endregion

        #region ILayoutTarget Callbacks
        void ILayoutTargetOnChangedParent(ILayoutTarget layoutTarget, ILayoutTarget parent, ILayoutTarget prevParent)
        {
            var group = Groups.FirstOrDefault(_g => _g.Targets.Contains(layoutTarget));
            if (group == null) return;

            var containParent = group.Targets.Any(_t => _t == parent);
            var containPrevParent = group.Targets.Any(_t => _t == prevParent);
            if (!containParent && !containPrevParent) return;


        }
        #endregion

        /// <summary>
        /// LayoutManagerのILayoutTargetを管理する構成単位。
        /// 
        /// TargetsにあるILayoutTargetは必ずRootを基準にしたILayoutTarget階層と直接つながっているようにしてください。
        ///
        /// ILayoutTarget#ParentのParentがつながっているなど、
        /// Rootを基準にしたILayoutTarget階層と間接的につながっている場合は別のGroupとしてLayoutManagerに登録してください。
        /// </summary>
        public class Group : System.IDisposable
        {
            public delegate void OnDisposedCallback(Group self);
            public delegate void OnChangedParentCallback(Group self, Group prev);
            public delegate void OnChangedPriorityCallback(Group self, int prevPriotiry);

            SmartDelegate<OnDisposedCallback> _onDisposed = new SmartDelegate<OnDisposedCallback>();
            SmartDelegate<OnChangedParentCallback> _onChangedParent = new SmartDelegate<OnChangedParentCallback>();
            SmartDelegate<OnChangedPriorityCallback> _onChangedPriority = new SmartDelegate<OnChangedPriorityCallback>();

            List<ILayoutTarget> _targets = new List<ILayoutTarget>();
            Group _parentGroup = null;
            List<Group> _childGroups = new List<Group>();

            public NotInvokableDelegate<OnDisposedCallback> OnDisposed { get => _onDisposed; }
            public NotInvokableDelegate<OnChangedParentCallback> OnChangedParent { get => _onChangedParent; }
            public NotInvokableDelegate<OnChangedPriorityCallback> OnChangedPriority { get => _onChangedPriority; }

            int _priority = 0;
            public int Priority
            {
                get => _priority;
                set
                {
                    if (_priority == value) return;
                    var prev = _priority;
                    _priority = value;
                    _onChangedPriority.SafeDynamicInvoke(this, prev, () => "Fail in set Prioprity...", LayoutDefines.LOG_SELECTOR);
                }
            }
            public ILayoutTarget Root { get => _targets.FirstOrDefault(); }

            public IEnumerable<ILayoutTarget> Targets { get => _targets; }
            public Group ParentGroup
            {
                get => _parentGroup;
                set
                {
                    if (_parentGroup == value) return;
                    var prev = _parentGroup;
                    _parentGroup = value;
                    if (prev != null)
                    {
                        prev._childGroups.Remove(this);
                    }

                    if (_parentGroup != null)
                    {
                        Priority = _parentGroup.Priority - 1;
                        _parentGroup._childGroups.Add(this);
                    }

                    _onChangedParent.SafeDynamicInvoke(this, prev, () => "Fail in set ParentGroup", LayoutDefines.LOG_SELECTOR);
                }
            }

            public IEnumerable<Group> ChildGroups { get => _childGroups; }
            public IEnumerable<ILayout> CaluculationOrder
            {
                get
                {
                    //TODO 遅延計算レイアウトの対応
                    return _targets.SelectMany(_t => _t.Layouts);
                }
            }

            public Group(ILayoutTarget root)
            {
                InnerAdd(root, true);

                foreach(var t in root.GetHierarchyEnumerable())
                {
                    InnerAdd(t, t == root);
                }
            }

            public void CaluculateLayouts()
            {
                throw new System.NotImplementedException();
            }

            public bool ContainsInRootHierachy(ILayoutTarget target)
            {
                return target == Root || target.GetParentEnumerable().Any(_p => _p == Root);
            }

            internal bool Add(ILayoutTarget target)
            {
                return InnerAdd(target, false);
            }

            private bool InnerAdd(ILayoutTarget target, bool isRoot)
            {
                if (_targets.Contains(target)) return false;
                if (!isRoot && -1 == _targets.FindIndex(_t => _t == target.Parent)) return false;

                var insertIndex = _targets.FindLastIndex(_t => _t.Parent == target.Parent);
                if (insertIndex == -1)
                {
                    _targets.Add(target);
                }
                else
                {
                    _targets.Insert(insertIndex, target);
                }

                if (!target.Layouts.Any(_l => _l is ParentFollowLayout))
                {
                    target.AddLayout(new ParentFollowLayout());
                }

                target.OnDisposed.Add(ILayoutTargetOnDisposed);
                return true;
            }

            internal void Remove(ILayoutTarget target)
            {
                if (!_targets.Contains(target)) return;
                if (Root == target)
                {
                    _targets.Clear();
                }
                else
                {
                    _targets.Remove(target);
                }

                target.OnDisposed.Remove(ILayoutTargetOnDisposed);
            }

            #region ILayoutTarget Callbacks
            void ILayoutTargetOnDisposed(ILayoutTarget layoutTarget)
            {
                Remove(layoutTarget);

                if(Root == null)
                {
                    Dispose();
                }
            }
            #endregion

            #region System.IDisposable
            public void Dispose()
            {
                ParentGroup = null;
                foreach (var child in ChildGroups)
                {
                    child.ParentGroup = null;
                }

                foreach (var t in Targets)
                {
                    t.OnDisposed.Remove(ILayoutTargetOnDisposed);
                }
                _onDisposed.SafeDynamicInvoke(this, () => "Fail in Dispose...", LayoutDefines.LOG_SELECTOR);
            }
            #endregion

            internal class Comparer : IComparer<Group>
            {
                static public readonly Comparer Default = new Comparer();

                public int Compare(Group x, Group y)
                {
                    return -1 * x.Priority.CompareTo(y.Priority);
                }
            }
        }
    }
}

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

        public IReadOnlyListHelper<Group> Groups { get => _groups; }

        public LayoutManager()
        {
            _groups.OnAdded.Add((_, __) => {
                _groups.Sort(Group.Comparer.Default);
            });
            _groups.OnRemoved.Add((_, __) => {
                _groups.Sort(Group.Comparer.Default);
            });
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

            foreach(var t in target.GetHierarchyEnumerable())
            {
                t.OnChangedParent.Add(ILayoutTargetOnChangedParent);
            }
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
            if(!group.Targets.Any())
            {
                group.Dispose();
            }

            foreach(var t in target.GetHierarchyEnumerable())
            {
                t.OnChangedParent.Remove(ILayoutTargetOnChangedParent);
            }
            return this;
        }

        static readonly LayoutKind[] CALUCULATION_ORDER = new LayoutKind[]
        {
            LayoutKind.Normal,
            LayoutKind.Delay,
        };

        public void CaluculateLayouts()
        {
            foreach(var kind in CALUCULATION_ORDER)
            {
                foreach (var g in Groups)
                {
                    g.CaluculateLayouts(kind);
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

            var newParentGroup = Groups.FirstOrDefault(_g => _g.Targets.Contains(parent));
            var containParentInGroup = newParentGroup == group;
            var containPrevParentInGroup = group.Targets.Any(_t => _t == prevParent);
            if (!containParentInGroup && !containPrevParentInGroup) return;

            if(containPrevParentInGroup && !containParentInGroup)
            {//違うGroupに移動した時
                group.Remove(layoutTarget);

                if(newParentGroup != null)
                {//既存のGroupに移動したらそちらに登録する
                    newParentGroup.Add(layoutTarget);
                }
                else
                {//他のGroupに移動していなかったら新しくGroupを追加する
                    Entry(layoutTarget, group.Priority);
                }
            }
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
            public IEnumerable<ILayout> CaluculationOrder(LayoutKind layoutKind)
                => new CaluculationOrderEnumerable(this, layoutKind);

            public Group(ILayoutTarget root)
            {
                InnerAdd(root, true);

                foreach(var t in root.GetHierarchyEnumerable())
                {
                    InnerAdd(t, t == root);
                }
            }

            public void CaluculateLayouts(LayoutKind layoutKind)
            {
                foreach(var l in CaluculationOrder(layoutKind)
                    .Where(_l => _l.Validate() && _l.DoChanged))
                {
                    l.UpdateLayout();
                }
            }

            public bool ContainsInRootHierachy(ILayoutTarget target)
            {
                return target == Root || target.GetParentEnumerable().Any(_p => _p == Root);
            }

            internal bool Add(ILayoutTarget target)
            {
                bool isOK = true;
                foreach(var t in target.GetHierarchyEnumerable())
                {
                    isOK &= InnerAdd(t, false);
                }
                return isOK;
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
                    foreach(var t in target.GetHierarchyEnumerable())
                    {
                        _targets.Remove(t);
                    }
                }

                foreach(var t in target.GetHierarchyEnumerable())
                {
                    t.OnDisposed.Remove(ILayoutTargetOnDisposed);
                }
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
                _onDisposed.SafeDynamicInvoke(this, () => "Fail in Dispose...", LayoutDefines.LOG_SELECTOR);

                ParentGroup = null;
                while(ChildGroups.Any())
                {
                    ChildGroups.First().ParentGroup = null;
                }

                foreach (var t in Targets)
                {
                    t.OnDisposed.Remove(ILayoutTargetOnDisposed);
                }
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

            class CaluculationOrderEnumerable : IEnumerable<ILayout>, IEnumerable
            {
                Group _target;
                LayoutKind _kind;
                public CaluculationOrderEnumerable(Group target, LayoutKind layoutKind)
                {
                    _target = target;
                    _kind = layoutKind;
                }

                public IEnumerator<ILayout> GetEnumerator()
                {
                    foreach(var l in _target.Targets
                        .SelectMany(_t => _t.Layouts.Where(_l => _l.Kind == _kind)))
                    {
                        yield return l;
                    }
                }

                IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
            }

        }
    }
}

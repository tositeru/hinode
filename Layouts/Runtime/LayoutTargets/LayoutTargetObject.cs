using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static System.Math;

namespace Hinode.Layouts
{
    /// <summary>
	/// ILayoutTargetの標準実装
	/// 
	/// <seealso cref="ILayoutTarget"/>
	/// </summary>
    [System.Serializable]
    public class LayoutTargetObject : ILayoutTarget
    {
        SmartDelegate<ILayoutTargetOnDisposed> _onDisposed = new SmartDelegate<ILayoutTargetOnDisposed>();
        SmartDelegate<ILayoutTargetOnChangedParent> _onChangedParent = new SmartDelegate<ILayoutTargetOnChangedParent>();
        SmartDelegate<ILayoutTargetOnChangedChildren> _onChangedChildren = new SmartDelegate<ILayoutTargetOnChangedChildren>();
        SmartDelegate<ILayoutTargetOnChangedLocalPos> _onChangedLocalPos = new SmartDelegate<ILayoutTargetOnChangedLocalPos>();
        SmartDelegate<ILayoutTargetOnChangedLocalSize> _onChangedLocalSize = new SmartDelegate<ILayoutTargetOnChangedLocalSize>();

        SmartDelegate<ILayoutTargetOnChangedAnchorMinMax> _onChangedAnchorMinMax = new SmartDelegate<ILayoutTargetOnChangedAnchorMinMax>();
        SmartDelegate<ILayoutTargetOnChangedOffset> _onChangedOffset = new SmartDelegate<ILayoutTargetOnChangedOffset>();
        SmartDelegate<ILayoutTargetOnChangedPivot> _onChangedPivot = new SmartDelegate<ILayoutTargetOnChangedPivot>();
        SmartDelegate<ILayoutTargetOnChangedLayoutInfo> _onChangedLayoutInfo = new SmartDelegate<ILayoutTargetOnChangedLayoutInfo>();

        LayoutTargetObject _parent;
        HashSetHelper<LayoutTargetObject> _children = new HashSetHelper<LayoutTargetObject>();
        ListHelper<ILayout> _layouts = new ListHelper<ILayout>();

        [SerializeField] Vector3 _prevParentSize = Vector3.zero;

        [SerializeField] Vector3 _localPos;

        [SerializeField] Vector3 _localSize;
        [SerializeField] Vector3 _anchorMin;
        [SerializeField] Vector3 _anchorMax;
        [SerializeField] Vector3 _offset;
        [SerializeField] Vector3 _pivot = Vector3.one * 0.5f;

        public LayoutTargetObject()
        {
            _children.OnAdded.Add(_child => {
                _onChangedChildren.SafeDynamicInvoke(this, _child, ILayoutTargetOnChangedChildMode.Add, () => $"LayoutTargetObject#Add Child", LayoutDefines.LOG_SELECTOR);
            });
            _children.OnRemoved.Add(_child => {
                _onChangedChildren.SafeDynamicInvoke(this, _child, ILayoutTargetOnChangedChildMode.Remove, () => $"LayoutTargetObject#Remove Child", LayoutDefines.LOG_SELECTOR);
            });

            _layouts.OnAdded.Add((_item, _index) => {
                if (_item == null) return;
                _item.OnDisposed.Add(LayoutOnDisposed);
                _item.OnChangedOperationPriority.Add(LayoutOnChangedOperationPriority);
                _layouts.Sort(ILayoutDefaultComparer.Default);
            });
            _layouts.OnRemoved.Add((_item, _index) => {
                if (_item == null) return;
                _item.OnDisposed.Remove(LayoutOnDisposed);
                _item.OnChangedOperationPriority.Remove(LayoutOnChangedOperationPriority);
            });

            LayoutInfo.OnChangedValue.Add((_info, _kinds, _prevInfo) => {
                if(0 != (_kinds & LayoutInfo.ValueKind.MinSize)
                    || 0 != (_kinds & LayoutInfo.ValueKind.MaxSize))
                {
                    this.SetLocalSize(LocalSize);
                }
                else if(0 != (_kinds & LayoutInfo.ValueKind.LayoutSize))
                {
                    //子のサイズを更新する
                    foreach(var child in Children)
                    {
                        //変更前のOffsetMin/Maxを計算しています。
                        var parentLayoutSize = LocalSize;
                        if (_prevInfo.LayoutSize.x >= 0) parentLayoutSize.x = Min(_prevInfo.LayoutSize.x, parentLayoutSize.x);
                        if (_prevInfo.LayoutSize.y >= 0) parentLayoutSize.y = Min(_prevInfo.LayoutSize.y, parentLayoutSize.y);
                        if (_prevInfo.LayoutSize.z >= 0) parentLayoutSize.z = Min(_prevInfo.LayoutSize.z, parentLayoutSize.z);

                        var prevAnchorAreaSize = parentLayoutSize.Mul(child.AnchorMax - child.AnchorMin);
                        var max = prevAnchorAreaSize * 0.5f;
                        var min = -max;

                        var halfSize = child.LocalSize * 0.5f;
                        var (localMin, localMax) = (-halfSize + Offset, halfSize + Offset);
                        var (offsetMin, offsetMax) = (-(localMin - min), localMax - max);

                        child.UpdateAnchorParam(child.AnchorMin, child.AnchorMax, offsetMin, offsetMax);
                    }
                }

                _onChangedLayoutInfo.SafeDynamicInvoke(this, _kinds, () => $"LayoutInfo#OnChangedValue In LayoutTargetObject", LayoutDefines.LOG_SELECTOR);
            });
        }

        void LayoutOnDisposed(ILayout layout)
        {
            _layouts.Remove(layout);
        }

        void LayoutOnChangedOperationPriority(ILayout layout, int prevPriority)
        {
            _layouts.Sort(ILayoutDefaultComparer.Default);
        }

        public LayoutTargetObject SetParent(LayoutTargetObject parent)
        {
            if (_parent == parent) return this;

            if(_parent != null)
            {
                _parent.OnChangedLocalSize.Remove(ParentOnChangedLocalSize);
                _parent._children.Remove(this);
            }
            var prevParent = _parent;
            _parent = parent;

            if(_parent != null)
            {
                _parent._children.Add(this);
                _parent.OnChangedLocalSize.Add(ParentOnChangedLocalSize);
            }

            FollowParent();

            _onChangedParent.SafeDynamicInvoke(this, _parent, prevParent, () => $"LayoutTargetObject#SetParent", LayoutDefines.LOG_SELECTOR);
            return this;
        }

        /// <summary>
        /// 親のサイズが変更された時はAnchorMin/MaxとAnchorOffsetMin/Maxが変更されないようにパラメータを変更してください
        /// </summary>
        /// <param name="parent"></param>
        void ParentOnChangedLocalSize(ILayoutTarget parent, Vector3 prevLocalSize)
        {
            if (parent != Parent) return;

            if(IsAutoUpdate) FollowParent();
        }

        public void SetLayoutInfo(LayoutInfo layoutInfo)
        {
            if (LayoutInfo == layoutInfo) return;

            LayoutInfo.Assign(layoutInfo);
        }

        #region ILayoutTarget interface
        public NotInvokableDelegate<ILayoutTargetOnDisposed> OnDisposed { get => _onDisposed; }
        public NotInvokableDelegate<ILayoutTargetOnChangedParent> OnChangedParent { get => _onChangedParent; }
        public NotInvokableDelegate<ILayoutTargetOnChangedChildren> OnChangedChildren { get => _onChangedChildren; }
        public NotInvokableDelegate<ILayoutTargetOnChangedLocalPos> OnChangedLocalPos { get => _onChangedLocalPos; }
        public NotInvokableDelegate<ILayoutTargetOnChangedLocalSize> OnChangedLocalSize { get => _onChangedLocalSize; }
        public NotInvokableDelegate<ILayoutTargetOnChangedOffset> OnChangedOffset { get => _onChangedOffset; }
        public NotInvokableDelegate<ILayoutTargetOnChangedAnchorMinMax> OnChangedAnchorMinMax { get => _onChangedAnchorMinMax; }
        public NotInvokableDelegate<ILayoutTargetOnChangedPivot> OnChangedPivot { get => _onChangedPivot; }
        public NotInvokableDelegate<ILayoutTargetOnChangedLayoutInfo> OnChangedLayoutInfo { get => _onChangedLayoutInfo; }

        public bool IsAutoUpdate { get; set; } = true;

        public Vector3 PrevParentSize { get => _prevParentSize; }
        public ILayoutTarget Parent { get => _parent; }
        public IEnumerable<ILayoutTarget> Children { get => _children; }
        public int ChildCount { get => _children.Count; }

        public IReadOnlyListHelper<ILayout> Layouts { get => _layouts; }
        public LayoutInfo LayoutInfo { get; } = new LayoutInfo();

        public Vector3 LocalPos
        {
            get => _localPos;
            set 
            {
                var prevLocalPos = _localPos;
                _localPos = value;

                _onChangedLocalPos.SafeDynamicInvoke(this, prevLocalPos, () => $"LayoutTargetObject#Set LocalPos", LayoutDefines.LOG_SELECTOR);
            }
        }

        public Vector3 LocalSize
        {
            get => _localSize;
        }
        public Vector3 AnchorMin
        {
            get => _anchorMin;
        }
        public Vector3 AnchorMax
        {
            get => _anchorMax;
        }
        public Vector3 Offset
        {
            get => _offset;
        }

        /// <summary>
        /// Pivotの変更された場合は現在の位置を変更しないようにするため、UpdateAnchorParam()が呼びだされます。
        /// </summary>
        public Vector3 Pivot
        {
            get => _pivot;
            set
            {
                if (_pivot.AreNearlyEqual(value, LayoutDefines.NUMBER_PRECISION))
                    return;
                var (offsetMin, offsetMax) = this.AnchorOffsetMinMax();

                var prev = _pivot;
                _pivot = value;
                UpdateAnchorParam(AnchorMin, AnchorMax, offsetMin, offsetMax);
                _onChangedPivot.SafeDynamicInvoke(this, prev, () => $"LayoutTargetObject#Pivot", LayoutDefines.LOG_SELECTOR);
            }
        }

        public virtual void Dispose()
        {
            while(0 < _children.Count)
            {
                _children.Items.First().SetParent(null);
            }
            //_children.Clear();
            SetParent(null);

            _onDisposed.SafeDynamicInvoke(this, () => $"LayoutTargetObject#Dispose", LayoutDefines.LOG_SELECTOR);

            _onDisposed.Clear();
            _onChangedParent.Clear();
            _onChangedChildren.Clear();
            _onChangedLocalPos.Clear();
            _onChangedLocalSize.Clear();
            _onChangedOffset.Clear();
            _onChangedPivot.Clear();
            _onChangedLayoutInfo.Clear();
        }

        public void AddLayout(ILayout layout)
        {
            if (!_layouts.Contains(layout))
            {
                var insertIndex = _layouts.FindIndex((_l) => layout.OperationPriority >=_l.OperationPriority);
                if(insertIndex != -1)
                    _layouts.InsertTo(insertIndex, layout);
                else
                    _layouts.Add(layout);
            }
            if(layout.Target != this)
            {
                layout.Target = this;
            }
        }

        public void RemoveLayout(ILayout layout)
        {
            if (_layouts.Contains(layout))
            {
                _layouts.Remove(layout);
            }
            if(layout.Target == this)
            {
                layout.Target = null;
            }
        }


        public void UpdateAnchorParam(Vector3 anchorMin, Vector3 anchorMax, Vector3 offsetMin, Vector3 offsetMax)
        {
            NormalizeAnchorPos(ref anchorMin, ref anchorMax);
            var prevLocalSize = LocalSize;
            var prevOffset = Offset;

            var parentLayoutSize = Parent != null
                ? Parent.LayoutSize()
                : Vector3.zero;
            var anchorAreaSize = parentLayoutSize.Mul(anchorMax - anchorMin);
            _localSize = LimitLocalSizeByLayoutInfo(anchorAreaSize + offsetMin + offsetMax);

            NormalizeLocalSize(ref _localSize, ref offsetMin, ref offsetMax, anchorAreaSize);

            var prevAnchorMin = _anchorMin;
            var prevAnchorMax = _anchorMax;
            _anchorMin = anchorMin;
            _anchorMax = anchorMax;

            _offset = -offsetMin.Mul(Vector3.one - Pivot) + offsetMax.Mul(Pivot);

            _prevParentSize = parentLayoutSize;

            OnUpdateLocalSizeWithExceptionCheck(prevLocalSize);
            OnUpdateOffsetWithExceptionCheck(prevOffset);
            OnUpdateAnchorMinMaxWithExceptionCheck(prevAnchorMin, prevAnchorMax);
        }

        public void UpdateLocalSize(Vector3 localSize, Vector3 offset)
        {
            var prevLocalSize = LocalSize;
            var prevOffset = Offset;
            localSize = Vector3.Max(localSize, Vector3.zero);

            _localSize = LimitLocalSizeByLayoutInfo(localSize);

            _offset = offset;

            _prevParentSize = Parent != null
                ? Parent.LayoutSize()
                : Vector3.zero;
            OnUpdateLocalSizeWithExceptionCheck(prevLocalSize);
            OnUpdateOffsetWithExceptionCheck(prevOffset);
        }

        public void FollowParent()
        {
            //以前のOffsetMin/Maxを計算しています。
            var prevAnchorAreaSize = _prevParentSize.Mul(AnchorMax - AnchorMin);
            var max = prevAnchorAreaSize * 0.5f;
            var min = -max;

            var halfSize = LocalSize * 0.5f;
            var (localMin, localMax) = (-halfSize + Offset, halfSize + Offset);
            var (offsetMin, offsetMax) = (-(localMin - min), localMax - max);

            UpdateAnchorParam(AnchorMin, AnchorMax, offsetMin, offsetMax);
        }

        Vector3 LimitLocalSizeByLayoutInfo(Vector3 localSize)
        {
            var min = LayoutInfo.MinSize;
            if (min.x >= 0) localSize.x = Max(min.x, localSize.x);
            if (min.y >= 0) localSize.y = Max(min.y, localSize.y);
            if (min.z >= 0) localSize.z = Max(min.z, localSize.z);

            var max = LayoutInfo.MaxSize;
            if (max.x >= 0) localSize.x = Min(max.x, localSize.x);
            if (max.y >= 0) localSize.y = Min(max.y, localSize.y);
            if (max.z >= 0) localSize.z = Min(max.z, localSize.z);
            return localSize;
        }

        static Vector3 CalPivotOffset(Vector3 localSize, Vector3 pivot)
        {
            return -localSize.Mul((pivot - Vector3.one * 0.5f));
        }

        static void NormalizeAnchorPos(ref Vector3 min, ref Vector3 max)
        {
            var tmpMin = Vector3.Min(min, max);
            max = Vector3.Max(min, max);
            min = tmpMin;
        }

        static void NormalizeLocalSize(ref Vector3 localSize, ref Vector3 offsetMin, ref Vector3 offsetMax, Vector3 anchorAreaSize)
        {
            if (localSize.x < 0)
            {
                localSize.x = 0;
                offsetMin.x = -anchorAreaSize.x * 0.5f;
                offsetMax.x = -anchorAreaSize.x * 0.5f;
            }
            if (localSize.y < 0)
            {
                localSize.y = 0;
                offsetMin.y = -anchorAreaSize.y * 0.5f;
                offsetMax.y = -anchorAreaSize.y * 0.5f;
            }
            if (localSize.z < 0)
            {
                localSize.z = 0;
                offsetMin.z = -anchorAreaSize.z * 0.5f;
                offsetMax.z = -anchorAreaSize.z * 0.5f;
            }
        }

        void OnUpdateLocalSizeWithExceptionCheck(Vector3 prevLocalSize)
        {
            if (prevLocalSize.AreNearlyEqual(_localSize)) return;

            _onChangedLocalSize.SafeDynamicInvoke(this, prevLocalSize, () => $"LayoutTargetObject#Update LocalSize", LayoutDefines.LOG_SELECTOR);
        }

        void OnUpdateOffsetWithExceptionCheck(Vector3 prevOffset)
        {
            if (prevOffset.AreNearlyEqual(_offset)) return;

            _onChangedOffset.SafeDynamicInvoke(this, prevOffset, () => $"LayoutTargetObject#Update Offset", LayoutDefines.LOG_SELECTOR);
        }

        void OnUpdateAnchorMinMaxWithExceptionCheck(Vector3 prevAnchorMin, Vector3 prevAnchorMax)
        {
            if (prevAnchorMin.AreNearlyEqual(_anchorMin)
                && prevAnchorMax.AreNearlyEqual(_anchorMax))
            {
                return;
            }

            _onChangedAnchorMinMax.SafeDynamicInvoke(this, prevAnchorMin, prevAnchorMax, () => $"LayoutTargetObject#Update AnchorMin/Max", LayoutDefines.LOG_SELECTOR);
        }

        #endregion
    }
}

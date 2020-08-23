using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Hinode.Layouts
{
    /// <summary>
	/// ILayoutTargetの標準実装
	/// 
	/// <seealso cref="ILayoutTarget"/>
	/// </summary>
    public class LayoutTargetObject : ILayoutTarget
    {
        SmartDelegate<ILayoutTargetOnDisposed> _onDisposed = new SmartDelegate<ILayoutTargetOnDisposed>();
        SmartDelegate<ILayoutTargetOnChangedParent> _onChangedParent = new SmartDelegate<ILayoutTargetOnChangedParent>();
        SmartDelegate<ILayoutTargetOnChangedChildren> _onChangedChildren = new SmartDelegate<ILayoutTargetOnChangedChildren>();
        SmartDelegate<ILayoutTargetOnChangedLocalPos> _onChangedLocalPos = new SmartDelegate<ILayoutTargetOnChangedLocalPos>();
        SmartDelegate<ILayoutTargetOnChangedLocalSize> _onChangedLocalSize = new SmartDelegate<ILayoutTargetOnChangedLocalSize>();
        SmartDelegate<ILayoutTargetOnChangedOffset> _onChangedOffset = new SmartDelegate<ILayoutTargetOnChangedOffset>();

        LayoutTargetObject _parent;
        HashSetHelper<LayoutTargetObject> _children = new HashSetHelper<LayoutTargetObject>();

        Vector3 _localPos;

        Vector3 _localSize;
        Vector3 _anchorMin;
        Vector3 _anchorMax;
        Vector3 _offset;

        public LayoutTargetObject()
        {
            _children.OnAdded.Add(_child => {
                try
                {
                    _onChangedChildren.Instance?.Invoke(this, _child, ILayoutTargetOnChangedChildMode.Add);
                }
                catch(System.Exception e)
                {
                    Logger.LogWarning(Logger.Priority.High, () => $"Exception!! LayoutTargetObject#Add Child: {e.Message}", LayoutDefines.LOG_SELECTOR);
                }
            });
            _children.OnRemoved.Add(_child => {
                try
                {
                    _onChangedChildren.Instance?.Invoke(this, _child, ILayoutTargetOnChangedChildMode.Remove);
                }
                catch (System.Exception e)
                {
                    Logger.LogWarning(Logger.Priority.High, () => $"Exception!! LayoutTargetObject#Remove Child: {e.Message}", LayoutDefines.LOG_SELECTOR);
                }
            });
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

            var prevAnchorAreaSize = prevLocalSize.Mul(AnchorMax - AnchorMin);
            var max = prevAnchorAreaSize*0.5f;
            var min = -max;
            var (localMin, localMax) = this.LocalAreaMinMaxPos();
            var (offsetMin, offsetMax) = (-(localMin - min), localMax - max);

            UpdateLocalSizeWithAnchorParam(AnchorMin, AnchorMax, offsetMin, offsetMax);
        }

        #region ILayoutTarget interface
        public NotInvokableDelegate<ILayoutTargetOnDisposed> OnDisposed { get => _onDisposed; }
        public NotInvokableDelegate<ILayoutTargetOnChangedParent> OnChangedParent { get => _onChangedParent; }
        public NotInvokableDelegate<ILayoutTargetOnChangedChildren> OnChangedChildren { get => _onChangedChildren; }
        public NotInvokableDelegate<ILayoutTargetOnChangedLocalPos> OnChangedLocalPos { get => _onChangedLocalPos; }
        public NotInvokableDelegate<ILayoutTargetOnChangedLocalSize> OnChangedLocalSize { get => _onChangedLocalSize; }
        public NotInvokableDelegate<ILayoutTargetOnChangedOffset> OnChangedOffset { get => _onChangedOffset; }

        public ILayoutTarget Parent { get => _parent; }
        public IEnumerable<ILayoutTarget> Children { get => _children; }
        public int ChildCount { get => _children.Count; }

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
        }

        public void UpdateLocalSizeWithAnchorParam(Vector3 anchorMin, Vector3 anchorMax, Vector3 offsetMin, Vector3 offsetMax)
        {
            NormalizeAnchorPos(ref anchorMin, ref anchorMax);
            var prevLocalSize = LocalSize;
            var prevOffset = Offset;

            var anchorAreaSize = this.ParentLocalSize().Mul(anchorMax - anchorMin);
            _localSize = anchorAreaSize + offsetMin + offsetMax;

            NormalizeLocalSize(ref _localSize, ref offsetMin, ref offsetMax, anchorAreaSize);

            _anchorMin = anchorMin;
            _anchorMax = anchorMax;

            var minPos = _localSize * -0.5f - offsetMin;
            var maxPos = _localSize * 0.5f + offsetMax;
            _offset = (maxPos + minPos) * 0.5f;

            OnUpdateLocalSizeWithExceptionCheck(prevLocalSize);
            OnUpdateOffsetWithExceptionCheck(prevOffset);
        }

        public void UpdateLocalSizeWithSizeAndAnchorParam(Vector3 localSize, Vector3 anchorMin, Vector3 anchorMax, Vector3 offset)
        {
            NormalizeAnchorPos(ref anchorMin, ref anchorMax);
            var prevLocalSize = LocalSize;
            var prevOffset = Offset;
            localSize = Vector3.Max(localSize, Vector3.zero);

            _anchorMin = anchorMin;
            _anchorMax = anchorMax;
            _localSize = localSize;
            _offset = offset;

            OnUpdateLocalSizeWithExceptionCheck(prevLocalSize);
            OnUpdateOffsetWithExceptionCheck(prevOffset);
        }

        void NormalizeAnchorPos(ref Vector3 min, ref Vector3 max)
        {
            var tmpMin = Vector3.Min(min, max);
            max = Vector3.Max(min, max);
            min = tmpMin;
        }

        void NormalizeLocalSize(ref Vector3 localSize, ref Vector3 offsetMin, ref Vector3 offsetMax, Vector3 anchorAreaSize)
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
        #endregion
    }
}

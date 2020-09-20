using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.Layouts
{
    public delegate void ILayoutOnDisposed(ILayout self);
    public delegate void ILayoutOnChanged(ILayout self, bool doChanged);
    public delegate void ILayoutOnChangedOperationPriority(ILayout self, int prevPriority);

    /// <summary>
    /// Layoutの処理対象となるものを表すenum
    /// </summary>
    [System.Flags]
    public enum LayoutOperationTarget
    {
        Self_LocalPos = 0x1 << 0,
        Self_Anchor = 0x1 << 1,
        Self_LocalSize = 0x1 << 2,
        Self_Offset = 0x1 << 3,
        Self_Pivot = 0x1 << 4,

        Children_LocalPos = 0x1 << 5,
        Children_Anchor = 0x1 << 6,
        Children_LocalSize = 0x1 << 7,
        Children_Offset = 0x1 << 8,
        Children_Pivot = 0x1 << 9,

        Parent_LocalPos = 0x1 << 10,
        Parent_Anchor = 0x1 << 11,
        Parent_LocalSize = 0x1 << 12,
        Parent_Offset = 0x1 << 13,
        Parent_Pivot = 0x1 << 14,
    }

    /// <summary>
	/// <seealso cref="ILayoutTarget"/>
	/// <seealso cref="LayoutManager"/>
	/// </summary>
    public interface ILayout : System.IDisposable
    {
        NotInvokableDelegate<ILayoutOnDisposed> OnDisposed { get;}
        NotInvokableDelegate<ILayoutOnChanged> OnChanged { get; }
        NotInvokableDelegate<ILayoutOnChangedOperationPriority> OnChangedOperationPriority { get; }

        int OperationPriority { get; set; }
        ILayoutTarget Target { get; set; }
        bool DoChanged { get; }

        LayoutOperationTarget OperationTargetFlags { get; }

        bool Validate();
        void UpdateLayout();
        void ForceUpdateLayout();
    }

    public static partial class ILayoutExtensions
    {
        public static bool ContainsTarget(this ILayout layout)
            => layout.Target != null;
    }

    /// <summary>
	/// <seealso cref="ILayout"/>
	/// </summary>
    public abstract class LayoutBase : ILayout
    {
        virtual protected void InnerOnChangedTarget(ILayoutTarget current, ILayoutTarget prev) { }
        virtual protected void InnerOnChanged(bool doChanged) { }

        SmartDelegate<ILayoutOnDisposed> _onDisposed = new SmartDelegate<ILayoutOnDisposed>();
        protected SmartDelegate<ILayoutOnChanged> _onChanged = new SmartDelegate<ILayoutOnChanged>();
        SmartDelegate<ILayoutOnChangedOperationPriority> _onChangedOperationPriority = new SmartDelegate<ILayoutOnChangedOperationPriority>();

        [SerializeField] int _operationPriority = 0;

        ILayoutTarget _target;
        public ILayoutTarget Target
        {
            get => _target;
            set
            {
                if (_target == value) return;

                var prev = _target;
                _target = value;
                if(prev != null)
                {
                    prev.RemoveLayout(this);
                    prev.OnDisposed.Remove(AutoRemoveTarget);
                }
                if(_target != null)
                {
                    _target.AddLayout(this);
                    _target.OnDisposed.Add(AutoRemoveTarget);
                }
                DoChanged = true;
                InnerOnChangedTarget(_target, prev);
            }
        }

        void AutoRemoveTarget(ILayoutTarget target)
        {
            target.OnDisposed.Remove(AutoRemoveTarget);
            if(_target == target)
                _target = null;
        }

        public virtual void Dispose()
        {
            _onDisposed.SafeDynamicInvoke(this, () => $"LayoutBase#OnDisposed", LayoutDefines.LOG_SELECTOR);
            _onDisposed.Clear();
            _onChanged.Clear();
            _onChangedOperationPriority.Clear();

            Target = null;
        }

        #region ILayout interface
        bool _doChanged = false;

        public NotInvokableDelegate<ILayoutOnDisposed> OnDisposed { get => _onDisposed; }
        public NotInvokableDelegate<ILayoutOnChanged> OnChanged { get => _onChanged; }
        public NotInvokableDelegate<ILayoutOnChangedOperationPriority> OnChangedOperationPriority { get => _onChangedOperationPriority; }

        public int OperationPriority
        {
            get => _operationPriority;
            set
            {
                if (_operationPriority == value) return;
                var prevPriority = _operationPriority;
                _operationPriority = value;

                _onChangedOperationPriority.SafeDynamicInvoke(this, prevPriority, () => $"LayoutBase#OnChangedOperationPriority", LayoutDefines.LOG_SELECTOR);
            }
        }

        public bool DoChanged
        {
            get => _doChanged;
            protected set
            {
                if (_doChanged == value) return;
                _doChanged = value;

                try
                {
                    InnerOnChanged(_doChanged);
                }
                catch (System.Exception e)
                {
                    Logger.LogWarning(Logger.Priority.High, () => $"Exception!! LayoutBase#DoChanged {System.Environment.NewLine}{e.Message}", LayoutDefines.LOG_SELECTOR);
                }
                _onChanged.SafeDynamicInvoke(this, _doChanged, () => $"LayoutBase#DoChanged", LayoutDefines.LOG_SELECTOR);
            }
        }

        public abstract LayoutOperationTarget OperationTargetFlags { get; }

        public abstract bool Validate();
        public abstract void UpdateLayout();

        public void ForceUpdateLayout()
        {
            DoChanged = true;
            UpdateLayout();
        }
        #endregion
    }

}

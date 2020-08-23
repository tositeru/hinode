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
        Self,
        Children,
        Parent,
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
        Vector3 UnitSize { get; }

        LayoutOperationTarget OperationTargetFlags { get; }

        void UpdateUnitSize();
        void UpdateLayout();
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
                var prev = _target;
                if(_target != null)
                {
                    _target.OnDisposed.Remove(AutoRemoveTarget);
                }
                _target = value;
                if(_target != null)
                {
                    _target.OnDisposed.Add(AutoRemoveTarget);
                }
                DoChanged = _target != prev;
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
        Vector3 _unitSize;

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

        /// <summary>
        /// このプロパティを変更してもDoChangedに影響は与えませんので注意してください。
        /// </summary>
        public Vector3 UnitSize
        {
            get => _unitSize;
            protected set
            {
                _unitSize = value;
            }
        }

        public abstract LayoutOperationTarget OperationTargetFlags { get; }

        public abstract void UpdateUnitSize();
        public abstract void UpdateLayout();
        #endregion
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.Layouts
{
    public delegate void ILayoutOnDisposed(ILayout self);

    /// <summary>
	/// <seealso cref="ILayoutTarget"/>
	/// <seealso cref="LayoutManager"/>
	/// </summary>
    public interface ILayout : System.IDisposable
    {
        NotInvokableDelegate<ILayoutOnDisposed> OnDisposed { get;}

        ILayoutTarget Target { get; set; }
        bool DoChanged { get; }
        Vector3 UnitSize { get; }

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
        SmartDelegate<ILayoutOnDisposed> _onDisposed = new SmartDelegate<ILayoutOnDisposed>();

        ILayoutTarget _target;
        public ILayoutTarget Target
        {
            get => _target;
            set
            {
                if(_target != null)
                {
                    _target.OnDisposed.Remove(AutoRemoveTarget);
                }
                _target = value;
                if(_target != null)
                {
                    _target.OnDisposed.Add(AutoRemoveTarget);
                }
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
            try
            {
                _onDisposed.Instance?.Invoke(this);
            }
            catch(System.Exception e)
            {
                Logger.LogWarning(Logger.Priority.High, () => $"Exception!! LayoutBase#OnDisposed {System.Environment.NewLine}{e.Message}", LayoutDefines.LOG_SELECTOR);
            }
            _onDisposed.Clear();

            Target = null;
        }

        #region ILayout interface
        public NotInvokableDelegate<ILayoutOnDisposed> OnDisposed { get => _onDisposed; }

        public abstract bool DoChanged { get; }
        public abstract Vector3 UnitSize { get; }

        public abstract void UpdateUnitSize();
        public abstract void UpdateLayout();
        #endregion
    }

}

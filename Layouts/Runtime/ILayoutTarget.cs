using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.Layouts
{
    public delegate void ILayoutTargetOnDisposed(ILayoutTarget self);

    /// <summary>
	/// <seealso cref="ILayout"/>
	/// <seealso cref="LayoutManager"/>
	/// </summary>
    public interface ILayoutTarget : System.IDisposable
    {
        NotInvokableDelegate<ILayoutTargetOnDisposed> OnDisposed { get; }

        ILayoutTarget Parent { get; }
        IEnumerable<ILayoutTarget> Childrens { get; }

        Vector3 Pos { get; set; }
        Vector3 Size { get; set; }

        Vector3 AnchorMin { get; set; }
        Vector3 AnchorMax { get; set; }

        Vector3 AnchorOffsetMin { get; set; }
        Vector3 AnchorOffsetMax { get; set; }
    }

    /// <summary>
	/// <see cref="ILayoutTarget"/>
	/// </summary>
    public abstract class LayoutTargetBase : ILayoutTarget
    {
        SmartDelegate<ILayoutTargetOnDisposed> _onDisposed = new SmartDelegate<ILayoutTargetOnDisposed>();

        #region ILayoutTarget interface
        public NotInvokableDelegate<ILayoutTargetOnDisposed> OnDisposed { get => _onDisposed; }

        public abstract ILayoutTarget Parent { get; }
        public abstract IEnumerable<ILayoutTarget> Childrens { get; }

        public abstract Vector3 Pos { get; set; }
        public abstract Vector3 Size { get; set; }

        public abstract Vector3 AnchorMin { get; set; }
        public abstract Vector3 AnchorMax { get; set; }

        public abstract Vector3 AnchorOffsetMin { get; set; }
        public abstract Vector3 AnchorOffsetMax { get; set; }

        public virtual void Dispose()
        {
            _onDisposed.Instance?.Invoke(this);
            _onDisposed.Clear();
        }
        #endregion
    }
}

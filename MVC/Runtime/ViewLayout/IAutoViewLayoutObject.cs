using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.MVC
{
    /// <summary>
    /// 
    /// <seealso cref="IViewObject"/>
    /// <seealso cref="ViewLayouter"/>
    /// </summary>
    public interface IAutoViewLayoutObject : IViewLayoutable
    {
        IViewObject Target { get; }
        void Attach(IViewObject viewObject);
        void Dettach();
        void OnViewLayouted();
    }

    public class EmptyAutoViewLayoutObject : IAutoViewLayoutObject
    {
        public IViewObject Target { get; protected set; }
        public virtual void Attach(IViewObject viewObject)
        {
            Target = viewObject;
        }

        public virtual void Dettach()
        {
            Target = null;
        }
        public virtual void OnViewLayouted() { }
    }
}

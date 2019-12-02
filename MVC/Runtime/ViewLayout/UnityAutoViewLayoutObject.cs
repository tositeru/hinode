using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hinode.MVC
{
    /// <summary>
    /// <seealso cref="IAutoViewLayoutObject"/>
    /// </summary>
    public class UnityAutoViewLayoutObject : MonoBehaviour
        , IAutoViewLayoutObject
    {
        #region IAutoViewLayoutObject interface
        public IViewObject Target { get; private set; }

        public virtual void Attach(IViewObject viewObject)
        {
            Assert.IsTrue(viewObject is MonoBehaviour, $"The ViewObject that this class is attached is not MonoBehaviour... viewObj Type={viewObject.GetType()}");
            Target = viewObject;
        }

        public virtual void Dettach()
        {
            Destroy(this);
            Target = null;
        }

        public virtual void OnViewLayouted()
        { }
        #endregion
    }
}

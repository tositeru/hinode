using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode.MVC
{
    /// <summary>
    /// <seealso cref="ISiblingOrderViewLayout"/>
    /// <seealso cref="IAutoViewLayoutObject"/>
    /// </summary>
    public class SiblingOrderAutoViewLayoutObject : EmptyAutoViewLayoutObject
        , ISiblingOrderViewLayout
    {
        public uint SiblingOrder { get; set; }

        #region IAutoViewLayoutObject interface
        #endregion

        public class AutoCreator : ViewLayouter.IAutoViewObjectCreator
        {
            protected override IAutoViewLayoutObject CreateImpl(IViewObject viewObj)
            {
                return new SiblingOrderAutoViewLayoutObject();
            }

            public override IEnumerable<System.Type> GetSupportedIViewLayouts()
            {
                return typeof(SiblingOrderAutoViewLayoutObject).GetInterfaces()
                    .Where(_t => _t.HasInterface<IViewLayout>());
            }
        }

    }
}

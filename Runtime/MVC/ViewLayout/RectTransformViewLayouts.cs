using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    interface IRectTransformAncherXViewLayout : IViewLayout
    {
        Vector2 RectTransformAnchorXLayout { get; set; }
    }

    class RectTransformAnchorXViewLayoutAccessor : IViewLayoutAccessor
    {
        public override System.Type ViewLayoutType { get => typeof(IRectTransformAncherXViewLayout); }

        public override System.Type ValueType { get => typeof(Vector2); }

        protected override object GetImpl(IViewObject viewObj)
        {
            return (viewObj as IRectTransformAncherXViewLayout).RectTransformAnchorXLayout;
        }

        protected override void SetImpl(object value, IViewObject viewObj)
        {
            (viewObj as IRectTransformAncherXViewLayout).RectTransformAnchorXLayout = (Vector2)value;
        }
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode
{
    public interface IDepthViewLayout : IViewLayout
    {
        float DepthLayout { get; set; }
    }

    public class DepthViewLayoutAccessor : IViewLayoutAccessor
    {
        public override Type ViewLayoutType { get => typeof(IDepthViewLayout); }
        public override Type ValueType { get => typeof(float); }
        public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

        protected override object GetImpl(IViewObject viewObj)
        {
            return (viewObj as IDepthViewLayout).DepthLayout;
        }

        protected override void SetImpl(object value, IViewObject viewObj)
        {
            var layout = (viewObj as IDepthViewLayout);
            if (value.GetType().IsFloat())
            {
                layout.DepthLayout = (float)value;
            }
            else if (value.GetType().IsInteger())
            {
                layout.DepthLayout = (int)value;
            }
            else
            {
                layout.DepthLayout = (float)value;
            }
        }

        public override bool IsVaildValue(object value)
        {
            return value.GetType().IsNumeric();
        }

    }
}

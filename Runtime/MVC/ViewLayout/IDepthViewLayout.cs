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

        protected override object GetImpl(IViewObject viewObj)
        {
            return (viewObj as IDepthViewLayout).DepthLayout;
        }

        protected override void SetImpl(object value, IViewObject viewObj)
        {
            (viewObj as IDepthViewLayout).DepthLayout = (float)value;
        }
    }
}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.MVC
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

        protected override object GetImpl(object viewLayoutObj)
        {
            return (viewLayoutObj as IDepthViewLayout).DepthLayout;
        }

        protected override void SetImpl(object value, object viewLayoutObj)
        {
            var layout = (viewLayoutObj as IDepthViewLayout);
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.MVC
{
    public interface IColorViewLayout : IViewLayout
    {
        Color ColorLayout { get; set; }
    }

    public class ColorViewLayoutAccessor : IViewLayoutAccessor
    {
        public override Type ViewLayoutType { get => typeof(IColorViewLayout); }
        public override Type ValueType { get => typeof(Color); }

        public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

        protected override object GetImpl(object viewLayoutObj)
            => (viewLayoutObj as IColorViewLayout).ColorLayout;

        protected override void SetImpl(object value, object viewLayoutObj)
            => (viewLayoutObj as IColorViewLayout).ColorLayout = (Color)value;
    }
}


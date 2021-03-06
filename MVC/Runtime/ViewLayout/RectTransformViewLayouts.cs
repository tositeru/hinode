﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.MVC
{
    public enum RectTransformViewLayoutName
    {
        anchorX,
        anchorY,
        anchorMin,
        anchorMax,
        pivot,
        size,
        offsetMin,
        offsetMax,
    }

    //
    // 以下のクラス定義はHinode/Editors/Assets/MVC/RectTransformViewLayoutTemplate.assetから生成されています。
    //

    #region AnchorX
    public interface IRectTransformAnchorXViewLayout : IViewLayout
    {
        Vector2 RectTransformAnchorXLayout { get; set; }
    }

    public class RectTransformAnchorXViewLayoutAccessor : IViewLayoutAccessor
    {
        public override System.Type ViewLayoutType { get => typeof(IRectTransformAnchorXViewLayout); }
        public override System.Type ValueType { get => typeof(Vector2); }
        public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

        protected override object GetImpl(object viewLayoutObj)
        {
            return (viewLayoutObj as IRectTransformAnchorXViewLayout).RectTransformAnchorXLayout;
        }

        protected override void SetImpl(object value, object viewLayoutObj)
        {
            (viewLayoutObj as IRectTransformAnchorXViewLayout).RectTransformAnchorXLayout = (Vector2)value;
        }
    }
    #endregion

    #region AnchorY
    public interface IRectTransformAnchorYViewLayout : IViewLayout
    {
        Vector2 RectTransformAnchorYLayout { get; set; }
    }

    public class RectTransformAnchorYViewLayoutAccessor : IViewLayoutAccessor
    {
        public override System.Type ViewLayoutType { get => typeof(IRectTransformAnchorYViewLayout); }
        public override System.Type ValueType { get => typeof(Vector2); }
        public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

        protected override object GetImpl(object viewLayoutObj)
        {
            return (viewLayoutObj as IRectTransformAnchorYViewLayout).RectTransformAnchorYLayout;
        }

        protected override void SetImpl(object value, object viewLayoutObj)
        {
            (viewLayoutObj as IRectTransformAnchorYViewLayout).RectTransformAnchorYLayout = (Vector2)value;
        }
    }
    #endregion

    #region Pivot
    public interface IRectTransformPivotViewLayout : IViewLayout
    {
        Vector2 RectTransformPivotLayout { get; set; }
    }

    public class RectTransformPivotViewLayoutAccessor : IViewLayoutAccessor
    {
        public override System.Type ViewLayoutType { get => typeof(IRectTransformPivotViewLayout); }
        public override System.Type ValueType { get => typeof(Vector2); }
        public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

        protected override object GetImpl(object viewLayoutObj)
        {
            return (viewLayoutObj as IRectTransformPivotViewLayout).RectTransformPivotLayout;
        }

        protected override void SetImpl(object value, object viewLayoutObj)
        {
            (viewLayoutObj as IRectTransformPivotViewLayout).RectTransformPivotLayout = (Vector2)value;
        }
    }
    #endregion

    #region Size
    public interface IRectTransformSizeViewLayout : IViewLayout
    {
        Vector2 RectTransformSizeLayout { get; set; }
    }

    public class RectTransformSizeViewLayoutAccessor : IViewLayoutAccessor
    {
        public override System.Type ViewLayoutType { get => typeof(IRectTransformSizeViewLayout); }
        public override System.Type ValueType { get => typeof(Vector2); }
        public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

        protected override object GetImpl(object viewLayoutObj)
        {
            return (viewLayoutObj as IRectTransformSizeViewLayout).RectTransformSizeLayout;
        }

        protected override void SetImpl(object value, object viewLayoutObj)
        {
            (viewLayoutObj as IRectTransformSizeViewLayout).RectTransformSizeLayout = (Vector2)value;
        }
    }
    #endregion

    #region AnchorMin
    public interface IRectTransformAnchorMinViewLayout : IViewLayout
    {
        Vector2 RectTransformAnchorMinLayout { get; set; }
    }

    public class RectTransformAnchorMinViewLayoutAccessor : IViewLayoutAccessor
    {
        public override System.Type ViewLayoutType { get => typeof(IRectTransformAnchorMinViewLayout); }
        public override System.Type ValueType { get => typeof(Vector2); }
        public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

        protected override object GetImpl(object viewLayoutObj)
        {
            return (viewLayoutObj as IRectTransformAnchorMinViewLayout).RectTransformAnchorMinLayout;
        }

        protected override void SetImpl(object value, object viewLayoutObj)
        {
            (viewLayoutObj as IRectTransformAnchorMinViewLayout).RectTransformAnchorMinLayout = (Vector2)value;
        }
    }
    #endregion

    #region AnchorMax
    public interface IRectTransformAnchorMaxViewLayout : IViewLayout
    {
        Vector2 RectTransformAnchorMaxLayout { get; set; }
    }

    public class RectTransformAnchorMaxViewLayoutAccessor : IViewLayoutAccessor
    {
        public override System.Type ViewLayoutType { get => typeof(IRectTransformAnchorMaxViewLayout); }
        public override System.Type ValueType { get => typeof(Vector2); }
        public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

        protected override object GetImpl(object viewLayoutObj)
        {
            return (viewLayoutObj as IRectTransformAnchorMaxViewLayout).RectTransformAnchorMaxLayout;
        }

        protected override void SetImpl(object value, object viewLayoutObj)
        {
            (viewLayoutObj as IRectTransformAnchorMaxViewLayout).RectTransformAnchorMaxLayout = (Vector2)value;
        }
    }
    #endregion

    #region OffsetMin
    public interface IRectTransformOffsetMinViewLayout : IViewLayout
    {
        Vector2 RectTransformOffsetMinLayout { get; set; }
    }

    public class RectTransformOffsetMinViewLayoutAccessor : IViewLayoutAccessor
    {
        public override System.Type ViewLayoutType { get => typeof(IRectTransformOffsetMinViewLayout); }
        public override System.Type ValueType { get => typeof(Vector2); }
        public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

        protected override object GetImpl(object viewLayoutObj)
        {
            return (viewLayoutObj as IRectTransformOffsetMinViewLayout).RectTransformOffsetMinLayout;
        }

        protected override void SetImpl(object value, object viewLayoutObj)
        {
            (viewLayoutObj as IRectTransformOffsetMinViewLayout).RectTransformOffsetMinLayout = (Vector2)value;
        }
    }
    #endregion

    #region OffsetMax
    public interface IRectTransformOffsetMaxViewLayout : IViewLayout
    {
        Vector2 RectTransformOffsetMaxLayout { get; set; }
    }

    public class RectTransformOffsetMaxViewLayoutAccessor : IViewLayoutAccessor
    {
        public override System.Type ViewLayoutType { get => typeof(IRectTransformOffsetMaxViewLayout); }
        public override System.Type ValueType { get => typeof(Vector2); }
        public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

        protected override object GetImpl(object viewLayoutObj)
        {
            return (viewLayoutObj as IRectTransformOffsetMaxViewLayout).RectTransformOffsetMaxLayout;
        }

        protected override void SetImpl(object value, object viewLayoutObj)
        {
            (viewLayoutObj as IRectTransformOffsetMaxViewLayout).RectTransformOffsetMaxLayout = (Vector2)value;
        }
    }
    #endregion
}

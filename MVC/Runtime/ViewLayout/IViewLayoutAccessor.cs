using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.MVC
{
    [System.Flags()]
    public enum ViewLayoutAccessorUpdateTiming : uint
    {
        AtOnlyModel = 0x1 << 0,
        Always = 0x1 << 1,
        All = 0xffffffff,
    }

    public abstract class IViewLayoutAccessor
    {
        public abstract System.Type ViewLayoutType { get; }
        public abstract System.Type ValueType { get; }
        public abstract ViewLayoutAccessorUpdateTiming UpdateTiming { get; }

        public ViewLayouter.IAutoViewObjectCreator UseAutoCreator { get; set; }

        protected abstract void SetImpl(object value, IViewObject viewObj);
        protected abstract object GetImpl(IViewObject viewObj);

        public void Set(object value, IViewObject viewObj)
        {
            if (!IsVaildValue(value) || !IsVaildViewObject(viewObj))
            {
                throw new System.ArgumentException($"Don't set value({value.GetType()}) to {viewObj.GetType()}... Valid ValueType={ValueType} viewLayoutType={ViewLayoutType}");
            }
            SetImpl(value, viewObj);
        }

        public object Get(IViewObject viewObj)
        {
            if (!IsVaildViewObject(viewObj))
            {
                throw new System.ArgumentException($"Don't Get value from {viewObj.GetType()}... Valid viewLayoutType={ViewLayoutType}");
            }
            return GetImpl(viewObj);
        }

        public virtual bool IsVaildViewObject(IViewObject viewObj)
        {
            return viewObj.GetType().HasInterface(ViewLayoutType);
        }
        public virtual bool IsVaildValue(object value)
        {
            return value.GetType().Equals(ValueType);
        }
    }
}

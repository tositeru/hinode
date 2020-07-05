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

        protected abstract void SetImpl(object value, object viewLayoutObj);
        protected abstract object GetImpl(object viewLayoutObj);

        public void Set(object value, object viewLayoutObj)
        {
            if (!IsVaildValue(value) || !IsVaildViewLayoutType(viewLayoutObj.GetType()))
            {
                throw new System.ArgumentException($"Don't set value({value.GetType()}) to {viewLayoutObj.GetType()}... Valid ValueType={ValueType} viewLayoutType={ViewLayoutType}");
            }
            SetImpl(value, viewLayoutObj);
        }

        public object Get(object viewLayoutObj)
        {
            if (!IsVaildViewLayoutType(viewLayoutObj.GetType()))
            {
                throw new System.ArgumentException($"Don't Get value from {viewLayoutObj.GetType()}... Valid viewLayoutType={ViewLayoutType}");
            }
            return GetImpl(viewLayoutObj);
        }

        public virtual bool IsVaildViewLayoutType(System.Type type)
        {
            return type.ContainsInterface(ViewLayoutType);
        }

        public virtual bool IsVaildValue(object value)
        {
            return value.GetType().Equals(ValueType);
        }

        static protected IViewObject GetViewObject(object viewLayoutObj)
        {
            if(viewLayoutObj is IViewObject)
            {
                return viewLayoutObj as IViewObject;
            }
            else if(viewLayoutObj is IAutoViewLayoutObject)
            {
                return (viewLayoutObj as IAutoViewLayoutObject).Target;
            }
            else
            {
                throw new System.ArgumentException($"This Object({viewLayoutObj.GetType()}) is not Support Type... Please Pass IViewObject or IAutoViewLayoutObject!");
            }
        }
    }
}

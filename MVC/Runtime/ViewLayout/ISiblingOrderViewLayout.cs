using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hinode.MVC
{
    /// <summary>
    /// View用のIChildrenOrder
    /// Modelに継承されているIChildrenOrderとは関係なしにIViewObject自身の並び順を表します。
    /// <seealso cref="IChildrenOrder"/>
    /// </summary>
    public interface ISiblingOrderViewLayout : ISiblingOrder, IViewLayout
    {
    }

    public class SiblingOrderViewLayoutAccessor : IViewLayoutAccessor
    {
        public override Type ViewLayoutType { get => typeof(ISiblingOrderViewLayout); }
        public override Type ValueType { get => typeof(uint); }
        public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

        protected override object GetImpl(IViewObject viewObj)
            => (viewObj as ISiblingOrderViewLayout).SiblingOrder;

        protected override void SetImpl(object value, IViewObject viewObj)
        {
            var layout = (viewObj as ISiblingOrderViewLayout);
            layout.SiblingOrder = (uint)value;

            if(viewObj is MonoBehaviour)
            {
                var behaviour = viewObj as MonoBehaviour;
                if(behaviour.transform.parent != null)
                {
                    SiblingOrderViewLayoutAccessor.Insert(behaviour.transform.parent, viewObj);
                }
            }
        }

        public static void Insert(Transform parent, IViewObject target)
        {
            if (!(target is MonoBehaviour)) return;

            if(!(target.UseModel is ISiblingOrder)
                && !(target.UseBindInfo?.ViewLayoutValues.Layouts.OfType<ISiblingOrderViewLayout>().Any() ?? false))
                return;

            var comparer = new SiblingOrderViewObjectCompare();
            var lowerChild = parent.GetChildEnumerable()
                .Where(_c =>
                {
                    if (_c.TryGetComponent<IViewObject>(out var childViewObj))
                    {
                        return comparer.Compare(target, childViewObj) == -1;
                    }
                    else
                    {
                        return false;
                    }
                })
                .FirstOrDefault();

            var t = (target as MonoBehaviour).transform;
            if (t.parent != parent)
            {
                t.SetParent(parent);
            }

            if (lowerChild != null)
            {
                t.SetSiblingIndex(lowerChild.GetSiblingIndex());
            }
            else
            {
                t.SetSiblingIndex(parent.childCount-1);
            }

            if (target.UseModel is ButtonModel)
            {
                var btn = target.UseModel as ButtonModel;
                var insertIndex = lowerChild != null ? lowerChild.GetSiblingIndex() : -1;
                Debug.Log($"{btn.Text} insert to parent({parent.name}) index={insertIndex}");
            }
            //Debug.Log($"debug - {(lowerChild != null ? lowerChild.name : "(null)")}");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SiblingOrderViewObjectCompare : IComparer<IViewObject>
    {
        public int Compare(IViewObject left, IViewObject right)
        {
            var leftOrder = (modelOrder: left.GetModelSiblingOrder(), viewOrder: left.GetViewObjSiblingOrder());
            var rightOrder = (modelOrder: right.GetModelSiblingOrder(), viewOrder: right.GetViewObjSiblingOrder());

            if (leftOrder.modelOrder == ISiblingOrderConst.INVALID_ORDER
                && rightOrder.modelOrder == ISiblingOrderConst.INVALID_ORDER)
            {
                return CompareViewVsView(leftOrder, rightOrder);
            }
            else if(leftOrder.modelOrder == ISiblingOrderConst.INVALID_ORDER
                && rightOrder.modelOrder != ISiblingOrderConst.INVALID_ORDER)
            {
                return CompareViewVsModel(leftOrder, rightOrder);
            }
            else if(leftOrder.modelOrder != ISiblingOrderConst.INVALID_ORDER
                && rightOrder.modelOrder == ISiblingOrderConst.INVALID_ORDER)
            {
                return CompareModelVsView(leftOrder, rightOrder);
            }
            else
            {
                return CompareModelVsModel(leftOrder, rightOrder);
            }
        }

        int CompareModelVsModel((uint modelOrder, uint viewOrder) left, (uint modelOrder, uint viewOrder) right)
        {
            if (left.modelOrder == right.modelOrder)
            {
                return CompareViewVsView(left, right);
            }
            else
            {
                return left.modelOrder.CompareTo(right.modelOrder) * -1;
            }
        }

        int CompareModelVsView((uint modelOrder, uint viewOrder) left, (uint modelOrder, uint viewOrder) right)
        {
            if (right.viewOrder == ISiblingOrderConst.INVALID_ORDER) return -1;

            if(left.modelOrder == right.viewOrder)
            {
                //Model側が優先
                return -1;
            }
            else
            {
                return left.modelOrder.CompareTo(right.viewOrder) * -1;
            }
        }

        int CompareViewVsModel((uint modelOrder, uint viewOrder) left, (uint modelOrder, uint viewOrder) right)
        {
            return CompareModelVsView(right, left) * -1;
        }

        int CompareViewVsView((uint modelOrder, uint viewOrder) left, (uint modelOrder, uint viewOrder) right)
        {
            if (left.viewOrder == right.viewOrder) return 0;

            if (left.viewOrder == ISiblingOrderConst.INVALID_ORDER) return 1;
            if (right.viewOrder == ISiblingOrderConst.INVALID_ORDER) return -1;
            return left.viewOrder.CompareTo(right.viewOrder) * -1;
        }

    }

    public static partial class IViewObjectExtensions
    {
        public static uint GetModelSiblingOrder(this IViewObject target)
        {
            if(target.UseModel != null && target.UseModel is ISiblingOrder)
            {
                return (target.UseModel as ISiblingOrder).SiblingOrder;
            }
            else
            {
                return ISiblingOrderConst.INVALID_ORDER;
            }
        }

        public static uint GetViewObjSiblingOrder(this IViewObject target)
        {
            if (target.UseBindInfo != null && target.UseBindInfo.HasViewLayoutValue(BasicViewLayoutName.siblingOrder))
            {
                var value = target.UseBindInfo.GetViewLayoutValue(BasicViewLayoutName.siblingOrder);
                if (value is uint) return (uint)value;
                if (value is short) return (uint)(short)value;
                if (value is int) return (uint)(int)value;
                if (value is long) return (uint)(long)value;
                if (value is ushort) return (uint)(ushort)value;
                if (value is uint) return (uint)(uint)value;
                if (value is ulong) return (uint)(ulong)value;
                throw new System.InvalidCastException($"value Type={value.GetType()}");
            }
            else
            {
                return ISiblingOrderConst.INVALID_ORDER;
            }

        }

    }
}

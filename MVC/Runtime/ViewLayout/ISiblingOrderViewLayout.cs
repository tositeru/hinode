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

    public static partial class ISiblingOrderViewLayoutExtensions
    {
        public static string ToString(this ISiblingOrderViewLayout target)
        {
            var orderStr = target.SiblingOrder == ISiblingOrderConst.INVALID_ORDER
                ? "-1"
                : target.SiblingOrder.ToString();
            return $"SiblingOrder={orderStr}";
        }
    }

    public class SiblingOrderViewLayoutAccessor : IViewLayoutAccessor
    {
        public override System.Type ViewLayoutType { get => typeof(ISiblingOrderViewLayout); }
        public override System.Type ValueType { get => typeof(uint); }
        public override ViewLayoutAccessorUpdateTiming UpdateTiming { get => ViewLayoutAccessorUpdateTiming.AtOnlyModel; }

        protected override object GetImpl(object viewLayoutObj)
            => (viewLayoutObj as ISiblingOrderViewLayout).SiblingOrder;

        protected override void SetImpl(object value, object viewLayoutObj)
        {
            var layout = (viewLayoutObj as ISiblingOrderViewLayout);
            layout.SiblingOrder = (uint)value;

            if (viewLayoutObj is MonoBehaviour)
            {
                var behaviour = viewLayoutObj as MonoBehaviour;
                if (behaviour.transform.parent != null)
                {
                    SiblingOrderViewLayoutAccessor.Insert(behaviour.transform.parent, viewLayoutObj);
                }
            }
        }

        public static void Insert(Transform parent, object target)
        {
            IViewObject viewObject = IViewLayoutAccessor.GetViewObject(target);
            Insert(parent, viewObject);
        }

        public static void Insert(Transform parent, IViewObject target)
        {
            if (!(target is MonoBehaviour)) return;

            if (!ContainsSibilingOrder(target))
            {
                return;
            }
            var t = (target as MonoBehaviour).transform;
            if (t.parent != parent)
            {
                Debug.Log($"not match parent... targetParent={t.parent} parent={parent}");
                return;
            }

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

            var insertIndex = (lowerChild != null)
                ? (lowerChild.GetSiblingIndex() - 1)
                : parent.childCount - 1;
            insertIndex = Mathf.Max(0, insertIndex);

            t.SetSiblingIndex(insertIndex);

            Logger.Log(Logger.Priority.Debug, () => {
                return $"{target} insert to parent({parent.name}) index={t.GetSiblingIndex()}, {target.ToSiblingOrderString()}";
            });
        }

        static bool ContainsSibilingOrder(IViewObject target)
        {
            return target.UseModel is ISiblingOrder
                || (target.GetViewLayoutState()?.ContainsKey(BasicViewLayoutName.siblingOrder) ?? false);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class SiblingOrderViewObjectCompare : IComparer<IViewObject>
    {
        public int Compare(IViewObject left, IViewObject right)
        {
            if (left == right) return 0;

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
            if (target.GetViewLayoutState()?.ContainsKey(BasicViewLayoutName.siblingOrder) ?? false)
            {
                var value = target.GetViewLayoutState().GetValue(BasicViewLayoutName.siblingOrder);
                if (value is uint) return (uint)value;
                if (value is short) return (uint)(short)value;
                if (value is int) return (uint)(int)value;
                if (value is long) return (uint)(long)value;
                if (value is ushort) return (uint)(ushort)value;
                if (value is uint) return (uint)(uint)value;
                if (value is ulong) return (uint)(ulong)value;
                throw new System.InvalidCastException($"value Type={value.GetType()}");
            }
            else if(target is ISiblingOrderViewLayout)
            {
                return (target as ISiblingOrderViewLayout).SiblingOrder;
            }
            else
            {
                return ISiblingOrderConst.INVALID_ORDER;
            }

        }

        public static string ToSiblingOrderString(this IViewObject target)
        {
            var modelOrder = target.GetModelSiblingOrder();
            var viewOrder = target.GetViewObjSiblingOrder();

            var modelOrderStr = modelOrder == ISiblingOrderConst.INVALID_ORDER
                ? "-1"
                : modelOrder.ToString();
            var viewOrderStr = viewOrder == ISiblingOrderConst.INVALID_ORDER
                ? "-1"
                : viewOrder.ToString();
            return $"SiblingOrder=[model={modelOrderStr},viewObj={viewOrderStr}]";
        }

    }
}
